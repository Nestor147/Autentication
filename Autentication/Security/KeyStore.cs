using System.Security.Cryptography;
using System.Text.Json;

namespace Autentication.Web.Security;

public static class KeyStore
{
    public sealed record KeyPair(string PrivatePem, string PublicPem, string Kid);
    private sealed record EncFile(string Alg, string Salt, string Nonce, string Cipher, string Tag, string Kid, string CreatedAtUtc);

    public static KeyPair LoadOrCreate(string passphrase, string keyDir, string kid = "k1", int rsaBits = 2048)
    {
        Directory.CreateDirectory(keyDir);
        var privPath = Path.Combine(keyDir, "private.enc");
        var pubPath = Path.Combine(keyDir, "public.pem");

        if (File.Exists(privPath) && File.Exists(pubPath))
        {
            // ✔ evita choque de nombres
            var existingPubPem = File.ReadAllText(pubPath);
            var (existingPrivPem, existingKid) = DecryptPrivatePem(privPath, passphrase);
            return new KeyPair(existingPrivPem, existingPubPem, existingKid ?? kid);
        }

        // Generar par nuevo
        using var rsa = RSA.Create(rsaBits);
        var privatePkcs8 = rsa.ExportPkcs8PrivateKey();
        var publicSpki = rsa.ExportSubjectPublicKeyInfo();

        var privPem = ToPem("PRIVATE KEY", privatePkcs8);
        var pubPem = ToPem("PUBLIC KEY", publicSpki);

        File.WriteAllText(pubPath, pubPem);                       // pública en claro (no secreta)
        EncryptPrivatePem(privPath, passphrase, privatePkcs8, kid); // privada cifrada con passphrase

        return new KeyPair(privPem, pubPem, kid);
    }

    public static (string PrivatePem, string? Kid) DecryptPrivatePem(string privEncPath, string passphrase)
    {
        var json = File.ReadAllText(privEncPath);
        var enc = JsonSerializer.Deserialize<EncFile>(json) ?? throw new InvalidDataException("private.enc corrupta");

        var salt = Convert.FromBase64String(enc.Salt);
        var nonce = Convert.FromBase64String(enc.Nonce);
        var data = Convert.FromBase64String(enc.Cipher);
        var tag = Convert.FromBase64String(enc.Tag);

        using var kdf = new Rfc2898DeriveBytes(passphrase, salt, 200_000, HashAlgorithmName.SHA256);
        var key = kdf.GetBytes(32); // AES‑256

        var plaintext = new byte[data.Length];
        using (var aes = new AesGcm(key)) aes.Decrypt(nonce, data, tag, plaintext);

        var privatePem = ToPem("PRIVATE KEY", plaintext);
        return (privatePem, enc.Kid);
    }

    private static void EncryptPrivatePem(string privEncPath, string passphrase, byte[] privatePkcs8, string kid)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var nonce = RandomNumberGenerator.GetBytes(12);

        using var kdf = new Rfc2898DeriveBytes(passphrase, salt, 200_000, HashAlgorithmName.SHA256);
        var key = kdf.GetBytes(32);

        var cipher = new byte[privatePkcs8.Length];
        var tag = new byte[16];

        using (var aes = new AesGcm(key)) aes.Encrypt(nonce, privatePkcs8, cipher, tag);

        var enc = new EncFile(
            Alg: "AES-GCM+PBKDF2(SHA256,200k)",
            Salt: Convert.ToBase64String(salt),
            Nonce: Convert.ToBase64String(nonce),
            Cipher: Convert.ToBase64String(cipher),
            Tag: Convert.ToBase64String(tag),
            Kid: kid,
            CreatedAtUtc: DateTime.UtcNow.ToString("O")
        );

        File.WriteAllText(privEncPath, JsonSerializer.Serialize(enc, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string ToPem(string label, byte[] der)
    {
        var b64 = Convert.ToBase64String(der, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN {label}-----\n{b64}\n-----END {label}-----";
    }
}
