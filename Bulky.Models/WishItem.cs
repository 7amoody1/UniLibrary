using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models;

public class WishItem
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser? ApplicationUser { get; set; }
}