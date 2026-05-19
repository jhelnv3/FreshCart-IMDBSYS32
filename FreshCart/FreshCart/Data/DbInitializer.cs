using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using System;
using System.Linq;

namespace FreshCart.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any()) return;

            // Create default admin
            var admin = new User
            {
                Username = "admin",
                Email = "admin@freshcart.ph",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Role = "Admin",
                PhoneNumber = "09170000001",
                Address = "FreshCart HQ, Manila"
            };
            context.Users.Add(admin);

            // Create sample staff
            var staff = new User
            {
                Username = "staff",
                Email = "staff@freshcart.ph",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                FullName = "Maria Santos",
                Role = "Staff",
                PhoneNumber = "09170000002",
                Address = "456 Quezon Ave, Quezon City"
            };
            context.Users.Add(staff);

            // Create sample customer
            var customer = new User
            {
                Username = "user",
                Email = "user@freshcart.ph",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                FullName = "Juan Dela Cruz",
                Role = "Customer",
                PhoneNumber = "09170000003",
                Address = "123 Rizal St, Makati City"
            };
            context.Users.Add(customer);

            // Seed categories
            var categories = new[]
            {
                new Category { Name = "Fruits", Description = "Fresh fruits from local farms" },
                new Category { Name = "Vegetables", Description = "Farm-fresh vegetables" },
                new Category { Name = "Dairy & Eggs", Description = "Milk, cheese, eggs, and more" },
                new Category { Name = "Meat & Seafood", Description = "Fresh meat and seafood products" },
                new Category { Name = "Pantry Essentials", Description = "Cooking basics and condiments" },
                new Category { Name = "Beverages", Description = "Drinks, juices, and water" },
                new Category { Name = "Bread & Bakery", Description = "Freshly baked goods" },
                new Category { Name = "Snacks", Description = "Chips, cookies, and treats" },
                new Category { Name = "Canned Goods", Description = "Canned and preserved foods" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Seed 37 products with Philippine prices
            var products = new[]
            {
                // Fruits (Category 1)
                new Product { Name = "Banana Lakatan", Description = "Sweet local lakatan bananas, per kilo", Price = 85.00m, StockQuantity = 100, CategoryId = 1, ImageUrl = "banana.jpg" },
                new Product { Name = "Mango (Carabao)", Description = "Sweet Philippine carabao mangoes, per kilo", Price = 180.00m, StockQuantity = 50, CategoryId = 1, ImageUrl = "mango.jpg" },
                new Product { Name = "Calamansi", Description = "Fresh local calamansi, 500g pack", Price = 60.00m, StockQuantity = 80, CategoryId = 1, ImageUrl = "calamansi.jpg" },
                new Product { Name = "Papaya", Description = "Ripe papaya, per kilo", Price = 55.00m, StockQuantity = 40, CategoryId = 1, ImageUrl = "papaya.jpg" },
                new Product { Name = "Pineapple", Description = "Sweet pineapple, whole", Price = 75.00m, StockQuantity = 35, CategoryId = 1, ImageUrl = "pineapple.jpg" },

                // Vegetables (Category 2)
                new Product { Name = "Kangkong", Description = "Fresh water spinach, 200g bundle", Price = 25.00m, StockQuantity = 100, CategoryId = 2, ImageUrl = "kangkong.jpg" },
                new Product { Name = "Siling Labuyo", Description = "Bird's eye chili, 100g pack", Price = 30.00m, StockQuantity = 60, CategoryId = 2, ImageUrl = "labuyo.jpg" },
                new Product { Name = "Eggplant", Description = "Long purple eggplant, per kilo", Price = 80.00m, StockQuantity = 45, CategoryId = 2, ImageUrl = "eggplant.jpg" },
                new Product { Name = "Okra", Description = "Fresh okra, per kilo", Price = 70.00m, StockQuantity = 50, CategoryId = 2, ImageUrl = "okra.jpg" },
                new Product { Name = "Baguio Beans", Description = "Fresh green beans from Baguio, 250g", Price = 45.00m, StockQuantity = 70, CategoryId = 2, ImageUrl = "baguio_beans.jpg" },

                // Dairy & Eggs (Category 3)
                new Product { Name = "Fresh Eggs (Large)", Description = "Dozen large farm eggs", Price = 110.00m, StockQuantity = 200, CategoryId = 3, ImageUrl = "eggs.jpg" },
                new Product { Name = "Fresh Milk", Description = "1L fresh cow's milk", Price = 95.00m, StockQuantity = 80, CategoryId = 3, ImageUrl = "milk.jpg" },
                new Product { Name = "Cheddar Cheese", Description = "165g cheddar cheese block", Price = 85.00m, StockQuantity = 40, CategoryId = 3, ImageUrl = "cheese.jpg" },
                new Product { Name = "Butter", Description = "Salted butter, 100g", Price = 65.00m, StockQuantity = 50, CategoryId = 3, ImageUrl = "butter.jpg" },

                // Meat & Seafood (Category 4)
                new Product { Name = "Chicken Breast", Description = "Boneless chicken breast, per kilo", Price = 220.00m, StockQuantity = 30, CategoryId = 4, ImageUrl = "chicken.jpg" },
                new Product { Name = "Pork Belly", Description = "Fresh pork belly (liempo), per kilo", Price = 350.00m, StockQuantity = 25, CategoryId = 4, ImageUrl = "liempo.jpg" },
                new Product { Name = "Tilapia", Description = "Fresh tilapia, per kilo", Price = 160.00m, StockQuantity = 20, CategoryId = 4, ImageUrl = "tilapia.jpg" },
                new Product { Name = "Bangus", Description = "Fresh milkfish, per kilo", Price = 190.00m, StockQuantity = 20, CategoryId = 4, ImageUrl = "bangus.jpg" },

                // Pantry Essentials (Category 5)
                new Product { Name = "Cooking Oil", Description = "1L vegetable cooking oil", Price = 120.00m, StockQuantity = 100, CategoryId = 5, ImageUrl = "oil.jpg" },
                new Product { Name = "Soy Sauce", Description = "500ml soy sauce", Price = 45.00m, StockQuantity = 80, CategoryId = 5, ImageUrl = "soy_sauce.jpg" },
                new Product { Name = "Vinegar", Description = "500ml cane vinegar", Price = 35.00m, StockQuantity = 80, CategoryId = 5, ImageUrl = "vinegar.jpg" },
                new Product { Name = "Fish Sauce", Description = "750ml patis", Price = 55.00m, StockQuantity = 70, CategoryId = 5, ImageUrl = "fish_sauce.jpg" },
                new Product { Name = "White Sugar", Description = "1kg refined sugar", Price = 80.00m, StockQuantity = 90, CategoryId = 5, ImageUrl = "sugar.jpg" },

                // Beverages (Category 6)
                new Product { Name = "Instant Coffee", Description = "200g instant coffee", Price = 150.00m, StockQuantity = 60, CategoryId = 6, ImageUrl = "coffee.jpg" },
                new Product { Name = "Powdered Juice", Description = "500g orange powdered juice", Price = 75.00m, StockQuantity = 50, CategoryId = 6, ImageUrl = "juice.jpg" },
                new Product { Name = "Mineral Water", Description = "500ml bottled water, 24-pack", Price = 200.00m, StockQuantity = 40, CategoryId = 6, ImageUrl = "water.jpg" },

                // Bread & Bakery (Category 7)
                new Product { Name = "Pandesal", Description = "Fresh pandesal, 12 pieces", Price = 60.00m, StockQuantity = 30, CategoryId = 7, ImageUrl = "pandesal.jpg" },
                new Product { Name = "Tasty Bread", Description = "Loaf of white bread", Price = 65.00m, StockQuantity = 25, CategoryId = 7, ImageUrl = "bread.jpg" },
                new Product { Name = "Ensaymada", Description = "Classic ensaymada, 6 pieces", Price = 120.00m, StockQuantity = 20, CategoryId = 7, ImageUrl = "ensaymada.jpg" },

                // Snacks (Category 8)
                new Product { Name = "Banana Chips", Description = "Crispy banana chips, 200g", Price = 85.00m, StockQuantity = 60, CategoryId = 8, ImageUrl = "banana_chips.jpg" },
                new Product { Name = "Polvoron", Description = "Classic polvoron, 10 pieces", Price = 75.00m, StockQuantity = 40, CategoryId = 8, ImageUrl = "polvoron.jpg" },
                new Product { Name = "Dried Mango", Description = "Sweet dried mango, 200g", Price = 120.00m, StockQuantity = 45, CategoryId = 8, ImageUrl = "dried_mango.jpg" },

                // Canned Goods (Category 9)
                new Product { Name = "Corned Beef", Description = "260g corned beef", Price = 85.00m, StockQuantity = 100, CategoryId = 9, ImageUrl = "corned_beef.jpg" },
                new Product { Name = "Sardines", Description = "155g sardines in tomato sauce", Price = 25.00m, StockQuantity = 150, CategoryId = 9, ImageUrl = "sardines.jpg" },
                new Product { Name = "Meat Loaf", Description = "397g luncheon meat", Price = 95.00m, StockQuantity = 80, CategoryId = 9, ImageUrl = "luncheon_meat.jpg" },
                new Product { Name = "Tuna", Description = "185g canned tuna flakes", Price = 45.00m, StockQuantity = 120, CategoryId = 9, ImageUrl = "tuna.jpg" },
                new Product { Name = "Spaghetti Sauce", Description = "500g Filipino-style spaghetti sauce", Price = 55.00m, StockQuantity = 70, CategoryId = 9, ImageUrl = "spaghetti_sauce.jpg" }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}