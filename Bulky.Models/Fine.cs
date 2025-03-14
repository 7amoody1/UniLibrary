using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models;

public class Fine
{
    public int Id { get; set; }

    public string ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser? ApplicationUser { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public double Amount { get; set; } // 0.250
    public DateTime IssuedDate { get; set; } = DateTime.Now;

    public string Type { get; set; }  // delay, corrupted 

    public string Status { get; set; } = "pending";
    public int OrderDetailsId { get; set; }
}