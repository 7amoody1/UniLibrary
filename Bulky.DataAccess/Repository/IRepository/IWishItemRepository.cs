using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository;

public interface IWishItemRepository : IRepository<WishItem>
{
    void Update(WishItem wishItem);
}