using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository;

public interface IFineRepository : IRepository<Fine>
{
    void Update(Fine obj);
    void AddRange(List<Fine> fines);
    
}