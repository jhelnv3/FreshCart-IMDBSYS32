using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace FreshCart.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;

        public HomeController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var featuredProducts = _productService.GetProducts(page: 1, pageSize: 8);
                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load products: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Error(int? statusCode = null)
        {
            var message = statusCode switch
            {
                404 => "Page not found.",
                403 => "Access denied.",
                500 => "Internal server error.",
                _ => "An error occurred."
            };

            ViewBag.StatusCode = statusCode ?? 500;
            ViewBag.Message = message;

            return View();
        }
    }
}