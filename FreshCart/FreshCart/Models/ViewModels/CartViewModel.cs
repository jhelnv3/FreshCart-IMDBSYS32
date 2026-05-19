using System.Collections.Generic;

namespace FreshCart.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; } = 50.00m;
        public decimal Total { get; set; }
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string ImageUrl { get; set; }
        public int AvailableStock { get; set; }
    }
}