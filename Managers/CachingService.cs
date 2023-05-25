using System.Text.Json;
using Redis_caching.Models;
using StackExchange.Redis;

namespace Redis_caching.Services;

public class CachingService: ICachingService
{
    
    private IDatabase _cacheRedisDb;
    
    public CachingService()
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        _cacheRedisDb = redis.GetDatabase();
    }
    
    public T GetData<T>(string key)
    {
        var data = _cacheRedisDb.StringGet(key);
        if (data.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(data);
    }
    
    public bool SetData<T>(string key, T data, DateTimeOffset expiration)
    {
        var expirationTime = expiration.DateTime.Subtract(DateTime.Now);
        var serializedData = JsonSerializer.Serialize(data);
        return _cacheRedisDb.StringSet(key, serializedData, expirationTime);
        //return _cacheRedisDb.StringSet(key, serializedData, expiration - DateTimeOffset.Now);
    }
    
    /*
    public object RemoveData(string key)
    {
        var dataExists = _cacheRedisDb.KeyExists(key);
        if (dataExists)
        {
            return _cacheRedisDb.KeyDelete(key);
        }
        return false;
    }
    */
}