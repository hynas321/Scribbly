namespace Scribbly.Infrastructure.Redis;

internal static class RedisGameStateScripts
{
    public const string CreateScript = """
        if redis.call('exists', KEYS[1]) == 1 then
            return 0
        end
        local previousConnections = redis.call('smembers', KEYS[3])
        for _, connectionId in ipairs(previousConnections) do
            local connectionKey = ARGV[4] .. ':connection:' .. connectionId .. ':game'
            if redis.call('get', connectionKey) == ARGV[3] then
                redis.call('del', connectionKey)
            end
        end
        redis.call('del', KEYS[3])
        redis.call('set', KEYS[1], ARGV[1], 'PX', ARGV[2])
        redis.call('sadd', KEYS[2], ARGV[3])
        for index = 5, #ARGV do
            local connectionId = ARGV[index]
            local connectionKey = ARGV[4] .. ':connection:' .. connectionId .. ':game'
            local previousGame = redis.call('get', connectionKey)
            if previousGame and previousGame ~= ARGV[3] then
                redis.call('srem', ARGV[4] .. ':game:' .. previousGame .. ':connections', connectionId)
            end
            redis.call('set', connectionKey, ARGV[3], 'PX', ARGV[2])
            redis.call('sadd', KEYS[3], connectionId)
        end
        if #ARGV >= 5 then
            redis.call('pexpire', KEYS[3], ARGV[2])
        end
        return 1
        """;

    public const string PersistScript = """
        if redis.call('get', KEYS[3]) ~= ARGV[5] or
           redis.call('get', KEYS[1]) ~= ARGV[6] or
           (ARGV[7] ~= '' and redis.call('get', KEYS[4]) ~= ARGV[7]) then
            return -1
        end
        local previousConnections = redis.call('smembers', KEYS[2])
        for _, connectionId in ipairs(previousConnections) do
            local connectionKey = ARGV[4] .. ':connection:' .. connectionId .. ':game'
            if redis.call('get', connectionKey) == ARGV[3] then
                redis.call('del', connectionKey)
            end
        end
        redis.call('del', KEYS[2])
        redis.call('set', KEYS[1], ARGV[1], 'PX', ARGV[2])
        for index = 8, #ARGV do
            local connectionId = ARGV[index]
            local connectionKey = ARGV[4] .. ':connection:' .. connectionId .. ':game'
            local previousGame = redis.call('get', connectionKey)
            if previousGame and previousGame ~= ARGV[3] then
                redis.call('srem', ARGV[4] .. ':game:' .. previousGame .. ':connections', connectionId)
            end
            redis.call('set', connectionKey, ARGV[3], 'PX', ARGV[2])
            redis.call('sadd', KEYS[2], connectionId)
        end
        if #ARGV >= 8 then
            redis.call('pexpire', KEYS[2], ARGV[2])
        end
        return 1
        """;

    public const string DeleteScript = """
        if redis.call('get', KEYS[4]) ~= ARGV[3] then
            return -1
        end
        if ARGV[4] ~= '' and redis.call('get', KEYS[1]) ~= ARGV[4] then
            return -1
        end
        if ARGV[5] ~= '' and redis.call('get', KEYS[5]) ~= ARGV[5] then
            return -1
        end
        local connections = redis.call('smembers', KEYS[3])
        for _, connectionId in ipairs(connections) do
            local connectionKey = ARGV[2] .. ':connection:' .. connectionId .. ':game'
            if redis.call('get', connectionKey) == ARGV[1] then
                redis.call('del', connectionKey)
            end
        end
        redis.call('del', KEYS[3])
        local removed = redis.call('del', KEYS[1])
        redis.call('srem', KEYS[2], ARGV[1])
        return removed
        """;
}
