using System.Security.Cryptography;
using System.Text;

namespace YAMB.Modules; 

public abstract class Encryption {
    private const int AesBlockByteSize = 128 / 8;

    private const int PasswordSaltByteSize = 128 / 8;
    private const int PasswordByteSize = 256 / 8;
    private const int PasswordIterationCount = 100000;

    private const int SignatureByteSize = 256 / 8;

    private const int MinimumEncryptedMessageByteSize =
        PasswordByteSize + // Auth Size
        PasswordByteSize + // Key Size
        AesBlockByteSize + // IV
        AesBlockByteSize + // Cipher Bytes Min Length
        SignatureByteSize; // Signature Tag

    private static readonly Encoding StringEncoding = Encoding.UTF8;
    private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();
    
    public static byte[] Encrypt(string plainText) {
        // Encrypt
        var keySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var key = GetEncryptionKey(keySalt);
        var iv = GenerateRandomBytes(AesBlockByteSize);

        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor(key, iv);

        var plainBytes = StringEncoding.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Sign
        var authKeySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var authKey = GetSignatureKey(authKeySalt);
        var result = MergeArrays(additionalCapacity: SignatureByteSize, authKeySalt, keySalt, iv, cipherBytes);

        using var hmac = new HMACSHA256(authKey);

        var payloadToSignLength = result.Length - SignatureByteSize;
        var signatureTag = hmac.ComputeHash(result, 0, payloadToSignLength);

        signatureTag.CopyTo(result, payloadToSignLength);
        
        return result;
    }
    
    public static byte[] Encrypt(byte[] plainBytes) {
        // Encrypt
        var keySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var key = GetEncryptionKey(keySalt);
        var iv = GenerateRandomBytes(AesBlockByteSize);

        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor(key, iv);

        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Sign
        var authKeySalt = GenerateRandomBytes(PasswordSaltByteSize);
        var authKey = GetSignatureKey(authKeySalt);
        var result = MergeArrays(additionalCapacity: SignatureByteSize, authKeySalt, keySalt, iv, cipherBytes);

        using var hmac = new HMACSHA256(authKey);

        var payloadToSignLength = result.Length - SignatureByteSize;
        var signatureTag = hmac.ComputeHash(result, 0, payloadToSignLength);

        signatureTag.CopyTo(result, payloadToSignLength);
        
        return result;
    }

    public static string Decrypt(byte[] encryptedData) {
        if (encryptedData.Length < MinimumEncryptedMessageByteSize) {
            throw new ArgumentException("Invalid length of encrypted data.");
        }

        var authKeySalt = encryptedData.AsSpan(0, PasswordSaltByteSize).ToArray();
        var keySalt = encryptedData.AsSpan(PasswordSaltByteSize, PasswordSaltByteSize).ToArray();
        var iv = encryptedData.AsSpan(2 * PasswordSaltByteSize, AesBlockByteSize).ToArray();
        var signatureTag = encryptedData.AsSpan(encryptedData.Length - SignatureByteSize, SignatureByteSize).ToArray();

        var cipherTextIndex = authKeySalt.Length + keySalt.Length + iv.Length;
        var cipherTextLength = encryptedData.Length - cipherTextIndex - signatureTag.Length;

        var authKey = GetSignatureKey(authKeySalt);
        var key = GetEncryptionKey(keySalt);
        
        // Verify Signature
        using var hmac = new HMACSHA256(authKey);

        var payloadToSignLength = encryptedData.Length - SignatureByteSize;
        var signatureTagExpected = hmac.ComputeHash(encryptedData, 0, payloadToSignLength);

        if (!CheckTimingValidation(signatureTag, signatureTagExpected)) {
            throw new CryptographicException("Invalid signature");
        }
        
        // Decrypt
        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor(key, iv);

        var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, cipherTextIndex, cipherTextLength);

        return StringEncoding.GetString(decryptedBytes);
    }

    public static string CreateBasicHash(string text) {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        using var sha = SHA512.Create();

        var textData = Encoding.UTF8.GetBytes(text);
        var hash = sha.ComputeHash(textData);

        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    private static Aes CreateAes() {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        return aes;
    }

    private static byte[] GetEncryptionKey(byte[] salt) {
        var password = GlobalSettings.Instance.EncryptionKey;
        var keyBytes = StringEncoding.GetBytes(password);

        using var derivitor = new Rfc2898DeriveBytes(keyBytes, salt, PasswordIterationCount, HashAlgorithmName.SHA256);

        return derivitor.GetBytes(PasswordByteSize);
    }

    private static byte[] GetSignatureKey(byte[] salt) {
        var password = GlobalSettings.Instance.SignatureKey;
        var keyBytes = StringEncoding.GetBytes(password);

        using var derivitor = new Rfc2898DeriveBytes(keyBytes, salt, PasswordIterationCount, HashAlgorithmName.SHA256);

        return derivitor.GetBytes(PasswordByteSize);
    }

    private static byte[] GenerateRandomBytes(int numberOfBytes) {
        var randomBytes = new byte[numberOfBytes];

        Random.GetBytes(randomBytes);

        return randomBytes;
    }
    
    // Merge the byte arrays into one byte array
    private static byte[] MergeArrays(int additionalCapacity = 0, params byte[][] arrays) {
        var merged = new byte[arrays.Sum(x => x.Length) + additionalCapacity];
        var mergeIndex = 0;

        for (var i = 0; i < arrays.GetLength(0); i++) {
            arrays[i].CopyTo(merged, mergeIndex);
            mergeIndex += arrays[i].Length;
        }

        return merged;
    }
    
    // Returns false if a timing attack is detected
    private static bool CheckTimingValidation(IReadOnlyCollection<byte> a, IReadOnlyList<byte> b) {
        if (a.Count != b.Count) return false;

        return !a.Where((t, i) => t != b[i]).Any();
    }
}