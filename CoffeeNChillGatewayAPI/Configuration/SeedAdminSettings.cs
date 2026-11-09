namespace CoffeeNChillGatewayAPI.Configuration
{
    public class SeedAdminSettings  // Microsoft, 2026ab
    {
        public const string SectionName = "SeedAdmin";

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = "Canteen Manager";
    }
}
