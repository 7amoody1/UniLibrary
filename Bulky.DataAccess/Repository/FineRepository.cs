using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository;

public class FineRepository : Repository<Fine>, IFineRepository
{
    private readonly ApplicationDbContext _db;
    
    public FineRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Fine obj)
    {
        _db.Update(obj);
    }

    public void AddRange(List<Fine> fines)
    {
        _db.AddRange(fines);
    }
}