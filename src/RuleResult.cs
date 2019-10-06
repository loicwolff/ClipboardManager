using System.Collections.Generic;

namespace ClipboardManager
{
    public class RuleResult
    {
        public RuleResult(string label, string[] values, IEnumerable<QuickAction> actions)
        {
            this.Label = label;
            this.Values = values;
            this.QuickActions = actions;
        }

        public string Label { get; set; }

        public IReadOnlyCollection<string> Values { get; set; }

        public IEnumerable<QuickAction> QuickActions { get; set; }
    }
}
