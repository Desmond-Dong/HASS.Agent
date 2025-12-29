using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace HASS.Agent.Managers
{
    /// <summary>
    /// Provides secure storage for sensitive data using Windows DPAPI
    /// </summary>
    public class SecureStorageManager
    {
        private static readonly byte[] _entropy = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// Encrypts sensitive data using Windows DPAPI
        /// </summary>
        /// <param name="plainText">The plain text to encrypt</param>
        /// <returns>Base64-encoded encrypted data</returns>
        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error encrypting data: {Err}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts data that was encrypted using Windows DPAPI
        /// </summary>
        /// <param name="encryptedText">Base64-encoded encrypted data</param>
        /// <returns>Decrypted plain text</returns>
        public static string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var plainBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error decrypting data: {Err}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Stores a sensitive value securely
        /// </summary>
        /// <param name="key">The key to identify the value</param>
        /// <param name="value">The sensitive value to store</param>
        public static bool StoreSecureValue(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    return false;

                var encrypted = EncryptString(value);
                if (string.IsNullOrEmpty(encrypted))
                    return false;

                // Store in a secure location (e.g., isolated storage or registry)
                // For now, we'll use Environment.SpecialFolder.ApplicationData
                var secureFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage");

                if (!Directory.Exists(secureFolder))
                    Directory.CreateDirectory(secureFolder);

                var secureFile = Path.Combine(secureFolder, $"{key}.secure");
                File.WriteAllText(secureFile, encrypted);

                Log.Information("[SECURESTORAGE] Stored secure value for key: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error storing secure value for key {Key}: {Err}", key, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a sensitive value securely
        /// </summary>
        /// <param name="key">The key to identify the value</param>
        /// <returns>The decrypted value, or empty string if not found</returns>
        public static string GetSecureValue(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return string.Empty;

                var secureFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage",
                    $"{key}.secure");

                if (!File.Exists(secureFile))
                    return string.Empty;

                var encrypted = File.ReadAllText(secureFile);
                var decrypted = DecryptString(encrypted);

                Log.Debug("[SECURESTORAGE] Retrieved secure value for key: {Key}", key);
                return decrypted;
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error retrieving secure value for key {Key}: {Err}", key, ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes a secure value
        /// </summary>
        /// <param name="key">The key to identify the value</param>
        public static bool DeleteSecureValue(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                var secureFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage",
                    $"{key}.secure");

                if (File.Exists(secureFile))
                {
                    File.Delete(secureFile);
                    Log.Information("[SECURESTORAGE] Deleted secure value for key: {Key}", key);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error deleting secure value for key {Key}: {Err}", key, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks if a secure value exists
        /// </summary>
        /// <param name="key">The key to check</param>
        public static bool HasSecureValue(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                var secureFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage",
                    $"{key}.secure");

                return File.Exists(secureFile);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lists all stored secure keys
        /// </summary>
        public static List<string> ListSecureKeys()
        {
            try
            {
                var secureFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage");

                if (!Directory.Exists(secureFolder))
                    return new List<string>();

                return Directory.GetFiles(secureFolder, "*.secure")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(name => name != null)
                    .ToList()!;
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error listing secure keys: {Err}", ex.Message);
                return new List<string>();
            }
        }

        /// <summary>
        /// Clears all secure storage
        /// </summary>
        public static bool ClearAllSecureValues()
        {
            try
            {
                var secureFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HASS.Agent",
                    "SecureStorage");

                if (Directory.Exists(secureFolder))
                {
                    Directory.Delete(secureFolder, recursive: true);
                    Log.Warning("[SECURESTORAGE] Cleared all secure values");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error("[SECURESTORAGE] Error clearing secure values: {Err}", ex.Message);
                return false;
            }
        }
    }
}
