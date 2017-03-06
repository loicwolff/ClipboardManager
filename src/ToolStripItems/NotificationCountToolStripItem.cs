using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardManager
{
    public class NotificationCountToolStripItem : ToolStripControlHost
    {
        public event NotificationCountEventHandler ValueChanged;
        
        public NotificationCountToolStripItem(int currentSelection)
            : base(new FlowLayoutPanel())
        {
            FlowLayoutPanel mainPanel = Control as FlowLayoutPanel;
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
                Height = 22 ,
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

                radioButton.CheckedChanged += radioButton_CheckedChanged;

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

        void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            int? notificationCount = (sender as RadioButton).Tag as int?;

            if (ValueChanged != null && notificationCount.HasValue)
            {
                ValueChanged(this, new NotificationCountEventArgs(notificationCount.Value));
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
