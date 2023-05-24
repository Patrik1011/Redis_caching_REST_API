using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Redis_caching.Managers;
using Redis_caching.Models;
using Redis_caching.Services;

namespace Redis_caching.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICachingService _cachingService;
    private readonly RestDbContext _dbContext;

    public CustomersController(ICachingService cachingService, RestDbContext dbContext)
    {
        _cachingService = cachingService;
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var cacheData = _cachingService.GetData<IEnumerable<Customer>>("customers");

        if (cacheData == null || !cacheData.Any())
        {
            cacheData = await _dbContext.Customers.ToListAsync();
            _cachingService.SetData<IEnumerable<Customer>>("customers", cacheData, DateTimeOffset.Now.AddSeconds(30));
        }

        return Ok(cacheData);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] Customer customer)
    {
        try
        {
            // Add data to the database
            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            // Retrieve data from the cache
            var cacheData = _cachingService.GetData<IEnumerable<Customer>>("customers");

            if (cacheData == null || !cacheData.Any())
            {
                // If cache is empty, fetch data from the database
                cacheData = await _dbContext.Customers.ToListAsync();
                _cachingService.SetData<IEnumerable<Customer>>("customers", cacheData, DateTimeOffset.Now.AddSeconds(30));
            }
            else
            {
                // Add the newly created customer to the existing cache
                var updatedCacheData = cacheData.ToList();
                updatedCacheData.Add(customer);
                _cachingService.SetData<IEnumerable<Customer>>("customers", updatedCacheData, DateTimeOffset.Now.AddSeconds(30));
                cacheData = updatedCacheData;
            }
            return Ok(cacheData);
        }

        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete (int customerId)
    {
        // Try to delete the customer from the cache first
        var cacheData = _cachingService.GetData<List<Customer>>("customers");
        if (cacheData != null && cacheData.RemoveAll(c => c.Id == customerId) > 0)
        {
            _cachingService.SetData("customers", cacheData, DateTimeOffset.Now.AddSeconds(30));
            _dbContext.Customers.Remove(new Customer { Id = customerId }); 
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        // If not found in cache, find in database
        var customer = await _dbContext.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return NotFound();
        }

        // Update the cache after deleting the customer
        //if cacheData is null, then set it to an empty list
        if(cacheData == null) cacheData = new List<Customer>();
        cacheData.RemoveAll(c => c.Id == customerId);
        _cachingService.SetData("customers", cacheData, DateTimeOffset.Now.AddSeconds(30));

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();

        return Ok(cacheData);

    }
}