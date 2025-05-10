using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository;

public class WishItemRepository : Repository<WishItem>, IWishItemRepository
{
    private readonly ApplicationDbContext _db;

    public WishItemRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(WishItem waitList)
    {
        _db.WishItems.Update(waitList);
    }

    public List<WishItem> GetByProductId(int? productId = null, string? userId = null, bool includeNotified = false)
    {
        var query = _db.WishItems.AsQueryable();
        
        if (productId is not null)
        {
            query = query.Where(w => w.ProductId == productId);
        }

        if (userId is not null)
        {
            query = query.Where(w => w.ApplicationUserId == userId);
        }

        if (!includeNotified)
        {
            query = query.Where(w => !w.IsNotified);
        }
        return query
            .Include(w => w.ApplicationUser)
            .Include(w => w.Product)
            .ToList();
        
    }
}