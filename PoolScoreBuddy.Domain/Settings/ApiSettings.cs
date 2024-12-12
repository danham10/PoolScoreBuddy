using System.Text;

namespace PoolScoreBuddy.Domain;

public static class ApiSettings
{
    internal static string SecretKey = "6ceccd7405ef4b00b2630009be568cfa";
    public static byte[] GenerateSecretByte() =>
        Encoding.ASCII.GetBytes(SecretKey);
}
