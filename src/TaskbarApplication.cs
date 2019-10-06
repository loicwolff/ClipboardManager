namespace ClipboardManager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using Clipboard = SafeClipboard;

    public class TaskbarApplication : ApplicationContext
    {
        /// <summary>
        /// Create if necessary and return the local application data folder
        /// </summary>
        public static string LocalDataFolder
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Clipboard Manager");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return folder;
            }
        }

        #region Variables

        public event EventHandler? CloseNotifications;

        private const int EllipsisLength = 30;

        private readonly Icon mainIcon = Icon.FromHandle(Resources.clip_white.GetHicon());
        private readonly Icon disabledIcon = Icon.FromHandle(Resources.clip_disabled.GetHicon());

        private readonly Lazy<AppConfiguration> configuration = new Lazy<AppConfiguration>(() => AppConfiguration.Load());

        private IReadOnlyCollection<RuleResult> ClipboardRules { get; set; }

        //private KeyboardHook KeyboardHook { get; set; }

        private NotifyIcon TrayIcon { get; set; }

        #endregion

        #region Properties

        private static readonly Object clipsLock = new Object();

        private readonly ClipboardHistoryCollection clipboardHistory = new ClipboardHistoryCollection();
        private ClipboardHistoryCollection ClipboardHistory
        {
            get
            {
                lock (clipsLock)
                {
                    return this.clipboardHistory;
                }
            }
        }

        private AppConfiguration Configuration => this.configuration.Value;

        public Icon DisabledIcon => this.disabledIcon;

        #endregion

        #region Constructor

        public TaskbarApplication()
        {
            this.TrayIcon = new NotifyIcon()
            {
                Icon = mainIcon,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
                Text = "Clipboard Manager",
            };

            this.TrayIcon.ContextMenuStrip.RenderMode = ToolStripRenderMode.System;

            // add left click to the systray icon
            // source: https://stackoverflow.com/a/3581311/12008
            this.TrayIcon.MouseClick += (sender, mouseEvent) =>
            {
                if (mouseEvent.Button == MouseButtons.Left)
                {
                    var methodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo?.Invoke(this.TrayIcon, null);
                }
            };

            //KeyboardHook = new KeyboardHook();

            //try
            //{
            //    KeyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.C);
            //}
            //catch (Exception)
            //{
            //    //Trace.TraceWarning("Cannot register Win + Shift + C");
            //}

            //try
            //{
            //    KeyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.W);
            //}
            //catch (Exception)
            //{
            //    //Trace.TraceWarning("Cannot register Win + Shift + W");
            //}

            //try
            //{
            //    KeyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.X);
            //}
            //catch (Exception)
            //{
            //    //Trace.TraceWarning("Cannot register Win + Shift + X");
            //}

            //try
            //{
            //    KeyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.D);
            //}
            //catch (Exception)
            //{
            //    //Trace.TraceWarning("Cannot register Win + Shift + D");
            //}

            //KeyboardHook.KeyPressed += KeyboardHook_KeyPressed;

            this.ClipboardHistory.OnHistoryChanged += this.OnClipboardHistoryChanged;

            this.ClipboardRules = ClipboardRule.GetMatchingRules(this.ClipboardHistory.CurrentClip.Text).ToList();

            this.DrawMenuItems();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.mainIcon.Dispose();
            this.disabledIcon.Dispose();
        }

        #region Systray menu

        private void AddClipboardHistory()
        {
            foreach (ClipItem clip in this.ClipboardHistory)
            {
                var (label, length) = Ellipsis(clip.Text);
                bool isInClipboard = clip == this.ClipboardHistory.CurrentClip;

                var trayIconMenuItem = new ToolStripMenuItem(label)
                {
                    ShortcutKeyDisplayString = length,
                    Enabled = this.ClipboardHistory.SavingEnabled,
                    Font = new Font(SystemFonts.MenuFont, isInClipboard ? FontStyle.Bold : FontStyle.Regular),
                };
                trayIconMenuItem.Click += (object? sender, EventArgs e) =>
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        this.ClipboardHistory.RemoveAll(clip);
                    }
                    else
                    {
                        Clipboard.SetText(clip.Text);
                    }
                };

                this.TrayIcon.ContextMenuStrip.Items.Add(trayIconMenuItem);
            }

            if (this.ClipboardHistory.CurrentClip.IsEmpty)
            {
                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Clipboard is empty")
                {
                    Enabled = false,
                });
            }
        }

        private void AddClipboardRules()
        {
            if (this.ClipboardRules != null && this.ClipboardRules.Any())
            {
                var actions = this.ClipboardRules.SelectMany(r => r.QuickActions);

                int width = actions.Max(a => TextRenderer.MeasureText(a.OpenLabel, SystemFonts.DefaultFont).Width);

                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                this.TrayIcon.ContextMenuStrip.Items.Add(new LabelToolStripItem("Quick Actions"));

                foreach (var ruleResult in this.ClipboardRules)
                {
                    var quickActionMenuItem = new QuickActionToolStripItem(ruleResult, width);
                    quickActionMenuItem.ItemClicked += (object? sender, EventArgs e) =>
                    {
                        this.TrayIcon.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                    };
                    quickActionMenuItem.CopyItemClicked += (object? sender, EventArgs e) =>
                    {
                        this.TrayIcon.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                    };

                    this.TrayIcon.ContextMenuStrip.Items.Add(quickActionMenuItem);
                }
            }
        }

        private void DrawMenuItems()
        {
            this.TrayIcon.ContextMenuStrip.Items.Clear();

            this.TrayIcon.ContextMenuStrip.Items.Add(this.GetOptionsMenuItem());

            if (this.ClipboardHistory.Any())
            {
                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            this.AddClipboardHistory();

            this.AddClipboardRules();

            if (!this.ClipboardHistory.SavingEnabled)
            {
                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Saving Disabled") { Enabled = false });

                this.TrayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Enable Clipboard Monitoring", null, this.ToggleSaving));
            }
        }

        private ToolStripMenuItem GetOptionsMenuItem()
        {
            var menuItem = new ToolStripMenuItem("Options");

            menuItem.DropDownItems.Add(new ToolStripMenuItem("Show Quick Actions Notifications", null, this.ToggleHUD) { Checked = this.Configuration.ShowHUD, ToolTipText = "Show notification if a special pattern is recognized" });

            if (this.Configuration.ShowHUD)
            {
                menuItem.DropDownItems.Add(new ToolStripMenuItem("Limit Notification Count", null, this.ToggleLimitNotification) { Checked = this.Configuration.LimitNotificationCount });

                if (this.Configuration.LimitNotificationCount)
                {
                    var notificationCountItem = new NotificationCountToolStripItem(this.Configuration.MaxNotificationCount) { Enabled = this.Configuration.ShowHUD };
                    notificationCountItem.ValueChanged += (object sender, NotificationCountEventArgs e) =>
                    {
                        this.Configuration.MaxNotificationCount = e.NotificationCount;
                        this.Configuration.Save();

                        this.TrayIcon.ContextMenuStrip.Close();
                    };
                    menuItem.DropDownItems.Add(notificationCountItem);
                }
            }

            menuItem.DropDownItems.Add(new ToolStripSeparator());

            menuItem.DropDownItems.Add(new ToolStripMenuItem("Empty Clipboard History", null, (object? sender, EventArgs e) => this.RemoveAllClips()));

            if (this.ClipboardHistory.SavingEnabled)
            {
                menuItem.DropDownItems.Add(new ToolStripMenuItem("Disable Clipboard Monitoring", null, this.ToggleSaving));
            }

            menuItem.DropDownItems.Add(new ToolStripSeparator());

            menuItem.DropDownItems.Add(new ToolStripMenuItem("Exit", null, this.Exit));

            return menuItem;
        }

        private void Exit(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Quit Clipboard Manager?\nYour clipboard will no longer be saved.", "Clipboard Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                // Hide tray icon, otherwise it will remain shown until user mouses over it
                this.TrayIcon.Visible = false;

                Application.Exit();
            }
        }

        private void ToggleSaving(object? sender, EventArgs e)
        {
            this.ClipboardHistory.ToggleSavingEnabled();

            this.TrayIcon.Icon = this.ClipboardHistory.SavingEnabled ? this.mainIcon : this.DisabledIcon;

            this.DrawMenuItems();
        }

        private void ToggleHUD(object? sender, EventArgs e)
        {
            this.Configuration.ShowHUD = !this.Configuration.ShowHUD;
            this.Configuration.Save();

            this.DrawMenuItems();
        }

        private void ToggleLimitNotification(object? sender, EventArgs e)
        {
            this.Configuration.LimitNotificationCount = !this.Configuration.LimitNotificationCount;
            this.Configuration.Save();

            this.DrawMenuItems();
        }

        private void RemoveAllClips()
        {
            if (MessageBox.Show("This will clear your clipboard and the clipboard history. Continue?", "Clear History?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                this.ClipboardHistory.Clear();
            }
        }

        #endregion

        #region Notifications

        public void ShowNotifications()
        {
            if (this.Configuration.ShowHUD && this.ClipboardRules != null && this.ClipboardRules.Any())
            {
                this.SendCloseNotificationsEvent();

                int index = 0;
                int startPosY = Screen.PrimaryScreen.WorkingArea.Height;

                foreach (var rule in this.ClipboardRules)
                {
                    var actions = from action in rule.QuickActions
                                  where action.IsEnabled
                                  select action;

                    if (this.Configuration.LimitNotificationCount)
                    {
                        actions = actions.Take(this.Configuration.MaxNotificationCount - index).AsQueryable();
                    }

                    foreach (var quickAction in actions)
                    {
                        var notification = new NotificationForm(this, quickAction);
                        notification.StartPosY = startPosY -= notification.Height + 5;

                        notification.OpenClick += (object sender, MouseEventArgs e) =>
                        {
                            quickAction.Run(rule.Values);

                            if (actions.Count() == 1)
                            {
                                this.SendCloseNotificationsEvent();
                            }
                        };

                        notification.CopyLinkClick += (object sender, MouseEventArgs e) =>
                        {
                            quickAction.Copy(rule.Values);

                            if (actions.Count() == 1)
                            {
                                this.SendCloseNotificationsEvent();
                            }
                        };

                        notification.RightClick += (object sender, MouseEventArgs e) =>
                        {
                            this.SendCloseNotificationsEvent();
                        };

                        notification.ShowInactiveTopmost();
                        notification.FadeIn();
                    }

                    var titleNotification = new NotificationForm(this, rule.Label);
                    titleNotification.StartPosY = (startPosY -= titleNotification.Height + 5);

                    titleNotification.RightClick += (object sender, MouseEventArgs e) =>
                    {
                        this.SendCloseNotificationsEvent();
                    };
                    titleNotification.ShowInactiveTopmost();
                    titleNotification.FadeIn();
                }
            }
        }

        private void SendCloseNotificationsEvent()
        {
            CloseNotifications?.Invoke(this, EventArgs.Empty);
        }

        private QuickAction? GetFirstQuickAction(bool copyOnly, out string[] urlValues)
        {
            var rule = this.ClipboardRules.FirstOrDefault();
            if (rule != null)
            {
                urlValues = rule.Values.ToArray();

                var actions = rule.QuickActions.Where(a => a.IsEnabled);

                if (copyOnly)
                {
                    actions = actions.Where(a => a.CanCopy);
                }

                var firstAction = actions.FirstOrDefault();

                return firstAction;
            }

            urlValues = Array.Empty<string>();
            return null;
        }

        private void CopyFirstLink()
        {
            var action = this.GetFirstQuickAction(true, out var urlValues);

            if (action != null)
            {
                this.SendCloseNotificationsEvent();

                action.Copy(urlValues);

                var titleNotification = new NotificationForm(this, String.Format(CultureInfo.CurrentCulture, "{0} link copied", action.Name));
                titleNotification.StartPosY = Screen.PrimaryScreen.WorkingArea.Height - (titleNotification.Height + 5);

                titleNotification.RightClick += (object sender, MouseEventArgs e) =>
                {
                    this.SendCloseNotificationsEvent();
                };
                titleNotification.ShowInactiveTopmost();
                titleNotification.FadeIn();
            }

        }

        private void OpenFirstAction()
        {
            var action = this.GetFirstQuickAction(true, out var urlValues);

            action?.Run(urlValues);
        }

        #endregion

        void OnClipboardHistoryChanged(object? sender, NewClipItemEventEventArgs e)
        {
            this.ClipboardRules = ClipboardRule.GetMatchingRules(e.ClipItem.Text).ToList();

            if (e.ClipItem != ClipItem.Empty && e.ClipItem != e.PreviousClipItem)
            {
                this.ShowNotifications();
            }

            this.DrawMenuItems();
        }

        void KeyboardHook_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.C)
            {
                Clipboard.SetText(this.ClipboardHistory.CurrentClip.Text);
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.W)
            {
                this.ShowNotifications();
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.X)
            {
                this.CopyFirstLink();
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.D)
            {
                this.OpenFirstAction();
            }
        }

        private static (string, string) Ellipsis(string? label)
        {
            if (String.IsNullOrWhiteSpace(label))
            {
                return (string.Empty, string.Empty);
            }

            label = Regex.Replace(label, @"([\r\n]+|\t+|\s+)", " ").Replace("&", "&&", StringComparison.InvariantCulture).Trim();

            if (label.Length > EllipsisLength + 1)
            {
                return (String.Concat(label.Substring(0, EllipsisLength).TrimEnd(), "…").PadRight(EllipsisLength + 5),
                        String.Format(CultureInfo.InvariantCulture, "[{0:N0}c]", label.Length));
            }
            else
            {
                return (label, String.Empty);
            }
        }
    }
}
