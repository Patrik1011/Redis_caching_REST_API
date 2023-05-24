using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Redis_caching.Models;

public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    //public string? City { get; set; }
    //public string? Country { get; set; }
    //public string? PostalCode { get; set; }
    //public string? Phone { get; set; }
    //public string? Email { get; set; }
}