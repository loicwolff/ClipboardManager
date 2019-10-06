namespace ClipboardManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public class NewClipItemEventEventArgs : EventArgs
    {
        public ClipItem? PreviousClipItem { get; set; }
        public ClipItem ClipItem { get; set; } = ClipItem.Empty;
    }

    public class ClipboardHistoryCollection : IEnumerable<ClipItem>
    {
        public event EventHandler<NewClipItemEventEventArgs>? OnHistoryChanged;

        private const string JsonClipsFile = "clips.json";

        private const int MaxClipCount = 30;

        /// <summary>
        /// Indique si la sauvegarde est activée
        /// </summary>
        public bool SavingEnabled { get; private set; }

        /// <summary>
        /// Fichier JSON dans le dossier LocalData
        /// </summary>
        private readonly string LocalDataJsonClipsFile = Path.Combine(TaskbarApplication.LocalDataFolder, JsonClipsFile);

        private List<ClipItem> Clips { get; set; }

        public ClipboardHistoryCollection()
        {
            this.Clips = new List<ClipItem>();
            this.CurrentClip = ClipItem.Empty;

            this.SavingEnabled = true;

            this.LoadHistory();

            this.AddClipItem(SafeClipboard.CurrentClipItem);

            ClipboardNotifier.ClipboardUpdate += this.ClipboardNotifier_ClipboardUpdate;
        }

        void ClipboardNotifier_ClipboardUpdate(object? sender, ClipboardEventArgs e)
        {
            if (this.SavingEnabled)
            {
                this.AddClipItem(e.ClipItem);
            }
        }

        public ClipItem CurrentClip
        {
            get;
            internal set;
        }

        internal void AddClipItem(ClipItem clipItem)
        {
            if (this.SavingEnabled)
            {
                var previousClip = this.CurrentClip;

                this.CurrentClip = clipItem;

                if (!String.IsNullOrWhiteSpace(clipItem.Text))
                {
                    this.Clips.RemoveAll(c => c.Text == clipItem.Text);

                    this.Clips.Add(clipItem);

                    int overhead = this.Clips.Count - MaxClipCount;
                    if (overhead > 0)
                    {
                        this.Clips = this.Clips.Skip(overhead).ToList();
                    }

                    this.Save();
                }

                this.SendHistoryChangedEvent(this.CurrentClip, previousClip);
            }
        }

        public void RemoveAll(ClipItem clipItem)
        {
            this.Clips = this.Clips.Where(c => c != clipItem).ToList();

            if (this.CurrentClip == clipItem)
            {
                SafeClipboard.Clear();
                this.CurrentClip = ClipItem.Empty;
            }

            this.Save();

            this.SendHistoryChangedEvent(this.CurrentClip);
        }

        public void Clear()
        {
            this.Clips = new List<ClipItem>();
            this.CurrentClip = ClipItem.Empty;

            SafeClipboard.Clear();

            this.Save();

            this.SendHistoryChangedEvent(ClipItem.Empty);
        }

        public void ToggleSavingEnabled()
        {
            this.SavingEnabled = !this.SavingEnabled;

            this.AddClipItem(SafeClipboard.CurrentClipItem);
        }

        private void SendHistoryChangedEvent(ClipItem newClip, ClipItem? previousClip = null)
        {
            OnHistoryChanged?.Invoke(this, new NewClipItemEventEventArgs() { ClipItem = newClip, PreviousClipItem = previousClip });
        }

        private void LoadHistory()
        {
            if (File.Exists(this.LocalDataJsonClipsFile))
            {
                this.Clips = this.LoadFromLocalDataJson();
            }
            else
            {
                this.Clips = new List<ClipItem>();
                this.Save();
            }
        }

        private List<ClipItem> LoadFromLocalDataJson() => LoadFromJsonCore(this.LocalDataJsonClipsFile);

        private static List<ClipItem> LoadFromJsonCore(string jsonFilePath)
        {
            using (var reader = new StreamReader(jsonFilePath))
            {
                string content;
                if (!String.IsNullOrWhiteSpace((content = reader.ReadToEnd())))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<ClipItem>>(content);
                    }
                    catch (JsonException) { }
                }
            }

            return new List<ClipItem>();
        }

        private void Save()
        {
            using (var writer = new StreamWriter(this.LocalDataJsonClipsFile))
            {
                string json = JsonConvert.SerializeObject(this.Clips, Formatting.Indented);
                writer.Write(json);
            }
        }

        public IEnumerator<ClipItem> GetEnumerator() => this.Clips.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Clips.GetEnumerator();
    }
}
