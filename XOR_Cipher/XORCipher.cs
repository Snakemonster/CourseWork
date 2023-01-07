using System.Text;

namespace XOR_Cipher;

public class XORCipher
{
    public static string GetRandomKey(int key, int length)
    {
        var gamma = new StringBuilder();
        var rnd = new Random(key);
        for (int i = 0; i < length; i++) gamma.Append((char)rnd.Next(33, 126));
        return gamma.ToString();
    }
    private string GetRepeatKey(string s, int n)
    {
        var r = new StringBuilder(s);
        while (r.Length < n) r.Append(r);
        return r.ToString()[..n];
    }

    private string Cipher(string text, string secretKey)
    {
        var currentKey = GetRepeatKey(secretKey, text.Length);
        var res = new StringBuilder();
        for (var i = 0; i < text.Length; i++) res.Append((char)(text[i] ^ currentKey[i]));
        return res.ToString();
    }

    public string Encrypt(string plainText, string password) => Cipher(plainText, password);
    
    public string Decrypt(string encryptedText, string password) => Cipher(encryptedText, password);
}