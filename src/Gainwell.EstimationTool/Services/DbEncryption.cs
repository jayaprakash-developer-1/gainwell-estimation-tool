using System.IO;
using System.Security.Cryptography;

namespace Gainwell.EstimationTool.Services;

/// <summary>
/// Manages SQLite database encryption key using Windows DPAPI (CurrentUser scope).
/// The key is generated once per user and stored DPAPI-encrypted in %LOCALAPPDATA%.
/// Only the current Windows user can decrypt the stored key.
/// </summary>
public static class DbEncryption
{
    private static readonly string KeyFilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Gainwell", "EstimationTool", ".dbkey");

    /// <summary>
    /// Retrieves existing encryption key or generates a new 256-bit key protected with DPAPI.
    /// </summary>
    public static string GetOrCreateKey()
    {
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(KeyFilePath)!);

        if (System.IO.File.Exists(KeyFilePath))
        {
            var protectedBytes = System.IO.File.ReadAllBytes(KeyFilePath);
            var keyBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(keyBytes);
        }

        var newKey = RandomNumberGenerator.GetBytes(32);
        var encrypted = ProtectedData.Protect(newKey, null, DataProtectionScope.CurrentUser);
        System.IO.File.WriteAllBytes(KeyFilePath, encrypted);
        return Convert.ToBase64String(newKey);
    }
}
