using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreshCart.Web.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly CartService _cartService;

        public OrderService(ApplicationDbContext context, AuthService authService, CartService cartService)
        {
            _context = context;
            _authService = authService;
            _cartService = cartService;
        }

        public string GenerateOrderNumber()
        {
            var random = new Random();
            return $"FC-{DateTime.Now:yyyyMMdd}-{random.Next(1000, 9999)}";
        }

        public Order CreateOrder(string shippingAddress, string paymentMethod)
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return null;

                var cart = _cartService.GetCart();
                if (cart.Items.Count == 0) return null;

                // Validate stock
                foreach (var item in cart.Items)
                {
                    var product = _context.Products.Find(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                        return null;
                }

                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = user.UserId,
                    TotalAmount = cart.Total,
                    PaymentMethod = paymentMethod,
                    Status = "Pending",
                    OrderDate = DateTime.Now,
                    ShippingAddress = shippingAddress
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Create order details and update stock
                foreach (var item in cart.Items)
                {
                    var product = _context.Products.Find(item.ProductId);

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    product.StockQuantity -= item.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }

                _context.SaveChanges();

                // Clear cart
                _cartService.ClearCart();

                return order;
            }
            catch
            {
                return null;
            }
        }

        public List<Order> GetUserOrders()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return new List<Order>();

            return _context.Orders
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == orderId);
        }

        public List<Order> GetAllOrders(string statusFilter = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            return query.OrderByDescending(o => o.OrderDate).ToList();
        }

        public bool UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var order = _context.Orders.Find(orderId);
                if (order == null) return false;

                var validStatuses = new[] { "Pending", "Packing", "ToDeliver", "ToReceive", "Delivered", "Received" };
                if (!validStatuses.Contains(newStatus)) return false;

                order.Status = newStatus;

                if (newStatus == "Received")
                {
                    order.ReceivedDate = DateTime.Now;
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ConfirmReceipt(int orderId)
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null) return false;

                var order = _context.Orders
                    .FirstOrDefault(o => o.OrderId == orderId && o.UserId == user.UserId);

                if (order == null || order.Status != "Delivered") return false;

                return UpdateOrderStatus(orderId, "Received");
            }
            catch
            {
                return false;
            }
        }

        public int GetPendingOrdersCount()
        {
            return _context.Orders.Count(o => o.Status == "Pending" || o.Status == "Packing");
        }

        public decimal GetTotalRevenue()
        {
            return _context.Orders
                .Where(o => o.Status == "Received" || o.Status == "Delivered")
                .Sum(o => o.TotalAmount);
        }
    }
}