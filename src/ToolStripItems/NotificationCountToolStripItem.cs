namespace ClipboardManager
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class NotificationCountToolStripItem : ToolStripControlHost
    {
        public event NotificationCountEventHandler ValueChanged;

        public NotificationCountToolStripItem(int currentSelection)
            : base(new FlowLayoutPanel())
        {
            var mainPanel = Control as FlowLayoutPanel;
            mainPanel.BackColor = Color.Transparent;
            mainPanel.FlowDirection = FlowDirection.TopDown;
            mainPanel.AutoSize = false;
            mainPanel.Size = new System.Drawing.Size(100, 10);
            mainPanel.MinimumSize = mainPanel.Size;

            var radioButtonPanel = new FlowLayoutPanel
            {
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                Height = 22,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            for (int i = 1; i <= 5; i++)
            {
                var radioButton = new RadioButton()
                {
                    BackColor = Color.Transparent,
                    Text = i.ToString(),
                    Width = 30,
                    Height = 18,
                    Checked = currentSelection == i,
                    Tag = i,
                    Enabled = this.Enabled,
                };

                radioButton.CheckedChanged += OnRadioButtonCheckedChanged;

                radioButtonPanel.Controls.Add(radioButton);
            }

            var maxNotificationLabel = new Label
            {
                Text = "Max Notifications Count",
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Height = 17,
                Width = 200,
            };

            mainPanel.Controls.Add(maxNotificationLabel);
            mainPanel.Controls.Add(radioButtonPanel);
        }

        void OnRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is int notificationCount)
            {
                ValueChanged?.Invoke(this, new NotificationCountEventArgs(notificationCount));
            }
        }
    }

    public delegate void NotificationCountEventHandler(object sender, NotificationCountEventArgs e);

    public class NotificationCountEventArgs : EventArgs
    {
        public int NotificationCount { get; set; }

        public NotificationCountEventArgs(int count)
        {
            NotificationCount = count;
        }
    }
}
