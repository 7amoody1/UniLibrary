using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models;

public class WishItem
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public int ProductId { get; set; } 
    public Product Product { get; set; } = null!;
    public DateTime EnrolledDate { get; set; } 
    public bool IsNotified { get; set; } 
}