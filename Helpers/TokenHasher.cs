using System.Security.Cryptography;
using System.Text;

namespace LibraryApi.Helpers;

public static class TokenHasher
{
    public static string Hash(string token)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] bytes =
            Encoding.UTF8.GetBytes(token);

        byte[] hash =
            sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}