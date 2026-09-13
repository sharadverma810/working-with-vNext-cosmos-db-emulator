using CosmosLearning.Vector.Api.Features.Products.Models;

namespace CosmosLearning.Vector.Api.Features.Products.Seed;

public static class LargeProductCatalog
{
    private static readonly Random Random = new(42);

    public static IEnumerable<VectorProduct> Generate(int count)
    {
        if (count <= 0)
        {
            yield break;
        }

        var products = ProductTemplates.ToList();

        for (int i = 0; i < count; i++)
        {
            var template = products[i % products.Count];

            int sequence =
                (i / products.Count) + 1;

            yield return CreateProduct(
                template,
                sequence);
        }
    }

    private static VectorProduct CreateProduct(
        ProductTemplate template,
        int sequence)
    {
        decimal price =
            template.BasePrice +
            Random.Next(-500, 1501);

        if (price < 999)
        {
            price = 999;
        }

        string name =
            $"{template.Name} {sequence}";

        string searchText =
            string.Join(
                ". ",
                name,
                template.SearchDescription,
                template.Features,
                template.UseCases,
                template.Keywords);

        return new VectorProduct
        {
            Name = name,
            Category = template.Category,
            Price = price,
            Description = template.Description,
            SearchText = searchText,
            Embedding = []
        };
    }

    private sealed record ProductTemplate(
        string Category,
        string Name,
        decimal BasePrice,
        string Description,
        string SearchDescription,
        string Features,
        string UseCases,
        string Keywords);

    private static readonly ProductTemplate[] ProductTemplates =
    [
        // ============================================================
        // GAMING KEYBOARDS
        // ============================================================

        new(
            "Gaming",
            "Esports Rapid Mechanical Keyboard",
            8999,
            "High-performance mechanical keyboard designed for competitive gaming.",
            "Ultra-fast gaming keyboard built for esports players and competitive PC gaming.",
            "8000Hz polling rate, 0.5ms response time, rapid actuation, anti-ghosting, N-key rollover, RGB lighting.",
            "Competitive gaming, esports tournaments, FPS games, reaction-time sensitive games.",
            "esports fast response competitive gaming rapid actuation anti ghosting mechanical keyboard"),

        new(
            "Gaming",
            "Tournament Mechanical Keyboard",
            10999,
            "Tournament-ready mechanical keyboard with responsive switches.",
            "Professional competitive gaming keyboard optimized for fast and precise key input.",
            "Hot-swappable switches, anti-ghosting, N-key rollover, programmable keys, detachable USB cable.",
            "Esports competitions, professional gaming, FPS, MOBA and competitive multiplayer games.",
            "professional tournament esports competitive precise fast mechanical keyboard"),

        new(
            "Gaming",
            "Low Latency Gaming Keyboard",
            7499,
            "Low-latency keyboard designed for fast gaming input.",
            "Responsive gaming keyboard for players who need quick reaction and minimal input delay.",
            "Low latency switches, 1000Hz polling, anti-ghosting, gaming mode, RGB backlight.",
            "Fast-paced games, FPS gaming, competitive PC gaming.",
            "low latency quick response fast input gaming keyboard"),

        new(
            "Gaming",
            "Quiet Gaming Mechanical Keyboard",
            8499,
            "Mechanical gaming keyboard with quieter switches.",
            "A quieter mechanical keyboard for gaming without sacrificing responsive controls.",
            "Silent tactile switches, anti-ghosting, RGB lighting, programmable macros.",
            "Gaming at night, shared rooms, competitive gaming and quiet environments.",
            "quiet silent mechanical gaming keyboard night gaming"),

        new(
            "Gaming",
            "Wireless Gaming Keyboard",
            11999,
            "Wireless mechanical keyboard designed for gaming and everyday use.",
            "Flexible wireless gaming keyboard offering responsive mechanical typing and freedom from cables.",
            "2.4GHz wireless, Bluetooth, mechanical switches, RGB lighting, rechargeable battery.",
            "Wireless gaming, desk setups, casual and competitive gaming.",
            "wireless bluetooth gaming mechanical keyboard rechargeable"),

        // ============================================================
        // PRODUCTIVITY / OFFICE KEYBOARDS
        // ============================================================

        new(
            "Electronics",
            "Silent Office Keyboard",
            3499,
            "Quiet keyboard designed for office environments.",
            "Comfortable silent keyboard for focused office work and shared workspaces.",
            "Silent switches, low-profile keys, ergonomic layout, spill resistance.",
            "Office work, meetings, libraries, shared workspaces.",
            "silent quiet office keyboard productivity typing"),

        new(
            "Electronics",
            "Low Profile Productivity Keyboard",
            4299,
            "Slim keyboard designed for comfortable everyday productivity.",
            "Low-profile keyboard for developers, writers and professionals who type for long periods.",
            "Low-profile keys, comfortable key spacing, wired USB connection.",
            "Programming, writing, documentation and office productivity.",
            "low profile typing programming developer productivity keyboard"),

        new(
            "Electronics",
            "Wireless Travel Keyboard",
            3999,
            "Compact wireless keyboard designed for portable use.",
            "Lightweight keyboard for professionals who work while travelling.",
            "Bluetooth connectivity, compact layout, rechargeable battery, lightweight design.",
            "Travel, remote work, tablets, laptops and mobile productivity.",
            "portable compact travel bluetooth wireless keyboard"),

        new(
            "Electronics",
            "Ergonomic Productivity Keyboard",
            6999,
            "Ergonomic keyboard designed for long typing sessions.",
            "Comfort-focused keyboard for programmers and professionals who type throughout the day.",
            "Split layout, wrist support, ergonomic key positioning, programmable shortcuts.",
            "Programming, writing, data entry and long working sessions.",
            "ergonomic comfortable programming keyboard long typing"),

        // ============================================================
        // MICE
        // ============================================================

        new(
            "Gaming",
            "Esports Lightweight Gaming Mouse",
            5999,
            "Lightweight gaming mouse designed for competitive players.",
            "Ultra-light mouse optimized for fast flicks and precise competitive aiming.",
            "59g weight, 26000 DPI sensor, 1000Hz polling, low-latency wireless.",
            "FPS games, esports, competitive aiming and fast mouse movement.",
            "esports lightweight gaming mouse fast precise FPS"),

        new(
            "Gaming",
            "Precision FPS Gaming Mouse",
            6499,
            "High-precision mouse designed for FPS gaming.",
            "Accurate gaming mouse for players who need precise aim and consistent tracking.",
            "High DPI optical sensor, adjustable sensitivity, low click latency.",
            "FPS, tactical shooters, competitive gaming.",
            "precision accurate FPS gaming mouse competitive aim"),

        new(
            "Electronics",
            "Silent Office Mouse",
            2299,
            "Quiet wireless mouse designed for office productivity.",
            "Comfortable mouse with quiet clicks for offices and shared environments.",
            "Silent buttons, ergonomic shape, Bluetooth, long battery life.",
            "Office work, meetings, libraries and quiet workspaces.",
            "silent quiet office mouse productivity bluetooth"),

        new(
            "Electronics",
            "Ergonomic Vertical Mouse",
            3299,
            "Vertical ergonomic mouse designed to reduce wrist strain.",
            "Comfort-oriented mouse for professionals working at a computer for extended periods.",
            "Vertical grip, adjustable DPI, ergonomic thumb rest.",
            "Programming, office work and long computer sessions.",
            "ergonomic vertical mouse comfortable wrist programming office"),

        // ============================================================
        // LAPTOPS
        // ============================================================

        new(
            "Computers",
            "Developer Performance Laptop",
            79999,
            "High-performance laptop designed for software development.",
            "Powerful programming laptop for developers running IDEs, containers and development tools.",
            "32GB RAM, 1TB SSD, high-performance processor, 15-inch display.",
            "Software development, coding, Docker, IDEs, backend development.",
            "developer programming software development coding laptop IDE"),

        new(
            "Computers",
            "Mobile Developer Laptop",
            69999,
            "Portable laptop designed for developers who work remotely.",
            "Lightweight programming laptop balancing performance and portability.",
            "16GB RAM, 1TB SSD, efficient processor, lightweight chassis.",
            "Remote development, travel, programming and cloud development.",
            "portable developer programming laptop remote work travel"),

        new(
            "Computers",
            "AI Development Laptop",
            119999,
            "High-performance laptop designed for AI and machine learning development.",
            "Powerful development machine for local AI experimentation and machine learning workflows.",
            "64GB RAM, dedicated GPU, 2TB SSD, high-end processor.",
            "Machine learning, AI development, Python, data science and model experimentation.",
            "AI machine learning GPU developer laptop Python data science"),

        new(
            "Computers",
            "Business Productivity Laptop",
            59999,
            "Reliable laptop designed for business productivity.",
            "Business laptop for documents, spreadsheets, video meetings and everyday professional work.",
            "16GB RAM, 512GB SSD, long battery life, webcam.",
            "Business work, spreadsheets, presentations, meetings and remote work.",
            "business office productivity laptop meetings remote work"),

        // ============================================================
        // MONITORS
        // ============================================================

        new(
            "Electronics",
            "Esports High Refresh Monitor",
            24999,
            "High-refresh gaming monitor designed for competitive play.",
            "Fast gaming display optimized for esports and competitive FPS gaming.",
            "240Hz refresh rate, 1ms response time, adaptive sync.",
            "Esports, FPS games, competitive gaming and fast motion.",
            "240Hz high refresh fast response esports gaming monitor"),

        new(
            "Electronics",
            "4K Productivity Monitor",
            32999,
            "High-resolution monitor for professional productivity.",
            "Sharp 4K display for developers, office professionals and content creators.",
            "4K resolution, USB-C, 32-inch panel, ergonomic stand.",
            "Programming, documents, spreadsheets and productivity.",
            "4K monitor productivity programming office USB-C"),

        new(
            "Electronics",
            "Color Accurate Creator Monitor",
            44999,
            "Professional monitor designed for content creation.",
            "Color-accurate display for photographers, designers and video creators.",
            "Wide color gamut, factory calibration, 4K resolution.",
            "Photo editing, video editing, graphic design and content creation.",
            "color accurate monitor photography design video editing creator"),

        // ============================================================
        // HEADPHONES
        // ============================================================

        new(
            "Audio",
            "Competitive Gaming Headset",
            8999,
            "Gaming headset designed for competitive players.",
            "Gaming headset with precise positional audio for competitive multiplayer games.",
            "Low-latency audio, directional sound, noise-isolating microphone.",
            "FPS games, esports, competitive multiplayer.",
            "competitive gaming headset positional audio esports FPS"),

        new(
            "Audio",
            "Noise Cancelling Office Headphones",
            12999,
            "Noise cancelling headphones designed for focused work.",
            "Comfortable headphones for meetings and concentration in noisy environments.",
            "Active noise cancellation, microphone, Bluetooth, long battery life.",
            "Office work, travel, video meetings and focused productivity.",
            "noise cancelling office headphones meetings productivity travel"),

        new(
            "Audio",
            "Studio Monitoring Headphones",
            15999,
            "Professional headphones designed for accurate audio monitoring.",
            "Detailed studio headphones for music production and audio editing.",
            "Flat frequency response, wired connection, over-ear design.",
            "Music production, mixing, recording and audio editing.",
            "studio monitoring headphones music production mixing recording"),

        // ============================================================
        // CAMERAS
        // ============================================================

        new(
            "Cameras",
            "Travel Mirrorless Camera",
            74999,
            "Compact mirrorless camera designed for travel photography.",
            "Lightweight interchangeable-lens camera for photographers travelling frequently.",
            "24MP sensor, image stabilization, compact body, 4K video.",
            "Travel photography, street photography and everyday shooting.",
            "travel photography mirrorless compact camera lightweight"),

        new(
            "Cameras",
            "Professional Portrait Camera",
            129999,
            "High-resolution mirrorless camera designed for professional photography.",
            "Professional camera optimized for portraits and detailed still photography.",
            "Full-frame sensor, high resolution, eye autofocus, RAW support.",
            "Portrait photography, professional photography and studio work.",
            "professional portrait camera full frame photography RAW"),

        new(
            "Cameras",
            "Action Adventure Camera",
            29999,
            "Rugged camera designed for outdoor adventures.",
            "Compact action camera for sports, travel and outdoor activities.",
            "4K video, waterproof body, stabilization, wide-angle lens.",
            "Adventure travel, cycling, hiking, sports and underwater shooting.",
            "action camera adventure travel sports waterproof 4K"),

        // ============================================================
        // CHAIRS
        // ============================================================

        new(
            "Furniture",
            "Ergonomic Coding Chair",
            14999,
            "Ergonomic office chair designed for long programming sessions.",
            "Comfortable chair for developers who sit at a computer throughout the day.",
            "Adjustable lumbar support, breathable mesh, adjustable height, padded seat.",
            "Programming, software development, remote work and long desk sessions.",
            "comfortable chair programming coding developer ergonomic long hours"),

        new(
            "Furniture",
            "Executive Office Chair",
            18999,
            "Premium office chair designed for professional workspaces.",
            "Supportive executive chair for long business and office sessions.",
            "High-back design, padded cushioning, adjustable armrests and lumbar support.",
            "Office work, management, meetings and professional workspaces.",
            "executive office chair comfortable business high back"),

        new(
            "Furniture",
            "Compact Home Office Chair",
            8999,
            "Space-efficient ergonomic chair for home offices.",
            "Compact comfortable chair for working from home in smaller rooms.",
            "Adjustable height, compact frame, breathable back support.",
            "Home office, remote work, study and apartment workspaces.",
            "compact home office chair remote work small space"),

        new(
            "Furniture",
            "Gaming Chair",
            16999,
            "Supportive gaming chair designed for extended gaming sessions.",
            "Comfortable gaming seat with strong back support for long gaming sessions.",
            "High back, adjustable armrests, lumbar cushion, reclining back.",
            "Gaming, esports practice and long PC sessions.",
            "gaming chair esports comfortable long sessions lumbar"),

        // ============================================================
        // DESKS
        // ============================================================

        new(
            "Furniture",
            "Developer Standing Desk",
            21999,
            "Height-adjustable standing desk designed for developers.",
            "Flexible workstation for programmers who alternate between sitting and standing.",
            "Electric height adjustment, memory presets, cable management.",
            "Programming, remote work and ergonomic office setups.",
            "standing desk developer programming ergonomic workstation"),

        new(
            "Furniture",
            "Gaming Desk",
            12999,
            "Large gaming desk designed for PC gaming setups.",
            "Spacious gaming workstation for monitors, keyboard, mouse and gaming accessories.",
            "Large surface, cable management, headphone hook.",
            "Gaming setups, streaming and esports practice.",
            "gaming desk esports streaming PC setup large"),

        // ============================================================
        // FITNESS
        // ============================================================

        new(
            "Fitness",
            "Running Smart Watch",
            18999,
            "Smart watch designed for runners and fitness enthusiasts.",
            "Fitness watch focused on running performance and workout tracking.",
            "GPS, heart-rate tracking, running metrics, workout modes.",
            "Running, jogging, cardio workouts and fitness tracking.",
            "running fitness watch GPS workout tracking"),

        new(
            "Fitness",
            "Adventure Fitness Watch",
            24999,
            "Rugged fitness watch designed for outdoor activities.",
            "Durable GPS watch for hiking, cycling and outdoor training.",
            "GPS, altimeter, compass, long battery life, water resistance.",
            "Hiking, cycling, trekking and outdoor fitness.",
            "adventure fitness watch hiking cycling GPS outdoor"),

        // ============================================================
        // STORAGE
        // ============================================================

        new(
            "Electronics",
            "Portable External SSD",
            7999,
            "Fast portable SSD designed for transferring large files.",
            "Compact high-speed external storage for professionals and creators.",
            "1TB capacity, USB-C, high sequential transfer speed.",
            "Video editing, backups, photography and large file transfers.",
            "portable SSD fast storage USB-C video editing backup"),

        new(
            "Electronics",
            "High Capacity Backup Drive",
            11999,
            "Large external drive designed for backups and archives.",
            "Reliable storage for photographers, businesses and home backups.",
            "4TB capacity, USB connectivity, backup software support.",
            "Backup, photo storage, document archives and media libraries.",
            "backup external drive high capacity storage archive")
    ];
}