using System.Collections.Concurrent;
using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public class InMemoryCanvasStateStore : ICanvasStateStore
{
    private class State
    {
        public long Epoch { get; set; } = 1;
        public long Revision { get; set; }
        public List<string> Order { get; } = new();
        public Dictionary<string, CanvasStrokeSnapshot> Strokes { get; } = new();
        public Dictionary<string, int> LastSequence { get; } = new();
    }

    private readonly ConcurrentDictionary<string, State> _states = new();
    private readonly CanvasOptions _options;

    public InMemoryCanvasStateStore(CanvasOptions? options = null)
    {
        _options = options ?? CanvasTestOptions.Create();
    }

    public Task<CanvasStoreAppendResult> AppendChunkAsync(
        string gameHash,
        CanvasStrokeChunk chunk,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State state = _states.GetOrAdd(gameHash, _ => new State { Epoch = chunk.Epoch });
        lock (state)
        {
            if (state.Epoch != chunk.Epoch)
            {
                return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.WrongEpoch, state.Revision));
            }
            int last = state.LastSequence.TryGetValue(chunk.StrokeId, out int value) ? value : -1;
            if (chunk.Sequence == last)
            {
                return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.Duplicate, state.Revision));
            }
            if (chunk.Sequence != last + 1)
            {
                return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.OutOfOrder, state.Revision));
            }
            if (!state.Strokes.TryGetValue(chunk.StrokeId, out CanvasStrokeSnapshot? stroke))
            {
                if (state.Order.Count >= _options.MaxStrokes)
                {
                    return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.LimitExceeded, state.Revision));
                }
                state.Order.Add(chunk.StrokeId);
                stroke = new CanvasStrokeSnapshot(chunk.StrokeId, chunk.Color, chunk.Thickness, Array.Empty<ushort>());
            }
            ushort[] combined = stroke.Coordinates.Concat(chunk.Coordinates).ToArray();
            if (combined.Length / 2 > _options.MaxPointsPerStroke)
            {
                return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.LimitExceeded, state.Revision));
            }
            state.Strokes[chunk.StrokeId] = stroke with
            {
                Coordinates = combined
            };
            state.LastSequence[chunk.StrokeId] = chunk.Sequence;
            return Task.FromResult(new CanvasStoreAppendResult(CanvasStoreAppendStatus.Applied, ++state.Revision));
        }
    }

    public Task<CanvasSnapshot> GetSnapshotAsync(
        string gameHash,
        long expectedEpoch,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_states.TryGetValue(gameHash, out State? state))
        {
            return Task.FromResult(new CanvasSnapshot(expectedEpoch, 0, Array.Empty<CanvasStrokeSnapshot>()));
        }
        lock (state)
        {
            return Task.FromResult(new CanvasSnapshot(state.Epoch, state.Revision,
                state.Order.Where(state.Strokes.ContainsKey).Select(id => state.Strokes[id]).ToArray()));
        }
    }

    public Task<CanvasStoreUndoResult> UndoAsync(
        string gameHash,
        long expectedEpoch,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_states.TryGetValue(gameHash, out State? state))
        {
            return Task.FromResult(new CanvasStoreUndoResult(false));
        }
        lock (state)
        {
            if (state.Epoch != expectedEpoch || state.Order.Count == 0)
            {
                return Task.FromResult(new CanvasStoreUndoResult(false, Revision: state.Revision));
            }
            string id = state.Order[^1];
            state.Order.RemoveAt(state.Order.Count - 1);
            state.Strokes.Remove(id);
            state.LastSequence.Remove(id);
            return Task.FromResult(new CanvasStoreUndoResult(true, id, ++state.Revision));
        }
    }

    public Task ClearAsync(string gameHash, long newEpoch, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _states[gameHash] = new State { Epoch = newEpoch };
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string gameHash, CancellationToken cancellationToken = default)
    {
        _states.TryRemove(gameHash, out _);
        return Task.CompletedTask;
    }
}
