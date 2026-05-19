using FreshCart.Data;
using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FreshCart.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;

        public OrderController(OrderService orderService, AuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult History()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                var orders = _orderService.GetUserOrders();
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order history: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Tracking()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                var orders = _orderService.GetUserOrders();
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load orders: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                var order = _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Tracking");
                }

                // Ensure user can only view their own orders (unless admin/staff)
                var user = _authService.GetCurrentUser();
                if (user.Role == "Customer" && order.UserId != user.UserId)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order details: " + ex.Message;
                return RedirectToAction("Tracking");
            }
        }

        [HttpPost]
        public IActionResult ConfirmReceipt(int orderId)
        {
            try
            {
                if (_orderService.ConfirmReceipt(orderId))
                {
                    TempData["Success"] = "Order received! Thank you for shopping with FreshCart.";
                }
                else
                {
                    TempData["Error"] = "Failed to confirm receipt.";
                }
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to confirm receipt: " + ex.Message;
                return RedirectToAction("Tracking");
            }
        }
    }
}