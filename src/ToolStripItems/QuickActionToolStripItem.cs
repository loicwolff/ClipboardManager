namespace ClipboardManager
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    
    public class QuickActionToolStripItem : ToolStripControlHost
    {
        public event EventHandler ItemClicked;
        public event EventHandler CopyItemClicked;

        public QuickActionToolStripItem(ClipboardRule clipboardRule, int buttonWidth)
            : base(new FlowLayoutPanel())
        {
            Margin = new Padding(0);
            Padding = new Padding(0);

            var mainPanel = this.Control as FlowLayoutPanel;
            mainPanel.BackColor = Color.Transparent;
            mainPanel.Padding = new Padding(0);
            mainPanel.Margin = new Padding(0);
            mainPanel.FlowDirection = FlowDirection.TopDown;

            foreach (var action in clipboardRule.QuickActions.Reverse())
            {
                var buttonPanel = new FlowLayoutPanel
                {
                    BackColor = Color.Transparent,
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = false,
                    Height = 25,
                    Width = 275,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };

                var openButton = new Button()
                {
                    Text = action.OpenLabel,
                    AutoSize = false,
                    Enabled = action.IsEnabled,
                    Padding = new Padding(0),
                    Margin = new Padding(0),
                    Width = buttonWidth + 40,
                };

                openButton.MinimumSize = openButton.Size;

                openButton.Click += (object sender, EventArgs e) =>
                {
                    ItemClicked?.Invoke(this, e);
                    
                    action.Start(clipboardRule.Values);
                };

                buttonPanel.Controls.Add(openButton);

                if (action.CanCopy)
                {
                    // Copy button
                    var copyButton = new Button
                    {
                        Text = "Copy Link",
                        Padding = new Padding(0),
                        Margin = new Padding(0),
                    };

                    copyButton.Click += (object sender, EventArgs e) =>
                    {
                        CopyItemClicked?.Invoke(this, e);

                        action.Copy(clipboardRule.Values);
                    };

                    buttonPanel.Controls.Add(copyButton);
                }

                mainPanel.Controls.Add(buttonPanel);
            }
        }
    }
}
