using CoffeeNChillGatewayAPI.Services;

namespace CoffeeNChillGatewayAPI.Tests
{
    public class PasswordHashingTests   // Microsoft, 2026ae; OWASP Foundation, 2026a
    {
        [Fact]
        public void HashPassword_NeverStoresThePlainTextPassword()
        {
            string passwordHash = AuthService.HashPassword("Barista@2026");

            Assert.NotEqual("Barista@2026", passwordHash);
            Assert.StartsWith("$2", passwordHash);
        }

        [Fact]
        public void VerifyPassword_AcceptsTheCorrectPassword()
        {
            string passwordHash = AuthService.HashPassword("Barista@2026");

            Assert.True(AuthService.VerifyPassword("Barista@2026", passwordHash));
        }

        [Fact]
        public void VerifyPassword_RejectsAWrongPassword()
        {
            string passwordHash = AuthService.HashPassword("Barista@2026");

            Assert.False(AuthService.VerifyPassword("barista@2026", passwordHash));
        }

        [Fact]
        public void HashPassword_UsesADifferentSaltEachTime()
        {
            string firstHash = AuthService.HashPassword("Barista@2026");
            string secondHash = AuthService.HashPassword("Barista@2026");

            Assert.NotEqual(firstHash, secondHash);
        }
    }
}
