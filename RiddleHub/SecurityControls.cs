using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web;
using System.Web.SessionState;

namespace RiddleHub.Security
{
    public static class CsrfProtection
    {
        private const string SessionKey = "RIDDLEHUB_CSRF_TOKEN";

        public static string GetToken(HttpSessionState session)
        {
            string token = Convert.ToString(session[SessionKey]);
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            byte[] tokenBytes = new byte[32];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(tokenBytes);
            }
            token = Convert.ToBase64String(tokenBytes);
            session[SessionKey] = token;
            return token;
        }

        public static string GetEncodedToken(HttpSessionState session)
        {
            return HttpUtility.HtmlAttributeEncode(GetToken(session));
        }

        public static bool IsValid(HttpRequest request, HttpSessionState session)
        {
            string expected = Convert.ToString(session[SessionKey]);
            string supplied = request.Form["csrf_token"];
            return !string.IsNullOrEmpty(expected) &&
                !string.IsNullOrEmpty(supplied) &&
                supplied.Length == expected.Length &&
                PasswordSecurity.FixedTimeEqualsUtf8(expected, supplied);
        }

        public static bool RejectInvalidPost(HttpRequest request, HttpResponse response, HttpSessionState session)
        {
            if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) || IsValid(request, session))
            {
                return false;
            }

            response.StatusCode = 403;
            response.TrySkipIisCustomErrors = true;
            response.Write("Invalid request token.");
            return true;
        }
    }

    public static class AuthRateLimiter
    {
        private sealed class AttemptWindow
        {
            public readonly Queue<DateTime> Failures = new Queue<DateTime>();
        }

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, AttemptWindow> Windows =
            new Dictionary<string, AttemptWindow>(StringComparer.Ordinal);
        private static readonly TimeSpan WindowLength = TimeSpan.FromMinutes(15);
        private const int PerAddressLimit = 30;
        private const int PerIdentityLimit = 8;
        private const int MaximumTrackedKeys = 10000;

        public static bool IsAllowed(HttpRequest request, string scope, string identity, out int retryAfterSeconds)
        {
            string addressKey = BuildKey(scope, "address", request.UserHostAddress ?? "unknown");
            string identityKey = BuildKey(scope, "identity", NormalizeIdentity(identity));
            DateTime now = DateTime.UtcNow;

            lock (Sync)
            {
                Prune(now);
                int addressRetry;
                int identityRetry;
                bool addressAllowed = IsWindowAllowed(addressKey, PerAddressLimit, now, out addressRetry);
                bool identityAllowed = IsWindowAllowed(identityKey, PerIdentityLimit, now, out identityRetry);
                retryAfterSeconds = Math.Max(addressRetry, identityRetry);
                return addressAllowed && identityAllowed;
            }
        }

        public static void RecordFailure(HttpRequest request, string scope, string identity)
        {
            DateTime now = DateTime.UtcNow;
            lock (Sync)
            {
                Record(BuildKey(scope, "address", request.UserHostAddress ?? "unknown"), now);
                Record(BuildKey(scope, "identity", NormalizeIdentity(identity)), now);
            }
        }

        public static void Reset(HttpRequest request, string scope, string identity)
        {
            lock (Sync)
            {
                Windows.Remove(BuildKey(scope, "address", request.UserHostAddress ?? "unknown"));
                Windows.Remove(BuildKey(scope, "identity", NormalizeIdentity(identity)));
            }
        }

        private static bool IsWindowAllowed(string key, int limit, DateTime now, out int retryAfterSeconds)
        {
            AttemptWindow window;
            if (!Windows.TryGetValue(key, out window) || window.Failures.Count < limit)
            {
                retryAfterSeconds = 0;
                return true;
            }

            DateTime availableAt = window.Failures.Peek().Add(WindowLength);
            retryAfterSeconds = Math.Max(1, (int)Math.Ceiling((availableAt - now).TotalSeconds));
            return false;
        }

        private static void Record(string key, DateTime now)
        {
            AttemptWindow window;
            if (!Windows.TryGetValue(key, out window))
            {
                if (Windows.Count >= MaximumTrackedKeys)
                {
                    return;
                }
                window = new AttemptWindow();
                Windows[key] = window;
            }
            window.Failures.Enqueue(now);
        }

        private static void Prune(DateTime now)
        {
            DateTime cutoff = now.Subtract(WindowLength);
            var emptyKeys = new List<string>();
            foreach (KeyValuePair<string, AttemptWindow> pair in Windows)
            {
                while (pair.Value.Failures.Count > 0 && pair.Value.Failures.Peek() <= cutoff)
                {
                    pair.Value.Failures.Dequeue();
                }
                if (pair.Value.Failures.Count == 0)
                {
                    emptyKeys.Add(pair.Key);
                }
            }
            foreach (string key in emptyKeys)
            {
                Windows.Remove(key);
            }
        }

        private static string NormalizeIdentity(string identity)
        {
            return (identity ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string BuildKey(string scope, string kind, string value)
        {
            return string.Join("|", scope ?? string.Empty, kind, value ?? string.Empty);
        }
    }
}
