using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>One detected secret: what kind it looks like, and a redacted preview.</summary>
    public class SecretMatch
    {
        public string Kind { get; set; }
        /// <summary>Preview with the middle masked — the full secret value is never retained.</summary>
        public string Redacted { get; set; }
    }

    /// <summary>
    /// Scans free text (botcomponent content/data) for hardcoded credentials. Heuristic and
    /// read-only — patterns are deliberately specific to keep false positives low, and known
    /// placeholder / environment-variable references are filtered out. Full secret values are
    /// NEVER returned or logged; only a redacted preview (first/last 3 characters).
    ///
    /// Addresses Microsoft "Top 10 Copilot Studio agent security risks" #7 (hardcoded credentials
    /// in agent logic).
    /// </summary>
    public static class SecretScanner
    {
        private sealed class Pattern
        {
            public string Kind;
            public Regex Regex;
        }

        private static readonly Pattern[] Patterns =
        {
            new Pattern { Kind = "AWS access key",
                Regex = new Regex(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled) },
            new Pattern { Kind = "JWT",
                Regex = new Regex(@"eyJ[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}", RegexOptions.Compiled) },
            new Pattern { Kind = "Bearer token",
                Regex = new Regex(@"(?i)bearer\s+[A-Za-z0-9._\-]{20,}", RegexOptions.Compiled) },
            new Pattern { Kind = "Secret / key assignment",
                Regex = new Regex(@"(?i)(password|pwd|client[_-]?secret|api[_-]?key|access[_-]?token|secret)\s*[:=]\s*[""']?[^\s""',}{)\]]{6,}", RegexOptions.Compiled) },
            new Pattern { Kind = "Connection string with password",
                Regex = new Regex(@"(?i)(server|data source)=[^;]+;[^\r\n]*(password|pwd)\s*=", RegexOptions.Compiled) },
        };

        // Tokens that make a "secret assignment" actually a placeholder, expression, or
        // environment-variable / connection reference — i.e. NOT a real hardcoded leak.
        private static readonly string[] Placeholders =
        {
            "environmentvariable", "@parameters", "@{", "${", "{{", "<", "your-", "yourkey",
            "xxxx", "******", "•", "...", "null", "true", "false", "example", "placeholder",
            "secret_name", "secretname", "keyvault", "@microsoft", "@triggerbody", "@body"
        };

        public static List<SecretMatch> Scan(string text)
        {
            var found = new List<SecretMatch>();
            if (string.IsNullOrEmpty(text)) return found;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in Patterns)
            {
                foreach (Match m in p.Regex.Matches(text))
                {
                    var val = m.Value;
                    var lower = val.ToLowerInvariant();
                    if (Placeholders.Any(ph => lower.Contains(ph))) continue;

                    var redacted = Redact(val);
                    var key = p.Kind + "|" + redacted;
                    if (seen.Add(key))
                        found.Add(new SecretMatch { Kind = p.Kind, Redacted = redacted });
                }
            }
            return found;
        }

        private static string Redact(string value)
        {
            value = value.Trim();
            if (value.Length <= 8) return new string('•', value.Length);
            var maskLen = Math.Min(value.Length - 6, 12);
            return value.Substring(0, 3) + new string('•', maskLen) + value.Substring(value.Length - 3);
        }
    }
}
