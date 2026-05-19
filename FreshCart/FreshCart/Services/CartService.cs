using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using FreshCart.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreshCart.Web.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public CartService(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public CartViewModel GetCart()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return new CartViewModel { Items = new List<CartItemViewModel>() };

            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .Where(c => c.UserId == user.UserId)
                .ToList();

            var items = cartItems.Select(c => new CartItemViewModel
            {
                CartItemId = c.CartItemId,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                UnitPrice = c.Product.Price,
                Quantity = c.Quantity,
                Subtotal = c.Product.Price * c.Quantity,
                ImageUrl = c.Product.ImageUrl,
                AvailableStock = c.Product.StockQuantity
            }).ToList();

            var subtotal = items.Sum(i => i.Subtotal);
            var deliveryFee = subtotal > 0 ? 50.00m : 0;

            return new CartViewModel
            {
                Items = items,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                Total = subtotal + deliveryFee
            };
        }

        public int GetCartItemCount()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return 0;

            return _context.CartItems
                .Where(c => c.UserId == user.UserId)
                .Sum(c => c.Quantity);
        }

        public bool AddToCart(int productId, int quantity)
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return false;

                var product = _context.Products.Find(productId);
                if (product == null || !product.IsActive) return false;

                if (product.StockQuantity < quantity) return false;

                var existingItem = _context.CartItems
                    .FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = user.UserId,
                        ProductId = productId,
                        Quantity = quantity
                    });
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromCart(int cartItemId)
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return false;

                var cartItem = _context.CartItems
                    .FirstOrDefault(c => c.CartItemId == cartItemId && c.UserId == user.UserId);

                if (cartItem == null) return false;

                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ClearCart()
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return false;

                var cartItems = _context.CartItems.Where(c => c.UserId == user.UserId);
                _context.CartItems.RemoveRange(cartItems);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}