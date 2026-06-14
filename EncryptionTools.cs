using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionTools
{
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("ArcaneWizSalt123!");

    public static void EncryptTextToFile(string plaintext, string outputPath, string password)
    {
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        using (Aes aes = Aes.Create())
        {
            var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);
            aes.IV = deriveBytes.GetBytes(16);

            using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                cs.FlushFinalBlock();
            }
        }
    }

    public static string DecryptFile(string inputPath, string password)
    {
        byte[] cipherBytes = File.ReadAllBytes(inputPath);

        using (Aes aes = Aes.Create())
        {
            var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);
            aes.IV = deriveBytes.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.FlushFinalBlock();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
