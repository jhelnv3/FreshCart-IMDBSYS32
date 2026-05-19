namespace FreshCart.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int LowStockProducts { get; set; }
    }
}