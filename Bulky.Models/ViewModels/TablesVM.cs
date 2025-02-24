namespace BulkyBook.Models.ViewModels;

public class TablesVM
{
    public List<Fine> FinesList { get; set; }
    public List<OrderHeader> OrderHeadersList { get; set; }
    public List<WishItem> WishItemsList { get; set; }

    public string RequestedData { get; set; }
}