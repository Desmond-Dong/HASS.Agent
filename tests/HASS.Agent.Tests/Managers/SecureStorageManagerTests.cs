using Xunit;
using FluentAssertions;
using HASS.Agent.Managers;

namespace HASS.Agent.Tests.Managers
{
    public class SecureStorageManagerTests
    {
        private const string TestKey = "test_key";
        private const string TestValue = "test_sensitive_value";

        [Fact]
        public void EncryptString_DecryptString_ShouldReturnOriginalValue()
        {
            // Arrange
            var original = "Hello, World!";

            // Act
            var encrypted = SecureStorageManager.EncryptString(original);
            var decrypted = SecureStorageManager.DecryptString(encrypted);

            // Assert
            decrypted.Should().Be(original);
        }

        [Fact]
        public void EncryptString_EmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            var empty = "";

            // Act
            var encrypted = SecureStorageManager.EncryptString(empty);

            // Assert
            encrypted.Should().Be(empty);
        }

        [Fact]
        public void EncryptString_Null_ShouldReturnEmptyString()
        {
            // Arrange
            string? nullString = null;

            // Act
            var encrypted = SecureStorageManager.EncryptString(nullString!);

            // Assert
            encrypted.Should().Be("");
        }

        [Fact]
        public void StoreSecureValue_GetSecureValue_ShouldStoreAndRetrieve()
        {
            // Arrange
            SecureStorageManager.DeleteSecureValue(TestKey); // Clean up first

            // Act
            var stored = SecureStorageManager.StoreSecureValue(TestKey, TestValue);
            var retrieved = SecureStorageManager.GetSecureValue(TestKey);

            // Assert
            stored.Should().BeTrue();
            retrieved.Should().Be(TestValue);

            // Cleanup
            SecureStorageManager.DeleteSecureValue(TestKey);
        }

        [Fact]
        public void StoreSecureValue_WithComplexValue_ShouldStoreAndRetrieve()
        {
            // Arrange
            var complexValue = "Password@123!#$%";

            // Act
            SecureStorageManager.StoreSecureValue(TestKey, complexValue);
            var retrieved = SecureStorageManager.GetSecureValue(TestKey);

            // Assert
            retrieved.Should().Be(complexValue);

            // Cleanup
            SecureStorageManager.DeleteSecureValue(TestKey);
        }

        [Fact]
        public void HasSecureValue_ExistingKey_ShouldReturnTrue()
        {
            // Arrange
            SecureStorageManager.StoreSecureValue(TestKey, TestValue);

            // Act
            var hasValue = SecureStorageManager.HasSecureValue(TestKey);

            // Assert
            hasValue.Should().BeTrue();

            // Cleanup
            SecureStorageManager.DeleteSecureValue(TestKey);
        }

        [Fact]
        public void HasSecureValue_NonExistingKey_ShouldReturnFalse()
        {
            // Act
            var hasValue = SecureStorageManager.HasSecureValue("non_existing_key");

            // Assert
            hasValue.Should().BeFalse();
        }

        [Fact]
        public void DeleteSecureValue_ExistingKey_ShouldReturnTrue()
        {
            // Arrange
            SecureStorageManager.StoreSecureValue(TestKey, TestValue);

            // Act
            var deleted = SecureStorageManager.DeleteSecureValue(TestKey);
            var hasValue = SecureStorageManager.HasSecureValue(TestKey);

            // Assert
            deleted.Should().BeTrue();
            hasValue.Should().BeFalse();
        }

        [Fact]
        public void DeleteSecureValue_NonExistingKey_ShouldReturnFalse()
        {
            // Act
            var deleted = SecureStorageManager.DeleteSecureValue("non_existing_key");

            // Assert
            deleted.Should().BeFalse();
        }

        [Fact]
        public void ListSecureKeys_WithStoredValues_ShouldReturnKeys()
        {
            // Arrange
            SecureStorageManager.StoreSecureValue("key1", "value1");
            SecureStorageManager.StoreSecureValue("key2", "value2");
            SecureStorageManager.StoreSecureValue("key3", "value3");

            // Act
            var keys = SecureStorageManager.ListSecureKeys();

            // Assert
            keys.Should().Contain("key1");
            keys.Should().Contain("key2");
            keys.Should().Contain("key3");

            // Cleanup
            SecureStorageManager.DeleteSecureValue("key1");
            SecureStorageManager.DeleteSecureValue("key2");
            SecureStorageManager.DeleteSecureValue("key3");
        }
    }
}
