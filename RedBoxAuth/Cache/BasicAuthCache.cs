using MemoryPack;
using Shared.Models;
using StackExchange.Redis;
using ZstdSharp;

namespace RedBoxAuth.Cache;

/// <inheritdoc />
public class BasicAuthCache : IBasicAuthCache
{
    private readonly IDatabase _redis;

    /// <summary>
    ///     Constructor for dependency injector container
    /// </summary>
    /// <param name="redis">Redis connection</param>
    public BasicAuthCache(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    /// <inheritdoc />
    public bool TryToGet(string? token, out User? user)
    {
        if (!_redis.KeyExists(token))
        {
            user = null;
            return false;
        }

        var stream = (byte[]?)_redis.StringGet(token);

        using var decompressor = new Decompressor();
        user = MemoryPackSerializer.Deserialize<User>(decompressor.Unwrap(stream));

        return true;
    }
}