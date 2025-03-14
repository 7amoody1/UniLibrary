﻿using BulkyBook.DataAccess.Repository.IRepository;
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
        return View(productList);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart cart = new()
        {
            Product = _unitOfWork.Product.Get(u => u.Id == productId, "Category,ProductImages"),
            Count = 1,
            ProductId = productId
        };
        return View(cart);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        if (shoppingCart.Type == "Borrow")
        {
            var isConflict = _unitOfWork.OrderDetail
                .GetAll(u => u.StartBorrowDate < shoppingCart.EndBorrowDate &&
                             u.EndBorrowDate > shoppingCart.StartBorrowDate && u.ProductId == shoppingCart.ProductId)
                .Any();

            if (isConflict)
            {
                ModelState.AddModelError(string.Empty, "This period is already occupied");
                ShoppingCart cart = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId, "Category,ProductImages"),
                    Count = shoppingCart.Count,
                    ProductId = shoppingCart.ProductId
                };
                return View(cart);
            }
        }

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;

        var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
                                                           u.ProductId == shoppingCart.ProductId &&
                                                           u.Type == shoppingCart.Type);

        if (cartFromDb is not null)
        {
            //shopping cart exists
            cartFromDb.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
        }
        else
        {
            //add cart record
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
        }

        TempData["success"] = "Cart updated successfully";
        return RedirectToAction(nameof(Index));
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