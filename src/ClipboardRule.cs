using System.Globalization;
using System.Text.RegularExpressions;

namespace ClipboardManager;

public class ClipboardRule
{
    public static IEnumerable<RuleResult> GetMatchingRules(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            foreach (var rule in ClipboardRules)
            {
                if (rule.IsMatch(text, out var values))
                {
                    yield return new RuleResult(
                        string.Format(CultureInfo.CurrentCulture, rule.Label, values),
                        values,
                        rule.QuickActions);
                }
            }
        }
    }

    private static readonly List<ClipboardRule> ClipboardRules =
    [
        new ClipboardRule(label: "Redmine issue #{1}",
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
            actions:
            [
                new UrlQuickAction(
                    "Redmine",
                    "Open Redmine issue",
                    "https://redmine.neovici.se/issues/{1}"),
            ]),
        new ClipboardRule(label: "DevOps PR {1}",
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
            actions:
            [
                new UrlQuickAction(
                    name: "Pull Request",
                    label: "View Pull Request",
                    "https://neovici.visualstudio.com/Cosmoz3/Cosmoz3%20Team/_git/cz3backend/pullrequest/{1}")
            ]),
    ];

    public ClipboardRule(string label, string pattern, IReadOnlyCollection<QuickAction> actions)
    {
        this.Label = label;
        this.RuleRegex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
        this.QuickActions = actions;
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

        output = [];

        return false;
    }
}
