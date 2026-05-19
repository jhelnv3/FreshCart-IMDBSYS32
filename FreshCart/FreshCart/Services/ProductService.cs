using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FreshCart.Web.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> GetProducts(string search = null, int? categoryId = null, int page = 1, int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return query.OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalProductCount(string search = null, int? categoryId = null)
        {
            var query = _context.Products.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return query.Count();
        }

        public Product GetProductById(int productId)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == productId);
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public int GetLowStockCount()
        {
            return _context.Products.Count(p => p.StockQuantity < 10 && p.IsActive);
        }
    }
}