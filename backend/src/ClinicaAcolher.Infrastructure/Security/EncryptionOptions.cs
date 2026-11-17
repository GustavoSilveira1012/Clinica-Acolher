namespace ClinicaAcolher.Infrastructure.Security;

public sealed class EncryptionOptions
{
    public const string Section = "Encryption";

    /// <summary>A chave mestra usada para cifrar dados novos.</summary>
    public string ActiveKeyId { get; set; } = string.Empty;

    /// <summary>Todas as chaves mestras conhecidas, em Base64, pelo identificador. As antigas continuam aqui para decifrar dados antigos.</summary>
    public Dictionary<string, string> Keys { get; set; } = new();

    public bool IsValid() => Keys.ContainsKey(ActiveKeyId) && Keys.Values.All(IsValidKey);

    private static bool IsValidKey(string base64)
    {
        try
        {
            return Convert.FromBase64String(base64).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}