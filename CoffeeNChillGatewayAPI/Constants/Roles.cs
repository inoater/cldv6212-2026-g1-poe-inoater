namespace CoffeeNChillGatewayAPI.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";

        public const string Staff = "Staff";

        public const string Customer = "Customer";

        public const string AdminOrStaff = Admin + "," + Staff;

        public static readonly string[] All = { Admin, Staff, Customer };
    }
}
