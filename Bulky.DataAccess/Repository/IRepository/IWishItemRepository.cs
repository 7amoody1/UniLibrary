using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository;

public interface IWishItemRepository : IRepository<WishItem>
{
    void Update(WishItem waitList);
    List<WishItem> GetByProductId(int? productId = null, string? userId = null, bool includeNotified = false);
}
