namespace ClinicaAcolher.Application.Common;

/// <summary>
/// Cifra e decifra campos sensíveis (RN-PRT-02, RN-SEG-07).
/// O contexto liga o dado cifrado ao registro dono dele, por exemplo "clinical-note:{id}".
/// </summary>
public interface IFieldEncryptor
{
    byte[] Encrypt(string plaintext, string context);
    string Decrypt(byte[] encrypted, string context);
}