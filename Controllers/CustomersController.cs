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
    private readonly ILogger<CustomersController> _logger;
    private readonly ICachingService _cachingService;
    private readonly RestDbContext _dbContext;

    public CustomersController(ILogger<CustomersController> logger, ICachingService cachingService, RestDbContext dbContext)
    {
        _logger = logger;
        _cachingService = cachingService;
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cacheData = _cachingService.GetData<IEnumerable<Customer>>("customers");
        
        if (cacheData != null && cacheData.Count() > 0)
        {
            return Ok(cacheData);
        }
        
        cacheData = await _dbContext.Customers.ToListAsync();
        _cachingService.SetData<IEnumerable<Customer>>("customers", cacheData, DateTimeOffset.Now.AddSeconds(30));
        return Ok(cacheData);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Customer customer)
    {
        var createdCustomer = await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();
        //set data in cache
        _cachingService.SetData<Customer>($"customer{ customer.Id }", createdCustomer.Entity, DateTimeOffset.Now.AddSeconds(30));
        return Ok(customer);
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete (int customerId)
    {
        //var customer = await _dbContext.Customers.FindAsync(customerId);
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId);
        if (customer == null)
        {
            return NotFound();
        }
        //remove data from database
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        //remove data from cache
        _cachingService.RemoveData($"customer{ customerId }");
        return Ok(customer);
    }
}