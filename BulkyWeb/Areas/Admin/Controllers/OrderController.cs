using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Stripe;
    using Stripe.Checkout;
    using System.Diagnostics;
    using System.Security.Claims;
    using Product = Stripe.Product;
    using TablesVM = BulkyBook.Models.ViewModels.TablesVM;

    namespace BulkyBookWeb.Areas.Admin.Controllers {
	    [Area("admin")]
        [Authorize]
	    public class OrderController : Controller {

		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string requestedData)
        {
            TablesVM tableVm;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            switch (requestedData)
            {
                case SD.Fines:
                {
                    var fines = User.IsInRole(SD.Role_Admin) 
                        ? _unitOfWork.Fine.GetAll(f => f.Status == SD.PendingFine, includeProperties:"ApplicationUser,Product").ToList() 
                        : _unitOfWork.Fine.GetAll(f => f.ApplicationUserId == userId && 
                            f.Status == SD.PendingFine, includeProperties:"Product").ToList();
                    tableVm = new TablesVM
                    {
                        FinesList = fines,
                        RequestedData = SD.Fines,
                        IsFines = true
                    };
                    return View(tableVm);    
                }
                case SD.Orders:
                {
                    var orderHeaders = User.IsInRole(SD.Role_Admin)
                     ? _unitOfWork.OrderHeader.GetAll(includeProperties:"ApplicationUser").ToList() 
                     : _unitOfWork.OrderHeader.GetAll(f => f.ApplicationUserId == userId).ToList();

                    tableVm = new TablesVM
                    {
                        OrderHeadersList = orderHeaders,
                        RequestedData = SD.Orders
                    };
                    return View(tableVm);
                }
                default:
                {
                    var wishItems = User.IsInRole(SD.Role_Admin)
                        ? _unitOfWork.WishItem.GetAll().ToList() 
                        : _unitOfWork.WishItem.GetAll(f => f.ApplicationUserId == userId).ToList();
                    tableVm = new TablesVM
                    {
                        WishItemsList = wishItems,
                        RequestedData = SD.WishItems
                    };
                    return View(tableVm);
                }
            }
        }

        public IActionResult PayFineConfirmation(int opId, int? fineId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            List<Fine> fines = [];
            if (fineId is null)
            {
                fines.AddRange(_unitOfWork.Fine.GetAll(
                    f => f.ApplicationUserId == userId && f.Status == SD.PendingFine,
                    includeProperties:"Product"));
            }
            else
            {
                fines.Add(_unitOfWork.Fine.Get(
                    f => f.ApplicationUserId == userId && f.Id == fineId && f.Status == SD.PendingFine
                    ,includeProperties:"Product"));
            }
            foreach (var fine in fines)
            {
                fine.Status = SD.PayedFine;
            }
            _unitOfWork.Save();
            return View(opId);
        }

        public IActionResult PayFineSummery(int? fineId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            List<Fine> fines = [];
            if (fineId is null)
            {
                fines.AddRange(_unitOfWork.Fine.GetAll(
                    f => f.ApplicationUserId == userId && f.Status == SD.PendingFine,
                     includeProperties:"Product"));
            }
            else
            {
                fines.Add(_unitOfWork.Fine.Get(
                    f => f.ApplicationUserId == userId && f.Id == fineId && f.Status == SD.PendingFine
                    ,includeProperties:"Product"));
            }

            var orderHeader = new OrderHeader
            {
                Name = user.Name,
                PhoneNumber = user.PhoneNumber ?? "",
                StreetAddress = user.StreetAddress ?? "",
                City = user.City ?? "",
                State = user.State ?? "",
                PostalCode = user.PostalCode ?? "",
                OrderTotal = fines.Sum(f => f.Amount)
            };

            var fineVm = new FineVm
            {
                Fines = fines,
                OrderHeader = orderHeader,  
                FineId = fineId 
            };
            return View(fineVm);
        }
        
        [HttpPost]
        [ActionName("PayFineSummery")]
        public IActionResult PayFine(FineVm fineVm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            List<Fine> fines = [];
            if (fineVm.FineId is null)
            {
                fines.AddRange(_unitOfWork.Fine.GetAll(
                    f => f.ApplicationUserId == userId && f.Status == SD.PendingFine,
                    includeProperties:"Product"));
            }
            else
            {
                fines.Add(_unitOfWork.Fine.Get(
                    f => f.ApplicationUserId == userId && f.Id == fineVm.FineId && f.Status == SD.PendingFine
                    ,includeProperties:"Product"));
            }

            fineVm.OrderHeader.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            fineVm.OrderHeader.OrderDate = DateTime.UtcNow;
            fineVm.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
            fineVm.OrderHeader.OrderStatus = SD.StatusApproved;
            
            _unitOfWork.OrderHeader.Add(fineVm.OrderHeader);
            _unitOfWork.Save();
            
            foreach (var fine in fines)
            {
                var orderDetail = new OrderDetail {
                    ProductId = fine.ProductId,
                    OrderHeaderId = fineVm.OrderHeader.Id,
                    Price = fine.Amount,
                    Count = 1, 
                    Type = fine.Type, 
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();

            var domain = Request.Scheme+ "://"+ Request.Host.Value +"/";
            var options = new SessionCreateOptions {
                SuccessUrl = domain+ $"admin/order/PayFineConfirmation?opId={fineVm.OrderHeader.Id}&fineId={fineVm.FineId}",
                CancelUrl = domain+$"admin/order/index?requestedData={SD.Fines}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            
            foreach (var fine in fines)
            {
                var sessionLineItem = new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        UnitAmount = (long)(fine.Amount * 100) , // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = fine.Product!.Title
                        }
                    },
                    Quantity = 1
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            var session = service.Create(options); 
            
            _unitOfWork.OrderHeader.UpdateStripePaymentID(fineVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            /*var fineFromDb = _unitOfWork.Fine.Get(x => x.Id == fineVm.FineId);
            _unitOfWork.Fine.Remove(fineFromDb);*/
            _unitOfWork.Save();
            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }
        
        public IActionResult Details(int orderId) {
            OrderVM = new() {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail() {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier)) {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber)) {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new {orderId= orderHeaderFromDb.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing() {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder() {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment) {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder() {

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved) {
                var options = new RefundCreateOptions {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }



        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW() 
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail) {
                var sessionLineItem = new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId) {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment) {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid") {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }


            }


            return View(orderHeaderId);
        }

      


        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status) {
            IEnumerable<OrderHeader> objOrderHeaders;


            if(User.IsInRole(SD.Role_Admin)|| User.IsInRole(SD.Role_Employee)) {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }


            switch (status) {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }


            return Json(new { data = objOrderHeaders });
		}


        [HttpPost]
        public IActionResult OnCheckChange(int orderHeaderId, bool isReturned)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId, tracked: true);
            orderHeader.IsReturned = isReturned;
            _unitOfWork.Save();
            return Ok();
        }


		#endregion
	}
}
