using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }

        public List<OrderDetail> GetOrderDetailsWithHeaders(string? userId = null)
        {
            List<OrderDetail> orderDetails;
            if (userId is null)
            {
                orderDetails = _db.OrderDetails
                .Include(x => x.Product)
                .Include(x => x.OrderHeader)
                .ThenInclude(x => x.ApplicationUser)
                .ToList();
            }
            else
            {
                orderDetails = _db.OrderDetails.Include(x => x.OrderHeader)
                    .Include(x => x.Product)
                    .Include(x => x.OrderHeader)
                    .Where(x => x.OrderHeader.ApplicationUserId == userId)
                    .ToList();
            }

            return orderDetails;
        }
    }
}
