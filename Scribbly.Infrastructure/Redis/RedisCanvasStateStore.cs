using System.Buffers.Binary;
using Microsoft.Extensions.Options;
using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;
using StackExchange.Redis;

namespace Scribbly.Infrastructure.Redis;

public class RedisCanvasStateStore : ICanvasStateStore
{
    private const string AppendScript = """
        local expectedEpoch = tonumber(ARGV[1])
        local currentEpoch = tonumber(redis.call('get', KEYS[1]) or '0')
        if currentEpoch == 0 or currentEpoch < expectedEpoch then
            redis.call('del', KEYS[2], KEYS[3], KEYS[4], KEYS[5], KEYS[6])
            redis.call('set', KEYS[1], expectedEpoch)
            redis.call('set', KEYS[7], 0)
            currentEpoch = expectedEpoch
        end
        if currentEpoch ~= expectedEpoch then
            return {-2, tonumber(redis.call('get', KEYS[7]) or '0')}
        end

        local seq = tonumber(ARGV[2])
        local points = tonumber(ARGV[3])
        local maxPerStroke = tonumber(ARGV[4])
        local maxStrokes = tonumber(ARGV[5])
        local maxTotal = tonumber(ARGV[6])
        local strokeId = ARGV[7]
        local color = ARGV[8]
        local thickness = ARGV[9]
        local ttl = tonumber(ARGV[10])
        local field = strokeId .. ':' .. seq

        if redis.call('sismember', KEYS[5], strokeId) == 1 then
            return {-5, tonumber(redis.call('get', KEYS[7]) or '0')}
        end

        local metadata = redis.call('hget', KEYS[3], strokeId)
        local previousPoints = 0
        if not metadata then
            if seq ~= 0 then return {-3, tonumber(redis.call('get', KEYS[7]) or '0')} end
            if redis.call('llen', KEYS[2]) >= maxStrokes then return {-4, tonumber(redis.call('get', KEYS[7]) or '0')} end
            redis.call('rpush', KEYS[2], strokeId)
        else
            local lastSeq, pointCount, storedColor, storedThickness = string.match(metadata, '^(%-?%d+)|(%d+)|([^|]+)|(%d+)$')
            if not lastSeq then return {-5, tonumber(redis.call('get', KEYS[7]) or '0')} end
            lastSeq = tonumber(lastSeq)
            previousPoints = tonumber(pointCount)
            if seq == lastSeq and redis.call('hexists', KEYS[6], field) == 1 then
                return {0, tonumber(redis.call('get', KEYS[7]) or '0')}
            end
            if seq ~= lastSeq + 1 then return {-3, tonumber(redis.call('get', KEYS[7]) or '0')} end
            if storedColor ~= color or tonumber(storedThickness) ~= tonumber(thickness) then
                return {-5, tonumber(redis.call('get', KEYS[7]) or '0')}
            end
        end

        local newStrokePoints = previousPoints + points
        local total = tonumber(redis.call('get', KEYS[4]) or '0')
        if newStrokePoints > maxPerStroke or total + points > maxTotal then
            if not metadata then redis.call('rpop', KEYS[2]) end
            return {-4, tonumber(redis.call('get', KEYS[7]) or '0')}
        end

        redis.call('hset', KEYS[6], field, ARGV[11])
        redis.call('hset', KEYS[3], strokeId, seq .. '|' .. newStrokePoints .. '|' .. color .. '|' .. thickness)
        redis.call('incrby', KEYS[4], points)
        local revision = redis.call('incr', KEYS[7])
        for _, key in ipairs(KEYS) do redis.call('pexpire', key, ttl) end
        return {1, revision}
        """;

    private const string UndoScript = """
        local expectedEpoch = tonumber(ARGV[1])
        if tonumber(redis.call('get', KEYS[1]) or '0') ~= expectedEpoch then
            return {0, tonumber(redis.call('get', KEYS[7]) or '0'), ''}
        end
        local strokeId = redis.call('rpop', KEYS[2])
        if not strokeId then return {0, tonumber(redis.call('get', KEYS[7]) or '0'), ''} end
        local metadata = redis.call('hget', KEYS[3], strokeId)
        if not metadata then return {0, tonumber(redis.call('get', KEYS[7]) or '0'), ''} end
        local lastSeq, pointCount = string.match(metadata, '^(%-?%d+)|(%d+)|')
        lastSeq = tonumber(lastSeq)
        pointCount = tonumber(pointCount)
        for i = 0, lastSeq do redis.call('hdel', KEYS[6], strokeId .. ':' .. i) end
        redis.call('hdel', KEYS[3], strokeId)
        redis.call('sadd', KEYS[5], strokeId)
        redis.call('decrby', KEYS[4], pointCount)
        local revision = redis.call('incr', KEYS[7])
        local ttl = tonumber(ARGV[2])
        for _, key in ipairs(KEYS) do redis.call('pexpire', key, ttl) end
        return {1, revision, strokeId}
        """;

    private const string ClearScript = """
        redis.call('del', KEYS[2], KEYS[3], KEYS[4], KEYS[5], KEYS[6])
        redis.call('set', KEYS[1], ARGV[1], 'PX', ARGV[2])
        redis.call('set', KEYS[7], 0, 'PX', ARGV[2])
        return 1
        """;

    private readonly IDatabase _database;
    private readonly RedisOptions _options;
    private readonly CanvasOptions _canvasOptions;

    public RedisCanvasStateStore(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options,
        CanvasOptions canvasOptions)
    {
        _database = connection.GetDatabase();
        _options = options.Value;
        _canvasOptions = canvasOptions;
    }

    public async Task<CanvasStoreAppendResult> AppendChunkAsync(
        string gameHash,
        CanvasStrokeChunk chunk,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisResult result = await _database.ScriptEvaluateAsync(AppendScript, Keys(gameHash), new RedisValue[]
        {
            chunk.Epoch, chunk.Sequence, chunk.Coordinates.Length / 2,
            _canvasOptions.MaxPointsPerStroke, _canvasOptions.MaxStrokes, _canvasOptions.MaxTotalPoints,
            chunk.StrokeId, chunk.Color, chunk.Thickness, (long)Ttl.TotalMilliseconds, Pack(chunk.Coordinates)
        });
        RedisResult[] values = (RedisResult[])result!;
        long code = (long)values[0];
        long revision = (long)values[1];
        return new(code switch
        {
            1 => CanvasStoreAppendStatus.Applied,
            0 => CanvasStoreAppendStatus.Duplicate,
            -2 => CanvasStoreAppendStatus.WrongEpoch,
            -3 => CanvasStoreAppendStatus.OutOfOrder,
            -4 => CanvasStoreAppendStatus.LimitExceeded,
            _ => CanvasStoreAppendStatus.InvalidStroke
        }, revision);
    }

    public async Task<CanvasSnapshot> GetSnapshotAsync(
        string gameHash,
        long expectedEpoch,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue[] header = await _database.StringGetAsync(new[] { EpochKey(gameHash), RevisionKey(gameHash) });
        long epoch = header[0].TryParse(out long parsedEpoch) ? parsedEpoch : expectedEpoch;
        long revision = header[1].TryParse(out long parsedRevision) ? parsedRevision : 0;
        if (epoch != expectedEpoch)
        {
            return new(expectedEpoch, revision, Array.Empty<CanvasStrokeSnapshot>());
        }

        RedisValue[] order = await _database.ListRangeAsync(OrderKey(gameHash));
        if (order.Length == 0)
        {
            return new(expectedEpoch, revision, Array.Empty<CanvasStrokeSnapshot>());
        }

        RedisValue[] metadata = await _database.HashGetAsync(MetaKey(gameHash), order);
        var descriptors = new List<(string Id, int LastSequence, string Color, int Thickness)>();
        var chunkFields = new List<RedisValue>();
        for (int i = 0; i < order.Length; i++)
        {
            string id = order[i].ToString();
            if (!TryParseMetadata(metadata[i].ToString(), out int lastSequence, out _, out string color, out int thickness))
            {
                continue;
            }
            descriptors.Add((id, lastSequence, color, thickness));
            for (int sequence = 0; sequence <= lastSequence; sequence++)
            {
                chunkFields.Add($"{id}:{sequence}");
            }
        }

        RedisValue[] chunks = chunkFields.Count == 0 ? Array.Empty<RedisValue>() : await _database.HashGetAsync(ChunksKey(gameHash), chunkFields.ToArray());
        int chunkIndex = 0;
        var strokes = new List<CanvasStrokeSnapshot>(descriptors.Count);
        foreach (var descriptor in descriptors)
        {
            var coordinates = new List<ushort>();
            for (int sequence = 0; sequence <= descriptor.LastSequence; sequence++)
            {
                if (chunkIndex >= chunks.Length)
                {
                    break;
                }
                RedisValue value = chunks[chunkIndex++];
                if (!value.IsNullOrEmpty)
                {
                    coordinates.AddRange(Unpack((byte[])value!));
                }
            }
            strokes.Add(new(descriptor.Id, descriptor.Color, descriptor.Thickness, coordinates.ToArray()));
        }
        return new(expectedEpoch, revision, strokes);
    }

    public async Task<CanvasStoreUndoResult> UndoAsync(
        string gameHash,
        long expectedEpoch,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisResult result = await _database.ScriptEvaluateAsync(UndoScript, Keys(gameHash), new RedisValue[] { expectedEpoch, (long)Ttl.TotalMilliseconds });
        RedisResult[] values = (RedisResult[])result!;
        bool applied = (long)values[0] == 1;
        return new(applied, applied ? values[2].ToString() : null, (long)values[1]);
    }

    public async Task ClearAsync(string gameHash, long newEpoch, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.ScriptEvaluateAsync(ClearScript, Keys(gameHash), new RedisValue[] { newEpoch, (long)Ttl.TotalMilliseconds });
    }

    public async Task DeleteAsync(string gameHash, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.KeyDeleteAsync(Keys(gameHash));
    }

    private RedisKey[] Keys(string gameHash) => new[]
    {
        EpochKey(gameHash), OrderKey(gameHash), MetaKey(gameHash), TotalKey(gameHash),
        TombstonesKey(gameHash), ChunksKey(gameHash), RevisionKey(gameHash)
    };

    private string Base(string gameHash) => $"{_options.KeyPrefix}:canvas:{{{gameHash}}}";
    private RedisKey EpochKey(string gameHash) => $"{Base(gameHash)}:epoch";
    private RedisKey OrderKey(string gameHash) => $"{Base(gameHash)}:order";
    private RedisKey MetaKey(string gameHash) => $"{Base(gameHash)}:meta";
    private RedisKey TotalKey(string gameHash) => $"{Base(gameHash)}:total";
    private RedisKey TombstonesKey(string gameHash) => $"{Base(gameHash)}:tombstones";
    private RedisKey ChunksKey(string gameHash) => $"{Base(gameHash)}:chunks";
    private RedisKey RevisionKey(string gameHash) => $"{Base(gameHash)}:revision";
    private TimeSpan Ttl => TimeSpan.FromHours(Math.Max(1, _options.GameTtlHours));

    private static byte[] Pack(ushort[] coordinates)
    {
        byte[] bytes = new byte[coordinates.Length * 2];
        for (int i = 0; i < coordinates.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(i * 2, 2), coordinates[i]);
        }

        return bytes;
    }

    private static ushort[] Unpack(byte[] bytes)
    {
        if (bytes.Length % 2 != 0)
        {
            return Array.Empty<ushort>();
        }
        ushort[] values = new ushort[bytes.Length / 2];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(i * 2, 2));
        }

        return values;
    }

    private static bool TryParseMetadata(
        string value,
        out int lastSequence,
        out int pointCount,
        out string color,
        out int thickness)
    {
        lastSequence = pointCount = thickness = 0;
        color = string.Empty;
        string[] parts = value.Split('|');
        return parts.Length == 4 && int.TryParse(parts[0], out lastSequence) && int.TryParse(parts[1], out pointCount) &&
               int.TryParse(parts[3], out thickness) && !string.IsNullOrWhiteSpace(color = parts[2]);
    }
}
