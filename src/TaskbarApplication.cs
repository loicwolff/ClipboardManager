using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClipboardManager.Properties;

namespace ClipboardManager
{
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

                if(!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return folder;
            }
        }
        
        #region Variables

        public event EventHandler CloseNotifications;

        private const int EllipsisLength = 30;
        private const int MaxClipCount = 30;
        
        private readonly Icon mainIcon = Icon.FromHandle(Resources.clip.GetHicon());
        private readonly Icon disabledIcon = Icon.FromHandle(Resources.clip_disabled.GetHicon());

        private Lazy<Configuration> configuration = new Lazy<Configuration>(() => Configuration.Load());

        private List<ClipboardRule> ClipboardRules = new List<ClipboardRule>();
        
        private KeyboardHook keyboardHook;

        private NotifyIcon trayIcon;

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
                    return clipboardHistory;
                }
            }
        }
        
        private Configuration Configuration => configuration.Value;

        public Icon DisabledIcon => disabledIcon;

        #endregion

        #region Constructor

        public TaskbarApplication()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = mainIcon,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
                Text = "Clipboard Manager",
            };

            // add left click to the systray icon
            // source: https://stackoverflow.com/a/3581311/12008
            trayIcon.MouseClick += (sender, mouseEvent) =>
            {
                if (mouseEvent.Button == MouseButtons.Left)
                {
                    MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi?.Invoke(trayIcon, null);
                }
            };
            
            keyboardHook = new KeyboardHook();

            try
            {
                keyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.C);
            }
            catch (Exception)
            {
                //Trace.TraceWarning("Cannot register Win + Shift + C");
            }

            try
            {
                keyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.W);
            }
            catch (Exception)
            {
                //Trace.TraceWarning("Cannot register Win + Shift + W");
            }

            try
            {
                keyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.X);
            }
            catch (Exception)
            {
                //Trace.TraceWarning("Cannot register Win + Shift + X");
            }

            try
            {
                keyboardHook.RegisterHotkey(ModifierKeys.Win | ModifierKeys.Shift, Keys.D);
            }
            catch (Exception)
            {
                //Trace.TraceWarning("Cannot register Win + Shift + D");
            }

            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;

            ClipboardHistory.OnHistoryChanged += OnClipboardHistoryChanged;

            ClipboardRules = ClipboardRule.GetMatchingRules(ClipboardHistory.CurrentClip.Text).ToList();

            DrawMenuItems();
        }
        
        #endregion

        #region Systray menu

        private void AddClipboardHistory()
        {
            foreach (ClipItem clip in ClipboardHistory)
            {
                var (label, length) = Ellipsis(clip.Text);

                bool isInClipboard = clip == ClipboardHistory.CurrentClip;

                var trayIconMenuItem = new ToolStripMenuItem(label)
                {
                    ShortcutKeyDisplayString = length,
                    Enabled = ClipboardHistory.SavingEnabled,
                    Font = new Font(SystemFonts.MenuFont, isInClipboard ? FontStyle.Bold : FontStyle.Regular),
                };
                trayIconMenuItem.Click += (object sender, EventArgs e) =>
                {
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        ClipboardHistory.RemoveAll(clip);
                    }
                    else
                    {
                        Clipboard.SetText(clip.Text);
                    }
                };

                trayIcon.ContextMenuStrip.Items.Add(trayIconMenuItem);
            }

            if (this.ClipboardHistory.CurrentClip.IsEmpty)
            {
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Clipboard is empty")
                {
                    Enabled = false,
                });
            }
        }

        private void AddClipboardRules()
        {
            if (ClipboardRules != null && ClipboardRules.Any())
            {
                var actions = ClipboardRules.SelectMany(r => r.QuickActions);

                int width = actions.Max(a => TextRenderer.MeasureText(a.OpenLabel, SystemFonts.DefaultFont).Width);

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                trayIcon.ContextMenuStrip.Items.Add(new LabelToolStripItem("Quick Actions"));

                foreach (ClipboardRule rule in ClipboardRules)
                {
                    var quickActionMenuItem = new QuickActionToolStripItem(rule, width);
                    quickActionMenuItem.ItemClicked += (object sender, EventArgs e) =>
                    {
                        trayIcon.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                    };
                    quickActionMenuItem.CopyItemClicked += (object sender, EventArgs e) =>
                    {
                        trayIcon.ContextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                    };

                    trayIcon.ContextMenuStrip.Items.Add(quickActionMenuItem);
                }
            }
        }

        private void DrawMenuItems()
        {
            trayIcon.ContextMenuStrip.Items.Clear();

            this.AddClipboardHistory();

            this.AddClipboardRules();

            if (ClipboardHistory.Any())
            {
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (!ClipboardHistory.SavingEnabled)
            {
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Saving Disabled") { Enabled = false });

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Enable Clipboard Monitoring", null, ToggleSaving));

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            trayIcon.ContextMenuStrip.Items.Add(GetSettingsMenuItem());

            trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, Exit));
        }

        private ToolStripMenuItem GetSettingsMenuItem()
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Settings");

            menuItem.DropDownItems.Add(new ToolStripMenuItem("Show Quick Actions Notifications", null, ToggleHUD) { Checked = Configuration.ShowHUD });

            if (Configuration.ShowHUD)
            {
                menuItem.DropDownItems.Add(new ToolStripMenuItem("Limit Notification Count", null, ToggleLimitNotification) { Checked = Configuration.LimitNotificationCount });

                if (Configuration.LimitNotificationCount)
                {
                    var notificationCountItem = new NotificationCountToolStripItem(Configuration.MaxNotificationCount) { Enabled = Configuration.ShowHUD };
                    notificationCountItem.ValueChanged += (object sender, NotificationCountEventArgs e) =>
                    {
                        Configuration.MaxNotificationCount = e.NotificationCount;
                        Configuration.Save();

                        this.trayIcon.ContextMenuStrip.Close();
                    };
                    menuItem.DropDownItems.Add(notificationCountItem);
                }

                menuItem.DropDownItems.Add(new ToolStripMenuItem("Always Show Notifications", null, ToggleAlwaysShowNotifications)
                {
                    Enabled = Configuration.ShowHUD,
                    Checked = Configuration.AlwaysShowNotifications,
                });
            }

            menuItem.DropDownItems.Add(new ToolStripSeparator());

            menuItem.DropDownItems.Add(new ToolStripMenuItem("Empty Clipboard History", null, (object sender, EventArgs e) => RemoveAllClips()));

            if (ClipboardHistory.SavingEnabled)
            {
                menuItem.DropDownItems.Add(new ToolStripMenuItem("Disable Clipboard Monitoring", null, ToggleSaving));
            }

            return menuItem;
        }

        private void Exit(object sender, EventArgs e)
        {
            if (MessageBox.Show("Quit Clipboard Manager?\nYour clipboard will no longer be saved.", "Clipboard Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                // Hide tray icon, otherwise it will remain shown until user mouses over it
                trayIcon.Visible = false;

                Application.Exit();
            }
        }

        private void ToggleSaving(object sender, EventArgs e)
        {
            ClipboardHistory.ToggleSavingEnabled();

            this.trayIcon.Icon = ClipboardHistory.SavingEnabled ? this.mainIcon : this.DisabledIcon;

            DrawMenuItems();
        }

        private void ToggleHUD(object sender, EventArgs e)
        {
            Configuration.ShowHUD = !Configuration.ShowHUD;
            Configuration.Save();

            DrawMenuItems();
        }

        private void ToggleLimitNotification(object sender, EventArgs e)
        {
            Configuration.LimitNotificationCount = !Configuration.LimitNotificationCount;
            Configuration.Save();

            DrawMenuItems();
        }

        private void ToggleAlwaysShowNotifications(object sender, EventArgs e)
        {
            Configuration.AlwaysShowNotifications = !Configuration.AlwaysShowNotifications;
            Configuration.Save();

            DrawMenuItems();
        }

        private void RemoveAllClips()
        {
            if (MessageBox.Show("This will clear your clipboard and the clipboard history. Continue?", "Clear History?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                ClipboardHistory.Clear();
            }
        }

        #endregion

        #region Notifications

        public void ShowNotifications()
        {
            if (Configuration.ShowHUD && ClipboardRules != null && ClipboardRules.Any())
            {
                SendCloseNotificationsEvent();

                int index = 0;
                int startPosY = Screen.PrimaryScreen.WorkingArea.Height;

                foreach (var rule in ClipboardRules)
                {
                    var actions = from action in rule.QuickActions
                                  where action.IsEnabled
                                  select action;

                    if (Configuration.LimitNotificationCount)
                    {
                        actions = actions.Take(Configuration.MaxNotificationCount - index).AsQueryable();
                    }

                    foreach (var quickAction in actions)
                    {
                        var notification = new NotificationForm(this, quickAction);
                        notification.StartPosY = startPosY -= notification.Height + 5;

                        notification.OpenClick += (object sender, MouseEventArgs e) =>
                        {
                            quickAction.Start(rule.Values);

                            if (actions.Count() == 1)
                            {
                                SendCloseNotificationsEvent();
                            }
                        };

                        notification.CopyLinkClick += (object sender, MouseEventArgs e) =>
                        {
                            quickAction.Copy(rule.Values);

                            if (actions.Count() == 1)
                            {
                                SendCloseNotificationsEvent();
                            }
                        };

                        notification.RightClick += (object sender, MouseEventArgs e) =>
                        {
                            SendCloseNotificationsEvent();
                        };

                        notification.ShowInactiveTopmost();
                        notification.FadeIn();
                    }

                    var titleNotification = new NotificationForm(this, rule.Label);
                    titleNotification.StartPosY = (startPosY -= titleNotification.Height + 5);

                    titleNotification.RightClick += (object sender, MouseEventArgs e) =>
                    {
                        SendCloseNotificationsEvent();
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

        private QuickAction GetFirstQuickAction(bool copyOnly, out string[] urlValues)
        {
            var rule = ClipboardRules.FirstOrDefault();
            if (rule != null)
            {
                urlValues = rule.Values;

                var actions = rule.QuickActions.Where(a => a.IsEnabled);

                if (copyOnly)
                {
                    actions = actions.Where(a => a.CanCopy);
                }

                var firstAction = actions.FirstOrDefault();

                return firstAction;
            }

            urlValues = null;
            return null;
        }

        private void CopyFirstLink()
        {
            var action = GetFirstQuickAction(true, out string[] urlValues);

            if (action != null)
            {
                SendCloseNotificationsEvent();

                action.Copy(urlValues);

                var titleNotification = new NotificationForm(this, String.Format("{0} link copied", action.Name));
                titleNotification.StartPosY = Screen.PrimaryScreen.WorkingArea.Height - (titleNotification.Height + 5);

                titleNotification.RightClick += (object sender, MouseEventArgs e) =>
                {
                    SendCloseNotificationsEvent();
                };
                titleNotification.ShowInactiveTopmost();
                titleNotification.FadeIn();
            }

        }

        private void OpenFirstAction()
        {
            var action = GetFirstQuickAction(true, out string[] urlValues);

            action?.Start(urlValues);
        }

        #endregion

        void OnClipboardHistoryChanged(object sender, NewClipItemEventEventArgs e)
        {
            ClipboardRules = ClipboardRule.GetMatchingRules(e.ClipItem.Text).ToList();

            if (e.ClipItem != ClipItem.Empty)
            {
                if (Configuration.AlwaysShowNotifications || e.ClipItem != e.PreviousClipItem)
                {
                    ShowNotifications();
                }
            }

            DrawMenuItems();
        }

        void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.C)
            {
                Clipboard.SetText(ClipboardHistory.CurrentClip.Text);
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.W)
            {
                ShowNotifications();
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.X)
            {
                CopyFirstLink();
            }
            else if (e.Modifier == (ModifierKeys.Win | ModifierKeys.Shift) && e.Key == Keys.D)
            {
                OpenFirstAction();
            }
        }

        private static (string label, string length) Ellipsis(string label)
        {
            if (String.IsNullOrWhiteSpace(label))
            {
                return (label, String.Empty);
            }

            label = Regex.Replace(label, @"([\r\n]+|\t+|\s+)", " ").Replace("&", "&&").Trim();

            if (label.Length > EllipsisLength + 1)
            {
                return (label : String.Concat(label.Substring(0, EllipsisLength).TrimEnd(), "…").PadRight(EllipsisLength + 5),
                        length : String.Format("[+{0:N0}c]", label.Length - EllipsisLength));
            }
            else
            {
                return (label, String.Empty);
            }
        }
    }
}
