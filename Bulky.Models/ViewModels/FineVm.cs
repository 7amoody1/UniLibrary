namespace BulkyBook.Models.ViewModels;

public class FineVm
{
    public required List<Fine> Fines { get; set; }
    public required OrderHeader OrderHeader { get; set; }
    public required int? FineId { get; set; }
}