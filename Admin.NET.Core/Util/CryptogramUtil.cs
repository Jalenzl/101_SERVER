namespace Admin.NET.Core;

public class CryptogramUtil
{
    public static readonly bool StrongPassword = App.GetConfig<bool>("Cryptogram:StrongPassword");
    public static readonly string PasswordStrengthValidation = App.GetConfig<string>("Cryptogram:PasswordStrengthValidation");
    public static readonly string PasswordStrengthValidationMsg = App.GetConfig<string>("Cryptogram:PasswordStrengthValidationMsg");
    public static readonly string CryptoType = App.GetConfig<string>("Cryptogram:CryptoType");
    public static readonly string PublicKey = App.GetConfig<string>("Cryptogram:PublicKey");
    public static readonly string PrivateKey = App.GetConfig<string>("Cryptogram:PrivateKey");
    public static readonly bool EnableLoginFail = App.GetConfig<bool>("Cryptogram:EnableLoginFail");
    public static readonly int FailCount = App.GetConfig<int>("Cryptogram:FailCount");
    public static readonly int LockMinutes = App.GetConfig<int>("Cryptogram:LockMinutes");
    public static readonly bool EnablePasswordExpire = App.GetConfig<bool>("Cryptogram:EnablePasswordExpire");
    public static readonly int PasswordValidityPeriod = App.GetConfig<int>("Cryptogram:PasswordValidityPeriod");

    public static readonly string SM4_key = "0123456789abcdeffedcba9876543210";
    public static readonly string SM4_iv = "595298c7c6fd271f0402f804c33d3f66";

    public static string Encrypt(string plainText)
    {
        if (CryptoType == CryptogramEnum.MD5.ToString()) return MD5Encryption.Encrypt(plainText);
        if (CryptoType == CryptogramEnum.SM2.ToString()) return SM2Encrypt(plainText);
        if (CryptoType == CryptogramEnum.SM4.ToString()) return SM4EncryptECB(plainText);
        return plainText;
    }

    public static string Decrypt(string cipherText)
    {
        if (CryptoType == CryptogramEnum.SM2.ToString()) return SM2Decrypt(cipherText);
        if (CryptoType == CryptogramEnum.SM4.ToString()) return SM4DecryptECB(cipherText);
        return cipherText;
    }

    public static string SM2Encrypt(string plainText) => GMUtil.SM2Encrypt(PublicKey, plainText);

    public static string SM2Decrypt(string cipherText) => GMUtil.SM2Decrypt(PrivateKey, cipherText);

    public static string SM4EncryptECB(string plainText) => GMUtil.SM4EncryptECB(SM4_key, plainText);

    public static string SM4DecryptECB(string cipherText) => GMUtil.SM4DecryptECB(SM4_key, cipherText);

    public static string SM4EncryptCBC(string plainText) => GMUtil.SM4EncryptCBC(SM4_key, SM4_iv, plainText);

    public static string SM4DecryptCBC(string cipherText) => GMUtil.SM4DecryptCBC(SM4_key, SM4_iv, cipherText);
}
