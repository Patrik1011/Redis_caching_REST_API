using Redis_caching.Models;

namespace Redis_caching.Services;

public interface ICachingService
{
    T GetData<T>(string key);
    
    bool SetData<T>(string key, T data, DateTimeOffset expiration);
    
    object RemoveData(string key);
    
    //get a customer by id
    Customer GetCustomerById(int id);
}