using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace Ecom.Cms.Web.lib
{
    public static class LibSecurity
    {
        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                // 1. Chuyển chuỗi verifier sang mảng byte ASCII
                var challengeBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));

                // 2. Encode mảng byte sang Base64Url (không dùng Base64 thường vì có ký tự đặc biệt)
                return WebEncoders.Base64UrlEncode(challengeBytes);
            }
        }
    }
}
