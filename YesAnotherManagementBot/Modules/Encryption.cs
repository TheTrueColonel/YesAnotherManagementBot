using System.Security.Cryptography;

namespace YAMB.Modules; 

public abstract class Encryption {
    public static string EncryptString(string plainText) {
        using var aes = Aes.Create();

        var encryptor = aes.CreateEncryptor();

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        
        using (var sw = new StreamWriter(cs)) {
            sw.Write(plainText);
        }
        
        var encryptedData = ms.ToArray();

        return Convert.ToBase64String(encryptedData);
    }

    public static string DecryptString(string encryptedText) {
        using var aes = Aes.Create();

        var decryptor = aes.CreateDecryptor();

        var encryptedBytes = Convert.FromBase64String(encryptedText);

        using (MemoryStream ms = new MemoryStream(encryptedBytes))
        {
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}