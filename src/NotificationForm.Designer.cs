namespace ClipboardManager
{
    partial class NotificationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.CopyLinkButton = new ClipboardManager.NonSelectableButton();
            this.OpenLinkButton = new ClipboardManager.NonSelectableButton();
            this.SuspendLayout();
            // 
            // CopyLinkButton
            // 
            this.CopyLinkButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.CopyLinkButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.CopyLinkButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.CopyLinkButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.CopyLinkButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.CopyLinkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CopyLinkButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyLinkButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CopyLinkButton.Location = new System.Drawing.Point(299, 0);
            this.CopyLinkButton.Margin = new System.Windows.Forms.Padding(0);
            this.CopyLinkButton.Name = "CopyLinkButton";
            this.CopyLinkButton.Size = new System.Drawing.Size(125, 40);
            this.CopyLinkButton.TabIndex = 1;
            this.CopyLinkButton.Text = "copy link";
            this.CopyLinkButton.UseVisualStyleBackColor = false;
            this.CopyLinkButton.MouseEnter += new System.EventHandler(this.OnMouseEnter);
            this.CopyLinkButton.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.CopyLinkButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CopyLinkLabel_MouseUp);
            // 
            // OpenLinkButton
            // 
            this.OpenLinkButton.BackColor = System.Drawing.Color.LightSteelBlue;
            this.OpenLinkButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.OpenLinkButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.OpenLinkButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.OpenLinkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenLinkButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenLinkButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.OpenLinkButton.Location = new System.Drawing.Point(0, 0);
            this.OpenLinkButton.Margin = new System.Windows.Forms.Padding(0);
            this.OpenLinkButton.Name = "OpenLinkButton";
            this.OpenLinkButton.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.OpenLinkButton.Size = new System.Drawing.Size(300, 40);
            this.OpenLinkButton.TabIndex = 0;
            this.OpenLinkButton.Text = "open link";
            this.OpenLinkButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OpenLinkButton.UseVisualStyleBackColor = false;
            this.OpenLinkButton.MouseEnter += new System.EventHandler(this.OnMouseEnter);
            this.OpenLinkButton.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.OpenLinkButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OpenLinkLabel_MouseUp);
            // 
            // NotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(424, 40);
            this.ControlBox = false;
            this.Controls.Add(this.CopyLinkButton);
            this.Controls.Add(this.OpenLinkButton);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NotificationForm";
            this.MouseEnter += new System.EventHandler(this.OnMouseEnter);
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.ResumeLayout(false);
        }

        #endregion

        private NonSelectableButton OpenLinkButton;
        private NonSelectableButton CopyLinkButton;

    }
}