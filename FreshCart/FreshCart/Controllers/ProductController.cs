using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace FreshCart.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;

        public ProductController(ProductService productService, CartService cartService,
            AuthService authService, ApplicationDbContext context)
        {
            _productService = productService;
            _cartService = cartService;
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string search = null, int? categoryId = null, int page = 1)
        {
            try
            {
                var products = _productService.GetProducts(search, categoryId, page);
                var totalCount = _productService.GetTotalProductCount(search, categoryId);
                var categories = _context.Categories.OrderBy(c => c.Name).ToList();

                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                ViewBag.Search = search;
                ViewBag.CategoryId = categoryId;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / 12);
                ViewBag.TotalCount = totalCount;

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load products: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load product: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                if (quantity < 1)
                {
                    TempData["Error"] = "Quantity must be at least 1.";
                    return RedirectToAction("Details", new { id = productId });
                }

                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                if (product.StockQuantity < quantity)
                {
                    TempData["Error"] = $"Only {product.StockQuantity} items available.";
                    return RedirectToAction("Details", new { id = productId });
                }

                if (_cartService.AddToCart(productId, quantity))
                {
                    TempData["Success"] = $"{quantity}x {product.Name} added to cart!";
                }
                else
                {
                    TempData["Error"] = "Failed to add item to cart.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to add to cart: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}