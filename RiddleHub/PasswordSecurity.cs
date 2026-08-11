using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RiddleHub.Security
{
    public enum PasswordVerificationResult
    {
        Failed,
        Success,
        SuccessRehashNeeded,
        LegacyPlaintextSuccess
    }

    public static class PasswordSecurity
    {
        private const string FormatName = "pbkdf2-sha256";
        private const int IterationCount = 150000;
        private const int SaltSizeBytes = 16;
        private const int DerivedKeySizeBytes = 32;

        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt = new byte[SaltSizeBytes];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash = DeriveKey(password, salt, IterationCount, DerivedKeySizeBytes);
            return string.Join(
                "$",
                FormatName,
                IterationCount.ToString(CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static string CreateUnusablePasswordHash()
        {
            byte[] randomSecret = new byte[DerivedKeySizeBytes];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(randomSecret);
            }
            return HashPassword(Convert.ToBase64String(randomSecret));
        }

        public static PasswordVerificationResult Verify(string suppliedPassword, string storedPassword)
        {
            if (suppliedPassword == null || string.IsNullOrEmpty(storedPassword))
            {
                return PasswordVerificationResult.Failed;
            }

            if (!storedPassword.StartsWith(FormatName + "$", StringComparison.Ordinal))
            {
                return FixedTimeEqualsUtf8(suppliedPassword, storedPassword)
                    ? PasswordVerificationResult.LegacyPlaintextSuccess
                    : PasswordVerificationResult.Failed;
            }

            try
            {
                string[] parts = storedPassword.Split('$');
                if (parts.Length != 4 || !string.Equals(parts[0], FormatName, StringComparison.Ordinal))
                {
                    return PasswordVerificationResult.Failed;
                }

                int iterations;
                if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out iterations) ||
                    iterations < 10000 || iterations > 1000000)
                {
                    return PasswordVerificationResult.Failed;
                }

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                if (salt.Length < SaltSizeBytes || expected.Length < 16 || expected.Length > 64)
                {
                    return PasswordVerificationResult.Failed;
                }

                byte[] actual = DeriveKey(suppliedPassword, salt, iterations, expected.Length);
                if (!FixedTimeEquals(actual, expected))
                {
                    return PasswordVerificationResult.Failed;
                }

                return iterations < IterationCount
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Success;
            }
            catch (FormatException)
            {
                return PasswordVerificationResult.Failed;
            }
            catch (ArgumentException)
            {
                return PasswordVerificationResult.Failed;
            }
        }

        public static bool IsPasswordHash(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(FormatName + "$", StringComparison.Ordinal);
        }

        public static bool FixedTimeEqualsUtf8(string left, string right)
        {
            byte[] leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            byte[] rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);
            return FixedTimeEquals(leftBytes, rightBytes);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int outputLength)
        {
            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(outputLength);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            int difference = left.Length ^ right.Length;
            int maximumLength = Math.Max(left.Length, right.Length);
            for (int index = 0; index < maximumLength; index++)
            {
                byte leftByte = index < left.Length ? left[index] : (byte)0;
                byte rightByte = index < right.Length ? right[index] : (byte)0;
                difference |= leftByte ^ rightByte;
            }
            return difference == 0;
        }
    }
}
