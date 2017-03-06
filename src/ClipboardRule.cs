using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClipboardManager
{
    [DebuggerDisplay("Type: {Type}")]
    public class ClipboardRule
    {
        public static IEnumerable<ClipboardRule> GetMatchingRules(string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                foreach (ClipboardRule rule in ClipboardRules)
                {
                    string[] values;
                    if (rule.IsMatch(text, out values))
                    {
                        yield return new ClipboardRule
                        {
                            Label = String.Format(rule.Label, values),
                            Values = values,
                            QuickActions = rule.QuickActions,
                        };
                    }
                }
            }
        }

        internal static List<ClipboardRule> ClipboardRules = new List<ClipboardRule>()
        {
            
        };

        public ClipboardRule()
        {
            QuickActions = new List<QuickAction>();
            Label = "{0}";
        }

        public string Type { get; set; }

        public string Label { get; set; }

        public string[] Values { get; set; }

        public Regex RuleRegex { get; private set; }

        public string RegexPattern
        {
            set
            {
                RuleRegex = new Regex(value, RegexOptions.IgnorePatternWhitespace);
            }
        }

        public List<QuickAction> QuickActions { get; set; }

        public virtual bool IsMatch(string input, out string[] output)
        {
            Match match = RuleRegex.Match(input);

            output = null;

            if (match.Success)
            {
                output = match.Groups.OfType<Group>().Select(g => g.Value).ToArray();

                return true;
            }

            return false;
        }
    }
}
