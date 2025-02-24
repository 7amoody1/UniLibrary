using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository;

public class WishItemRepository : Repository<WishItem>, IWishItemRepository
{
    private readonly ApplicationDbContext _db;

    public WishItemRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(WishItem wishItem)
    {
        _db.WishItems.Update(wishItem);
    }
}