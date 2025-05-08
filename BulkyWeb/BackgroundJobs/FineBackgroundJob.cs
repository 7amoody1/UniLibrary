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
    var delayedUsers = _unitOfWork.OrderDetail
        .GetAll(o => !o.IsReturned && o.Type == SD.Borrow && today > o.EndBorrowDate,
            includeProperties: "OrderHeader.ApplicationUser,Product")
        .ToList();

    delayedUsers.RemoveAll(x => x.IsReturned);
     
    // Fetch all existing fines in one go
    var existingFines = _unitOfWork.Fine
        .GetAll(f => delayedUsers.Select(du => du.OrderHeader.ApplicationUserId).Contains(f.ApplicationUserId) &&
                     delayedUsers.Select(du => du.ProductId).Contains(f.ProductId))
        .ToList();

    var finesToAdd = new List<Fine>();

    foreach (var delayedUser in delayedUsers)
    {
        var userId = delayedUser.OrderHeader.ApplicationUserId;
        var productId = delayedUser.ProductId;


        // Check for an existing fine from a previous day
        var existingFine = existingFines.FirstOrDefault(f => f.OrderDetailsId == delayedUser.Id);
        if (existingFine is not null)
        {
            // Check if a fine was issued today
            var hasFineToday = existingFines.Any(f => f.ApplicationUserId == userId &&
                                                      f.ProductId == productId &&
                                                      f.IssuedDate == today);
            if (hasFineToday) continue;
            existingFine.Amount += 0.250; // Update existing fine
            continue;
        }

        // Calculate fine amount based on full timespan
        var daysLate = (today - delayedUser.EndBorrowDate!.Value).TotalDays;
        var amount = daysLate * 0.250;

        // Create new fine
        finesToAdd.Add(new Fine
        {
            ApplicationUserId = userId,
            ProductId = productId,
            Amount = amount,
            IssuedDate = today,
            Type = SD.Delay,
            OrderDetailsId = delayedUser.Id
        });
    }

    // Batch insert new fines
    if (finesToAdd.Any())
    {
        _unitOfWork.Fine.AddRange(finesToAdd);
    }

    _unitOfWork.Save();
}}