using FreshCart.Data;
using FreshCart.Web.Models.ViewModels;
using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FreshCart.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        private readonly OrderService _orderService;
        private readonly EmailService _emailService;

        public CartController(CartService cartService, AuthService authService,
            OrderService orderService, EmailService emailService)
        {
            _cartService = cartService;
            _authService = authService;
            _orderService = orderService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                var cart = _cartService.GetCart();
                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load cart: " + ex.Message;
                return View(new CartViewModel());
            }
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            try
            {
                if (_cartService.RemoveFromCart(id))
                {
                    TempData["Success"] = "Item removed from cart.";
                }
                else
                {
                    TempData["Error"] = "Failed to remove item.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to remove item: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            try
            {
                if (!_authService.IsAuthenticated())
                    return RedirectToAction("Login", "Account");

                var cart = _cartService.GetCart();
                if (cart.Items.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index");
                }

                var model = new CheckoutViewModel
                {
                    Cart = cart,
                    ShippingAddress = _authService.GetCurrentUser()?.Address ?? "",
                    PaymentMethod = "COD"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load checkout: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Cart = _cartService.GetCart();
                    return View("Checkout", model);
                }

                var order = _orderService.CreateOrder(model.ShippingAddress, model.PaymentMethod);
                if (order != null)
                {
                    var user = _authService.GetCurrentUser();
                    _emailService.SendOrderConfirmation(user.Email, order.OrderNumber);

                    TempData["Success"] = $"Order #{order.OrderNumber} placed successfully!";
                    return RedirectToAction("Tracking", "Order");
                }

                TempData["Error"] = "Failed to place order. Some items may be out of stock.";
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to place order: " + ex.Message;
                return RedirectToAction("Checkout");
            }
        }
    }
}