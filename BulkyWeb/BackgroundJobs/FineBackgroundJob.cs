using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;

namespace BulkyBookWeb.BackgroundJobs;

public class FineBackgroundJob
{
    private readonly IUnitOfWork _unitOfWork;

    public FineBackgroundJob(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void CheckDelayReturns()
    {
        var today = DateTime.Now.Date; // Normalize to midnight for consistency

        // Fetch all delayed users in one query
        var ordersOfDelayedUsers = _unitOfWork.OrderDetail
            .GetAll(o => o.Type == SD.Borrow && !o.IsReturned && today > o.EndBorrowDate,
                includeProperties: "OrderHeader.ApplicationUser,Product")
            .ToList();

        var existingFines = _unitOfWork.Fine.GetAll(
            x => x.Status == SD.PendingFine, tracked: true).ToList();
            
        var payedFines = _unitOfWork.Fine.GetAll(
            x => x.Status == SD.PayedFine, tracked: true).ToList();

        var finesToAdd = new List<Fine>();
        
        foreach (var orderDetail in ordersOfDelayedUsers)
        {
            var userId = orderDetail.OrderHeader.ApplicationUserId;
            var productId = orderDetail.ProductId;

            var currentFines = existingFines.Where(x => x.OrderDetailsId == orderDetail.Id).ToList();
            
            if (currentFines.Any())
            {
                foreach (var currentFine in currentFines)
                {
                    currentFine.Amount += 0.250; // Update existing fine
                }

                continue;
            }

            var oldFines = payedFines.FirstOrDefault(x => x.OrderDetailsId == orderDetail.Id);

            if (oldFines is not null)
            {
                oldFines.Amount = 0.250;
                oldFines.Status = SD.PendingFine;
                continue;
            }
            
            // Calculate fine amount based on full timespan
            var daysLate = (today - orderDetail.EndBorrowDate!.Value).TotalDays;
            var amount = daysLate * 0.250;

            // Create new fine
            finesToAdd.Add(new Fine
            {
                ApplicationUserId = userId,
                ProductId = productId,
                Amount = amount,
                IssuedDate = today,
                Type = SD.Delay,
                OrderDetailsId = orderDetail.Id
            });
        }

        // Batch insert new fines
        if (finesToAdd.Any())
        {
            _unitOfWork.Fine.AddRange(finesToAdd);
        }

        _unitOfWork.Save();
    }
}