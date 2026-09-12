using CosmosLearning.Vector.Api.Features.Products.Models;

namespace CosmosLearning.Vector.Api.Features.Products.Seed;

public static class LargeProductCatalog
{
    private static readonly string[] Adjectives =
    [
        "Premium",
        "Advanced",
        "Professional",
        "Smart",
        "Ultra",
        "Performance",
        "Compact",
        "Portable",
        "Pro",
        "Modern"
    ];

    private static readonly string[] Colors =
    [
        "Black",
        "Silver",
        "White",
        "Blue",
        "Graphite"
    ];

    private static readonly ProductTemplate[] Templates =
    [
        new(
            "Electronics",
            "Wireless Mechanical Keyboard",
            4999,
            14999,
            "Mechanical keyboard with responsive switches, RGB lighting, wireless connectivity and comfortable typing for gaming and office work.",
            "gaming keyboard mechanical keyboard RGB keyboard wireless keyboard PC gaming typing"),

        new(
            "Electronics",
            "Noise Cancelling Headphones",
            7999,
            29999,
            "Wireless headphones with active noise cancellation, long battery life, clear audio and comfortable ear cushions for travel and work.",
            "wireless headphones noise cancellation audio music travel office"),

        new(
            "Electronics",
            "Bluetooth Speaker",
            2499,
            12999,
            "Portable Bluetooth speaker with powerful audio, deep bass, wireless connectivity and long battery life for indoor and outdoor use.",
            "Bluetooth speaker wireless speaker portable audio music outdoor"),

        new(
            "Electronics",
            "USB-C Docking Station",
            3999,
            14999,
            "USB-C docking station with multiple ports for monitors, keyboards, storage devices and professional workstation connectivity.",
            "USB-C dock docking station monitor workstation laptop accessories"),

        new(
            "Electronics",
            "Wireless Gaming Mouse",
            1999,
            8999,
            "High precision wireless gaming mouse with responsive tracking, programmable buttons and ergonomic design for competitive gaming.",
            "gaming mouse wireless mouse PC gaming esports RGB"),

        new(
            "Computers",
            "Gaming Laptop",
            65000,
            160000,
            "High performance gaming laptop with dedicated graphics, powerful processor, high refresh rate display and fast SSD storage.",
            "gaming laptop gaming computer graphics GPU processor high refresh gaming"),

        new(
            "Computers",
            "Business Laptop",
            55000,
            130000,
            "Professional business laptop with fast processor, long battery life, lightweight design and security features for office work.",
            "business laptop professional laptop office work productivity portable"),

        new(
            "Computers",
            "Developer Laptop",
            70000,
            150000,
            "Powerful laptop designed for software development with fast processor, large memory, high speed SSD and multiple connectivity options.",
            "programming laptop developer laptop software development coding computer"),

        new(
            "Computers",
            "Mini Desktop PC",
            30000,
            80000,
            "Compact desktop computer for office productivity, programming, browsing and everyday business workloads.",
            "mini PC desktop computer office programming productivity"),

        new(
            "Computers",
            "Ultrawide Monitor",
            25000,
            90000,
            "Large ultrawide monitor with high resolution, wide workspace and excellent color reproduction for productivity, coding and creative work.",
            "ultrawide monitor computer display programming productivity workstation"),

        new(
            "Gaming",
            "Gaming Keyboard",
            3999,
            14999,
            "RGB mechanical gaming keyboard with fast switches, anti-ghosting and responsive controls designed for competitive PC gaming.",
            "gaming keyboard mechanical RGB esports competitive gaming"),

        new(
            "Gaming",
            "Gaming Headset",
            2999,
            12999,
            "Gaming headset with surround sound, noise isolation, clear microphone and comfortable padding for long gaming sessions.",
            "gaming headset gaming headphones microphone PC console esports"),

        new(
            "Gaming",
            "Gaming Monitor",
            18000,
            75000,
            "High refresh rate gaming monitor with fast response time, adaptive sync and vivid display for competitive games.",
            "gaming monitor high refresh rate esports gaming display"),

        new(
            "Gaming",
            "Gaming Mouse",
            1999,
            9999,
            "Precision gaming mouse with programmable buttons, low latency wireless connectivity and ergonomic grip for competitive gaming.",
            "gaming mouse esports FPS gaming competitive PC"),

        new(
            "Gaming",
            "Gaming Chair",
            12000,
            45000,
            "Ergonomic gaming chair with lumbar support, adjustable armrests and reclining backrest for long gaming sessions.",
            "gaming chair ergonomic lumbar support gaming setup"),

        new(
            "Cameras",
            "Mirrorless Camera",
            55000,
            180000,
            "High resolution mirrorless camera designed for professional photography, travel photography, portraits and high quality video recording.",
            "professional camera mirrorless photography portrait travel video"),

        new(
            "Cameras",
            "Digital Camera",
            25000,
            90000,
            "Compact digital camera with high resolution sensor, optical zoom and advanced photography controls for travel and everyday photography.",
            "digital camera photography travel camera optical zoom"),

        new(
            "Cameras",
            "Camera Lens",
            15000,
            120000,
            "High quality interchangeable camera lens for portraits, landscapes, professional photography and creative photography.",
            "camera lens photography portrait landscape professional"),

        new(
            "Cameras",
            "Action Camera",
            15000,
            55000,
            "Rugged action camera for travel, adventure sports and outdoor recording with high resolution video and image stabilization.",
            "action camera adventure sports travel outdoor video"),

        new(
            "Cameras",
            "Camera Tripod",
            2500,
            15000,
            "Stable adjustable tripod for professional cameras, photography, video recording and studio or outdoor shooting.",
            "camera tripod photography video professional studio"),

        new(
            "Audio",
            "Studio Headphones",
            7000,
            30000,
            "Professional studio headphones with detailed sound reproduction designed for music production, mixing and critical listening.",
            "studio headphones professional audio music production mixing"),

        new(
            "Audio",
            "Wireless Earbuds",
            1999,
            14999,
            "True wireless earbuds with clear audio, comfortable fit, long battery life and noise isolation for commuting and exercise.",
            "wireless earbuds Bluetooth audio music running gym"),

        new(
            "Audio",
            "Soundbar",
            8000,
            45000,
            "Home entertainment soundbar with clear dialogue, powerful bass and wireless connectivity for movies and television.",
            "soundbar home theater TV audio movies entertainment"),

        new(
            "Audio",
            "USB Microphone",
            3500,
            18000,
            "USB microphone with clear voice capture for streaming, podcasting, online meetings and content creation.",
            "USB microphone streaming podcast meetings recording"),

        new(
            "Audio",
            "Portable Audio Player",
            8000,
            40000,
            "Portable high resolution audio player designed for music enthusiasts who want detailed sound and offline music playback.",
            "portable music player high resolution audio audiophile"),

        new(
            "Furniture",
            "Ergonomic Office Chair",
            12000,
            45000,
            "Ergonomic office chair with adjustable lumbar support, comfortable cushioning and adjustable height for long working hours.",
            "office chair ergonomic lumbar support work from home workstation"),

        new(
            "Furniture",
            "Study Desk",
            8000,
            30000,
            "Spacious study desk with storage and a large work surface suitable for students, home offices and computer workstations.",
            "study desk office desk workstation student computer home office"),

        new(
            "Furniture",
            "Standing Desk",
            18000,
            60000,
            "Height adjustable standing desk designed for flexible working, computer use and healthier office routines.",
            "standing desk adjustable desk office workstation computer"),

        new(
            "Furniture",
            "Bookshelf",
            5000,
            25000,
            "Modern bookshelf with multiple storage shelves for books, office accessories and home organization.",
            "bookshelf storage furniture home office organization"),

        new(
            "Furniture",
            "Computer Table",
            7000,
            28000,
            "Computer table with spacious surface and cable management designed for desktop computers and home office setups.",
            "computer table desk workstation home office PC setup"),

        new(
            "Home",
            "Robot Vacuum Cleaner",
            18000,
            60000,
            "Smart robot vacuum cleaner with automatic navigation, scheduling and efficient floor cleaning for modern homes.",
            "robot vacuum smart home floor cleaning automatic"),

        new(
            "Home",
            "Air Purifier",
            8000,
            35000,
            "Smart air purifier designed for bedrooms and living spaces with quiet operation and automated air quality monitoring.",
            "air purifier smart home bedroom living room air quality"),

        new(
            "Home",
            "Coffee Machine",
            6000,
            50000,
            "Automatic coffee machine for preparing fresh coffee at home with programmable settings and convenient operation.",
            "coffee machine espresso home kitchen automatic coffee"),

        new(
            "Home",
            "Smart LED Light",
            999,
            6999,
            "Smart LED lighting with adjustable brightness, scheduling and wireless control for bedrooms, offices and living spaces.",
            "smart light LED lighting home office bedroom IoT"),

        new(
            "Home",
            "Electric Kettle",
            1200,
            6000,
            "Fast electric kettle with automatic shutoff and temperature control for tea, coffee and everyday kitchen use.",
            "electric kettle kitchen tea coffee appliance"),

        new(
            "Fitness",
            "Treadmill",
            25000,
            120000,
            "Home treadmill with multiple workout programs, speed controls and digital fitness tracking for cardio training.",
            "treadmill cardio running home gym fitness workout"),

        new(
            "Fitness",
            "Exercise Bike",
            15000,
            60000,
            "Indoor exercise bike with adjustable resistance and digital workout tracking for home cardio and fitness training.",
            "exercise bike cycling cardio home gym fitness"),

        new(
            "Fitness",
            "Adjustable Dumbbell",
            5000,
            25000,
            "Adjustable dumbbell set for strength training, home workouts and full body fitness exercises.",
            "dumbbell strength training home workout fitness weights"),

        new(
            "Fitness",
            "Yoga Mat",
            999,
            5000,
            "Comfortable non-slip yoga mat designed for yoga, stretching, mobility and home fitness workouts.",
            "yoga mat exercise stretching fitness workout"),

        new(
            "Fitness",
            "Fitness Tracker",
            2500,
            15000,
            "Fitness tracker for monitoring daily activity, workouts, steps, calories and exercise performance.",
            "fitness tracker activity steps workout health exercise"),

        new(
            "Wearables",
            "Fitness Smart Watch",
            5000,
            35000,
            "Smart watch with fitness tracking, heart rate monitoring, sleep tracking and workout features for active users.",
            "smart watch fitness heart rate sleep workout wearable"),

        new(
            "Wearables",
            "Premium Smart Watch",
            15000,
            70000,
            "Premium smartwatch with advanced notifications, fitness monitoring, health tracking and long battery life.",
            "premium smartwatch wearable fitness notifications health"),

        new(
            "Wearables",
            "Sports Watch",
            8000,
            40000,
            "Rugged sports watch designed for running, cycling, outdoor activities and detailed workout tracking.",
            "sports watch running cycling outdoor fitness GPS"),

        new(
            "Wearables",
            "Smart Ring",
            10000,
            40000,
            "Compact smart ring for activity tracking, sleep monitoring and everyday wellness insights.",
            "smart ring wearable sleep activity wellness"),

        new(
            "Wearables",
            "Fitness Band",
            1500,
            8000,
            "Lightweight fitness band for activity tracking, step counting, sleep monitoring and everyday exercise.",
            "fitness band activity tracker steps sleep wearable"),

        new(
            "Mobile",
            "Camera Smartphone",
            25000,
            120000,
            "Smartphone with advanced camera system, high resolution photography, low light performance and powerful mobile processing.",
            "smartphone camera phone photography mobile low light"),

        new(
            "Mobile",
            "Business Smartphone",
            20000,
            90000,
            "Professional smartphone with strong battery life, secure features, fast performance and productivity tools for business users.",
            "business smartphone productivity mobile office professional"),

        new(
            "Mobile",
            "Gaming Smartphone",
            25000,
            100000,
            "High performance gaming smartphone with fast display, powerful processor, advanced cooling and long battery life.",
            "gaming smartphone mobile gaming high refresh processor"),

        new(
            "Mobile",
            "Budget Smartphone",
            8000,
            25000,
            "Affordable smartphone with reliable performance, long battery life, large display and modern connectivity.",
            "budget smartphone affordable mobile phone battery"),

        new(
            "Mobile",
            "Smartphone Power Bank",
            1200,
            6000,
            "Portable high capacity power bank for charging smartphones, tablets and mobile devices while traveling.",
            "power bank smartphone charger portable mobile travel")
    ];

    public static IReadOnlyList<VectorProduct> Generate(int count = 1000)
    {
        var products = new List<VectorProduct>(count);

        for (int i = 0; i < count; i++)
        {
            ProductTemplate template = Templates[i % Templates.Length];

            int variantNumber = (i / Templates.Length) + 1;

            decimal price = CalculatePrice(
                template.MinPrice,
                template.MaxPrice,
                variantNumber);

            string adjective =
                Adjectives[i % Adjectives.Length];

            string color =
                Colors[i % Colors.Length];

            string name =
                $"{adjective} {template.Name} {variantNumber}";

            string description =
                $"{template.Description} " +
                $"Available in {color}. " +
                $"Suitable for modern users looking for reliable " +
                $"performance, quality and practical everyday use.";

            string searchText =
                $"""
                Product: {name}
                Category: {template.Category}
                Description: {description}
                Keywords: {template.Keywords}
                Color: {color}
                Use cases: {template.Keywords}
                """;

            products.Add(
                new VectorProduct
                {
                    Id = $"catalog-{i + 1:0000}",
                    Name = name,
                    Category = template.Category,
                    Price = price,
                    Description = description,
                    SearchText = searchText
                });
        }

        return products;
    }

    private static decimal CalculatePrice(
        decimal minimum,
        decimal maximum,
        int variant)
    {
        decimal range = maximum - minimum;

        // Deterministic price variation.
        decimal percentage =
            ((variant * 37) % 100) / 100m;

        return Math.Round(
            minimum + (range * percentage),
            0);
    }

    private sealed record ProductTemplate(
        string Category,
        string Name,
        decimal MinPrice,
        decimal MaxPrice,
        string Description,
        string Keywords);
}