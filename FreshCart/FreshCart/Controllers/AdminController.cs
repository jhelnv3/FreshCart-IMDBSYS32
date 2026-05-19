using FreshCart.Data;
using FreshCart.Web.Filters;
using FreshCart.Web.Models.Entities;
using FreshCart.Web.Models.ViewModels;
using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FreshCart.Web.Controllers
{
    [RoleAuthorize("Admin", "Staff")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public AdminController(ApplicationDbContext context, AuthService authService,
            OrderService orderService, ProductService productService)
        {
            _context = context;
            _authService = authService;
            _orderService = orderService;
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    TotalProducts = _context.Products.Count(p => p.IsActive),
                    TotalOrders = _context.Orders.Count(),
                    TotalUsers = _context.Users.Count(u => u.IsActive),
                    PendingOrders = _orderService.GetPendingOrdersCount(),
                    TotalRevenue = _orderService.GetTotalRevenue(),
                    LowStockProducts = _productService.GetLowStockCount()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load dashboard: " + ex.Message;
                return View(new AdminDashboardViewModel());
            }
        }

        // ==================== PRODUCTS ====================

        [HttpGet]
        public IActionResult Products(string search = null, int? categoryId = null)
        {
            try
            {
                var products = _productService.GetAllProducts();
                if (!string.IsNullOrEmpty(search))
                {
                    products = products.Where(p => p.Name.Contains(search) ||
                        (p.Description != null && p.Description.Contains(search))).ToList();
                }
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
                }
                ViewBag.Categories = new SelectList(_context.Categories.OrderBy(c => c.Name), "CategoryId", "Name");
                ViewBag.Search = search;
                ViewBag.CategoryId = categoryId;
                return View("~/Views/Admin/Products/Index.cshtml", products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load products: " + ex.Message;
                return View("~/Views/Admin/Products/Index.cshtml", new List<Product>());
            }
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            var model = new ProductViewModel { Categories = GetCategoryList() };
            return View("~/Views/Admin/Products/Create.cshtml", model);
        }

        [HttpPost]
        public IActionResult CreateProduct(ProductViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Categories = GetCategoryList();
                    return View("~/Views/Admin/Products/Create.cshtml", model);
                }
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ImageUrl = model.ImageUrl ?? "placeholder.jpg"
                };
                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Product added successfully.";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create product: " + ex.Message);
                model.Categories = GetCategoryList();
                return View("~/Views/Admin/Products/Create.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }
                var model = new ProductViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    ImageUrl = product.ImageUrl,
                    Categories = GetCategoryList()
                };
                return View("~/Views/Admin/Products/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load product: " + ex.Message;
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        public IActionResult EditProduct(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["Error"] = "Validation failed: " + errors;
                model.Categories = GetCategoryList();
                return View("~/Views/Admin/Products/Edit.cshtml", model);
            }
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Categories = GetCategoryList();
                    return View("~/Views/Admin/Products/Edit.cshtml", model);
                }

                // Create a new product object with only the properties we want to update
                var product = new Product
                {
                    ProductId = id,
                    Name = model.Name,
                    Description = model.Description ?? "",
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ImageUrl = !string.IsNullOrEmpty(model.ImageUrl) ? model.ImageUrl : "placeholder.jpg",
                    IsActive = true
                };

                // Attach and mark only specific properties as modified
                _context.Products.Attach(product);
                _context.Entry(product).Property(p => p.Name).IsModified = true;
                _context.Entry(product).Property(p => p.Description).IsModified = true;
                _context.Entry(product).Property(p => p.Price).IsModified = true;
                _context.Entry(product).Property(p => p.StockQuantity).IsModified = true;
                _context.Entry(product).Property(p => p.CategoryId).IsModified = true;
                _context.Entry(product).Property(p => p.ImageUrl).IsModified = true;
                _context.Entry(product).Property(p => p.UpdatedAt).IsModified = true;

                _context.SaveChanges();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed: " + ex.Message;
                if (ex.InnerException != null)
                {
                    TempData["Error"] += " | " + ex.InnerException.Message;
                }
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product != null)
                {
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                    TempData["Success"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                }
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete product: " + ex.Message;
                return RedirectToAction("Products");
            }
        }

        // ==================== ORDERS ====================

        [HttpGet]
        public IActionResult Orders(string status = "All", string search = null)
        {
            try
            {
                var orders = _orderService.GetAllOrders(status);

                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    orders = orders.Where(o =>
                        o.OrderNumber.Contains(search) ||
                        (o.User != null && o.User.FullName.Contains(search)) ||
                        (o.User != null && o.User.Username.Contains(search))
                    ).ToList();
                }

                ViewBag.CurrentStatus = status;
                ViewBag.Search = search;
                return View("~/Views/Admin/Orders/Index.cshtml", orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load orders: " + ex.Message;
                return View("~/Views/Admin/Orders/Index.cshtml", new List<Order>());
            }
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            try
            {
                var order = _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Orders");
                }
                return View("~/Views/Admin/Orders/Details.cshtml", order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order: " + ex.Message;
                return RedirectToAction("Orders");
            }
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                if (_orderService.UpdateOrderStatus(orderId, newStatus))
                {
                    TempData["Success"] = $"Order status updated to {newStatus}.";
                }
                else
                {
                    TempData["Error"] = "Failed to update order status.";
                }
                return RedirectToAction("OrderDetails", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update status: " + ex.Message;
                return RedirectToAction("Orders");
            }
        }

        // ==================== USERS ====================

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult Users(string role = "All", string search = null)
        {
            try
            {
                var users = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(role) && role != "All")
                {
                    users = users.Where(u => u.Role == role);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    users = users.Where(u =>
                        u.Username.Contains(search) ||
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search));
                }

                ViewBag.CurrentRole = role;
                ViewBag.Search = search;
                return View("~/Views/Admin/Users/Index.cshtml", users.OrderBy(u => u.Username).ToList());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load users: " + ex.Message;
                return View("~/Views/Admin/Users/Index.cshtml", new List<User>());
            }
        }

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new SelectList(new[] { "Customer", "Staff" });
            return View("~/Views/Admin/Users/Create.cshtml", new RegisterViewModel());
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult CreateUser(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff" });
                    return View("~/Views/Admin/Users/Create.cshtml", model);
                }
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff" });
                    return View("~/Views/Admin/Users/Create.cshtml", model);
                }
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff" });
                    return View("~/Views/Admin/Users/Create.cshtml", model);
                }
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = _authService.HashPassword(model.Password),
                    FullName = model.FullName,
                    Role = model.Role,
                    PhoneNumber = "",
                    Address = ""
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["Success"] = $"User {model.Username} created successfully.";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create user: " + ex.Message);
                ViewBag.Roles = new SelectList(new[] { "Customer", "Staff" });
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }
        }

        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }
                var model = new EditUserViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                };
                ViewBag.Roles = new SelectList(new[] { "Customer", "Staff", "Admin" }, user.Role);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load user: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult EditUser(int id, EditUserViewModel model)
        {
            try
            {
                // Remove password from validation if both fields are empty
                if (string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        ModelState.AddModelError("Password", "Password is required when changing password.");
                    }
                    else if (model.Password.Length < 6)
                    {
                        ModelState.AddModelError("Password", "Password must be at least 6 characters.");
                    }
                    if (model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff", "Admin" }, model.Role);
                    return View("~/Views/Admin/Users/Edit.cshtml", model);
                }

                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }
                if (_context.Users.Any(u => u.Username == model.Username && u.UserId != id))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff", "Admin" }, model.Role);
                    return View("~/Views/Admin/Users/Edit.cshtml", model);
                }
                if (_context.Users.Any(u => u.Email == model.Email && u.UserId != id))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag.Roles = new SelectList(new[] { "Customer", "Staff", "Admin" }, model.Role);
                    return View("~/Views/Admin/Users/Edit.cshtml", model);
                }

                user.Username = model.Username;
                user.Email = model.Email;
                user.FullName = model.FullName;
                user.Role = model.Role;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = _authService.HashPassword(model.Password);
                }
                _context.SaveChanges();
                TempData["Success"] = $"User {model.Username} updated successfully.";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update user: " + ex.Message);
                ViewBag.Roles = new SelectList(new[] { "Customer", "Staff", "Admin" }, model.Role);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = false;
                    _context.SaveChanges();
                    TempData["Success"] = "User deactivated successfully.";
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete user: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        [RoleAuthorize("Admin")]
        [HttpPost]
        public IActionResult ReactivateUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = true;
                    _context.SaveChanges();
                    TempData["Success"] = $"User {user.Username} reactivated successfully.";
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to reactivate user: " + ex.Message;
                return RedirectToAction("Users");
            }
        }
                
        // ==================== CATEGORIES ====================

        [HttpGet]
        public IActionResult Categories()
        {
            try
            {
                var categories = _context.Categories.OrderBy(c => c.Name).ToList();
                return View("~/Views/Admin/Categories/Index.cshtml", categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load categories: " + ex.Message;
                return View("~/Views/Admin/Categories/Index.cshtml", new List<Category>());
            }
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View("~/Views/Admin/Categories/Create.cshtml", new Category());
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("~/Views/Admin/Categories/Create.cshtml", category);
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create category: " + ex.Message);
                return View("~/Views/Admin/Categories/Create.cshtml", category);
            }
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Categories");
                }
                return View("~/Views/Admin/Categories/Edit.cshtml", category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load category: " + ex.Message;
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("~/Views/Admin/Categories/Edit.cshtml", category);
                var existingCategory = _context.Categories.Find(category.CategoryId);
                if (existingCategory == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Categories");
                }
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                _context.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update category: " + ex.Message);
                return View("~/Views/Admin/Categories/Edit.cshtml", category);
            }
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category != null)
                {
                    if (_context.Products.Any(p => p.CategoryId == id && p.IsActive))
                    {
                        TempData["Error"] = "Cannot delete category with existing products.";
                    }
                    else
                    {
                        _context.Categories.Remove(category);
                        _context.SaveChanges();
                        TempData["Success"] = "Category deleted successfully.";
                    }
                }
                else
                {
                    TempData["Error"] = "Category not found.";
                }
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete category: " + ex.Message;
                return RedirectToAction("Categories");
            }
        }

        private List<SelectListItem> GetCategoryList()
        {
            return _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.CategoryId.ToString()))
                .ToList();
        }
    }
}