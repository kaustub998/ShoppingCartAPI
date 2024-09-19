using System.Text;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;

namespace EcorpAPI.Services
{
    public class CryptoService
    {
        private static readonly string password_salt = "K@usTuB@123";

        public static string CreatePasswordHash(string GuidSaltedPassword)
        {
            var message = string.Concat(GuidSaltedPassword, password_salt);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return hashString;
            }
        }
    }
}
