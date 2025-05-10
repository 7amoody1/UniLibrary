using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
        ViewBag.Categories = _unitOfWork.Category.GetAll().ToList(); // For category filter dropdown
        return View(productList);
    }

    [HttpGet]
    public IActionResult GetProducts(string searchTerm, int? categoryId, string sortOption)
    {
        var products = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").AsQueryable();

        // Search by title or author
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            products = products.Where(p => p.Title.ToLower().Contains(searchTerm) || p.Author.ToLower().Contains(searchTerm));
        }

        // Filter by category
        if (categoryId.HasValue && categoryId > 0)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        // Sort
        products = sortOption switch
        {
            "title-asc" => products.OrderBy(p => p.Title),
            "title-desc" => products.OrderByDescending(p => p.Title),
            "price-asc" => products.OrderBy(p => p.Price),
            "price-desc" => products.OrderByDescending(p => p.Price),
            "stock-asc" => products.OrderBy(p => p.QuantityInStock),
            "stock-desc" => products.OrderByDescending(p => p.QuantityInStock),
            _ => products.OrderBy(p => p.Title)
        };

        return PartialView("Products", products.ToList());
    }

    public IActionResult Details(int productId, bool error = false)
    {
        ShoppingCart cart = new()
        {
            Product = _unitOfWork.Product.Get(u => u.Id == productId, "Category,ProductImages"),
            Count = 1,
            ProductId = productId
        };
        if (error)
            ModelState.AddModelError("", "Quantity In Stock Exceeded");
        return View(cart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;

        var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
                                                           u.ProductId == shoppingCart.ProductId &&
                                                           u.Type == shoppingCart.Type);
        var product = _unitOfWork.Product.Get(x => x.Id == shoppingCart.ProductId);
        if (shoppingCart.Count > product.QuantityInStock)
        {
            return RedirectToAction(nameof(Details), new { productId = shoppingCart.ProductId, error = true });
        }

        if (cartFromDb is not null)
        {
            cartFromDb.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
        }
        else
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
        }

        TempData["success"] = "Cart updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public IActionResult JoinWaitList(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId is null)
        {
            TempData["error"] = "User not found.";
            return RedirectToAction(nameof(Details), new { productId });
        }
        
        var product = _unitOfWork.Product.Get(u => u.Id == productId);
        
        if (product is null)
        {
            TempData["error"] = "Product not found.";
            return RedirectToAction(nameof(Details), new { productId });
        }

        if (product.QuantityInStock > 0)
        {
            TempData["error"] = "This book is in stock. No need to join the wait list.";
            return RedirectToAction(nameof(Details), new { productId });
        }

        var existingEntry = _unitOfWork.WishItem.Get(
            w => w.ApplicationUserId == userId && w.ProductId == productId && !w.IsNotified);

        if (existingEntry is not null)
        {
            TempData["error"] = "You are already on the wait list for this book.";
            return RedirectToAction(nameof(Details), new { productId });
        }

        var waitListEntry = new WishItem
        {
            ApplicationUserId = userId,
            ProductId = productId,
            EnrolledDate = DateTime.Now,
            IsNotified = false
        };

        _unitOfWork.WishItem.Add(waitListEntry);
        _unitOfWork.Save();

        TempData["success"] = "You have been added to the wait list. We'll notify you when the book is available.";
        return RedirectToAction(nameof(Details), new { productId });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}