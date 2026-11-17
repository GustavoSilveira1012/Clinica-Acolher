using System.Security.Cryptography;
using System.Text;
using ClinicaAcolher.Application.Common;
using Microsoft.Extensions.Options;

namespace ClinicaAcolher.Infrastructure.Security;

/// <summary>Criptografia de envelope com AES-256-GCM (PROJETO.md, seção 13.4).</summary>
public sealed class AesGcmFieldEncryptor : IFieldEncryptor
{
    private const byte FormatVersion = 1;
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly string _activeKeyId;
    private readonly Dictionary<string, byte[]> _keys;

    public AesGcmFieldEncryptor(IOptions<EncryptionOptions> options)
    {
        var config = options.Value;
        if (!config.IsValid())
            throw new InvalidOperationException("Configuração de criptografia inválida.");

        _activeKeyId = config.ActiveKeyId;
        _keys = config.Keys.ToDictionary(k => k.Key, k => Convert.FromBase64String(k.Value));
    }

    public byte[] Encrypt(string plaintext, string context)
    {
        var associatedData = Encoding.UTF8.GetBytes(context);
        var dataKey = RandomNumberGenerator.GetBytes(KeySize);

        try
        {
            // 1. A chave de dados é cifrada ("embrulhada") com a chave mestra
            var (keyNonce, wrappedKey, keyTag) = Seal(_keys[_activeKeyId], dataKey, associatedData);

            // 2. O texto é cifrado com a chave de dados
            var (dataNonce, ciphertext, dataTag) = Seal(dataKey, Encoding.UTF8.GetBytes(plaintext), associatedData);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(FormatVersion);
            writer.Write(_activeKeyId);
            writer.Write(keyNonce);
            writer.Write(wrappedKey);
            writer.Write(keyTag);
            writer.Write(dataNonce);
            writer.Write(dataTag);
            writer.Write(ciphertext);
            writer.Flush();
            return stream.ToArray();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    public string Decrypt(byte[] encrypted, string context)
    {
        var associatedData = Encoding.UTF8.GetBytes(context);
        using var reader = new BinaryReader(new MemoryStream(encrypted));

        if (reader.ReadByte() != FormatVersion)
            throw new CryptographicException("Formato de dado cifrado desconhecido.");

        var keyId = reader.ReadString();
        if (!_keys.TryGetValue(keyId, out var masterKey))
            throw new InvalidOperationException($"A chave mestra '{keyId}' não está configurada.");

        var keyNonce = reader.ReadBytes(NonceSize);
        var wrappedKey = reader.ReadBytes(KeySize);
        var keyTag = reader.ReadBytes(TagSize);
        var dataNonce = reader.ReadBytes(NonceSize);
        var dataTag = reader.ReadBytes(TagSize);
        var ciphertext = reader.ReadBytes(encrypted.Length - (int)reader.BaseStream.Position);

        var dataKey = Open(masterKey, keyNonce, wrappedKey, keyTag, associatedData);
        try
        {
            return Encoding.UTF8.GetString(Open(dataKey, dataNonce, ciphertext, dataTag, associatedData));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    private static (byte[] Nonce, byte[] Ciphertext, byte[] Tag) Seal(byte[] key, byte[] plaintext, byte[] associatedData)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);
        return (nonce, ciphertext, tag);
    }

    private static byte[] Open(byte[] key, byte[] nonce, byte[] ciphertext, byte[] tag, byte[] associatedData)
    {
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
        return plaintext;
    }
}