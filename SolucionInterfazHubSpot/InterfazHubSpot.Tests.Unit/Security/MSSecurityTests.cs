using InterfazHubSpot.Business;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Security
{
    public sealed class MSSecurityTests
    {
        [Fact, Trait("Category", "Security")]
        public void GenerateSaltValue_DevuelveDiezCaracteres()
        {
            var salt = MSSecurity.GenerateSaltValue();
            Assert.Equal(10, salt.Length);
        }

        [Fact, Trait("Category", "Security")]
        public void GenerateHashWithSalt_MismaEntrada_MismoHash()
        {
            const string password = "TestPassword123";
            const string salt = "fixed-salt";

            var hash1 = MSSecurity.GenerateHashWithSalt(password, salt);
            var hash2 = MSSecurity.GenerateHashWithSalt(password, salt);

            Assert.Equal(hash1, hash2);
            Assert.False(string.IsNullOrEmpty(hash1));
        }

        [Fact, Trait("Category", "Security")]
        public void GenerateHashWithSalt_PasswordDistinto_HashDistinto()
        {
            const string salt = "fixed-salt";
            var hash1 = MSSecurity.GenerateHashWithSalt("password-a", salt);
            var hash2 = MSSecurity.GenerateHashWithSalt("password-b", salt);
            Assert.NotEqual(hash1, hash2);
        }

        [Fact, Trait("Category", "Security")]
        public void EncryptDecrypt_RoundTrip_DevuelveTextoOriginal()
        {
            const string plain = "dato-sensible-hubspot";
            var encrypted = MSSecurity.EncryptData(plain);
            var decrypted = MSSecurity.DecryptString(encrypted);
            Assert.Equal(plain, decrypted);
            Assert.NotEqual(plain, encrypted);
        }
    }
}
