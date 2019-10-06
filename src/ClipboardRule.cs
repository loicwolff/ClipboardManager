namespace ClipboardManager
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class ClipboardRule
    {
        public static IEnumerable<RuleResult> GetMatchingRules(string? text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                foreach (var rule in ClipboardRules)
                {
                    if (rule.IsMatch(text, out var values))
                    {
                        yield return new RuleResult(
                            String.Format(CultureInfo.CurrentCulture, rule.Label, values),
                            values,
                            rule.QuickActions);
                    }
                }
            }
        }

        internal static List<ClipboardRule> ClipboardRules = new List<ClipboardRule>()
        {
            new ClipboardRule(label: "Redmine issue",
                /*language=regex*/
                pattern: @"
                    ^
                    \s*
                    (?i:
                        refs\s?\#?
                        |
                        RM\s?
                        |
                        (?:\#)
                    )
                    ([0-9]+)
                    \s*
                    $
                    ",
                actions: new List<UrlQuickAction>()
                {
                    new UrlQuickAction(
                        "Redmine",
                        "Open Redmine issue",
                        "https://redmine.neovici.se/issues/{1}"),
                }),
            new ClipboardRule(label: "DevOps PR",
                /*language=regex*/
                pattern: @"
                    (?i)
                    ^
                    \s*
                    PR\s?
                    ([0-9]+)
                    \s*
                    $
                    ",
                actions: new List<UrlQuickAction>()
                {
                    new UrlQuickAction(
                        name: "Pull Request",
                        label: "View Pull Request",
                        "https://neovici.visualstudio.com/Cosmoz3/Cosmoz3%20Team/_git/cz3backend/pullrequest/{1}")
                })
            //new ClipboardRule(
            //    label: "Guid",
            //    /*language=regex*/
            //    pattern: @"
            //        (?im)                       
            //        ^
            //        \s*
            //        [{(]?
            //        [0-9A-F]{8}
            //        [-]?
            //        (?:[0-9A-F]{4}[-]?){3}
            //        [0-9A-F]{12}
            //        [)}]? 
            //        \s*
            //        $",
            //    actions: new List<UrlQuickAction>()
            //    {
            //        new ExtractQuickAction(name: "Guid", label: "Copy guid", )
            //        {
            //            CanCopy = true,
            //            Name = "Guid",
            //            OpenLabel = "Copy guid",
            //        }
            //    })
            
        };

        public ClipboardRule(string label, string pattern, IReadOnlyCollection<QuickAction> actions)
        {
            this.Label = label;
            this.RuleRegex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            this.QuickActions = actions;
            this.Label = "{0}";
        }

        public string Label { get; set; }

        public Regex RuleRegex { get; private set; }

        public IReadOnlyCollection<QuickAction> QuickActions { get; set; }

        public virtual bool IsMatch(string input, out string[] output)
        {
            var match = this.RuleRegex.Match(input);

            if (match.Success)
            {
                output = match.Groups.OfType<Group>().Select(g => g.Value).ToArray();

                return true;
            }

            output = Array.Empty<string>();

            return false;
        }
    }
}
