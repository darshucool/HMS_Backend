using System.Security.Cryptography;

namespace HMS.Modules.Identity.Application.Authentication;

internal static class CredentialGenerator
{
    public static string TemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        Span<byte> bytes = stackalloc byte[12];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[14];
        for (var i = 0; i < 12; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];

        chars[12] = '@';
        chars[13] = '1';
        return new string(chars);
    }
}
