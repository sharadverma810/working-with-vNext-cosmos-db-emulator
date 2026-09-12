using CosmosLearning.Vector.Api.Features.Products.Models;

namespace CosmosLearning.Vector.Api.Features.Products.Seed;

public static class ProductSeedData
{
    public static IReadOnlyList<VectorProduct> GetProducts()
    {
        return
        [
            new VectorProduct
            {
                Id = "product-001",
                Name = "Premium Wireless Noise Cancelling Headphones",
                Category = "Electronics",
                Price = 24999,
                Description =
                    "Comfortable wireless headphones with active noise cancellation, long battery life and high quality sound."
            },

            new VectorProduct
            {
                Id = "product-002",
                Name = "Bluetooth Sports Earbuds",
                Category = "Electronics",
                Price = 4999,
                Description =
                    "Lightweight wireless earbuds designed for gym workouts, running and outdoor activities."
            },

            new VectorProduct
            {
                Id = "product-003",
                Name = "Professional Mirrorless Camera",
                Category = "Electronics",
                Price = 89999,
                Description =
                    "High resolution mirrorless camera for professional photography, travel photography and video recording."
            },

            new VectorProduct
            {
                Id = "product-004",
                Name = "Smartphone Pro Camera Edition",
                Category = "Electronics",
                Price = 74999,
                Description =
                    "Premium smartphone with advanced camera system, excellent low light photography and powerful performance."
            },

            new VectorProduct
            {
                Id = "product-005",
                Name = "Gaming Laptop",
                Category = "Computers",
                Price = 119999,
                Description =
                    "High performance gaming laptop with powerful graphics card, fast processor and high refresh rate display."
            },

            new VectorProduct
            {
                Id = "product-006",
                Name = "Ultra Thin Business Laptop",
                Category = "Computers",
                Price = 79999,
                Description =
                    "Lightweight professional laptop with long battery life, fast performance and portable design."
            },

            new VectorProduct
            {
                Id = "product-007",
                Name = "Mechanical Gaming Keyboard",
                Category = "Electronics",
                Price = 7999,
                Description =
                    "Mechanical keyboard with responsive switches, RGB lighting and comfortable gaming design."
            },

            new VectorProduct
            {
                Id = "product-008",
                Name = "Ergonomic Office Chair",
                Category = "Furniture",
                Price = 18999,
                Description =
                    "Comfortable ergonomic chair with lumbar support for working long hours at a computer desk."
            },

            new VectorProduct
            {
                Id = "product-009",
                Name = "Modern Wooden Study Desk",
                Category = "Furniture",
                Price = 15999,
                Description =
                    "Spacious wooden desk suitable for home office, study room and computer workstation."
            },

            new VectorProduct
            {
                Id = "product-010",
                Name = "Stainless Steel Water Bottle",
                Category = "Home",
                Price = 999,
                Description =
                    "Reusable insulated water bottle that keeps drinks cold and hot for long periods."
            },

            new VectorProduct
            {
                Id = "product-011",
                Name = "Robot Vacuum Cleaner",
                Category = "Home",
                Price = 29999,
                Description =
                    "Smart robotic vacuum cleaner for automatic home floor cleaning with navigation and scheduling."
            },

            new VectorProduct
            {
                Id = "product-012",
                Name = "Fitness Smart Watch",
                Category = "Wearables",
                Price = 14999,
                Description =
                    "Smart watch for fitness tracking, heart rate monitoring, sleep tracking and workout activities."
            }
        ];
    }
}