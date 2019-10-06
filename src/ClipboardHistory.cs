namespace ClipboardManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    public class NewClipItemEventEventArgs : EventArgs
    {
        public ClipItem PreviousClipItem { get; set; }
        public ClipItem ClipItem { get; set; }
    }

    public class ClipboardHistoryCollection : IEnumerable<ClipItem>
    {
        public event EventHandler<NewClipItemEventEventArgs> OnHistoryChanged;

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
            this.SavingEnabled = true;

            LoadHistory();

            AddClipItem(SafeClipboard.CurrentClipItem);

            ClipboardNotifier.ClipboardUpdate += ClipboardNotifier_ClipboardUpdate;
        }

        void ClipboardNotifier_ClipboardUpdate(object sender, ClipboardEventArgs e)
        {
            if (SavingEnabled)
            {
                AddClipItem(e.ClipItem);
            }
        }

        public ClipItem CurrentClip
        {
            get;
            internal set;
        }

        internal void AddClipItem(ClipItem clipItem)
        {
            if (SavingEnabled)
            {
                var previousClip = CurrentClip;

                CurrentClip = clipItem;

                if (!String.IsNullOrWhiteSpace(clipItem.Text))
                {
                    Clips.RemoveAll(c => c.Text == clipItem.Text);

                    Clips.Add(clipItem);

                    int overhead = Clips.Count - MaxClipCount;
                    if (overhead > 0)
                    {
                        Clips = Clips.Skip(overhead).ToList();
                    }

                    Save();
                }

                SendHistoryChangedEvent(CurrentClip, previousClip);
            }
        }

        public void RemoveAll(ClipItem clipItem)
        {
            Clips = Clips.Where(c => c != clipItem).ToList();

            if (CurrentClip == clipItem)
            {
                SafeClipboard.Clear();
                CurrentClip = ClipItem.Empty;
            }

            Save();

            SendHistoryChangedEvent(CurrentClip);
        }

        public void Clear()
        {
            Clips = new List<ClipItem>();
            CurrentClip = ClipItem.Empty;

            SafeClipboard.Clear();

            Save();

            SendHistoryChangedEvent(ClipItem.Empty);
        }

        public void ToggleSavingEnabled()
        {
            SavingEnabled = !SavingEnabled;

            AddClipItem(SafeClipboard.CurrentClipItem);
        }

        private void SendHistoryChangedEvent(ClipItem newClip, ClipItem previousClip = null)
        {
            OnHistoryChanged?.Invoke(this, new NewClipItemEventEventArgs() { ClipItem = newClip, PreviousClipItem = previousClip });
        }

        private void LoadHistory()
        {
            if (File.Exists(LocalDataJsonClipsFile))
            {
                Clips = LoadFromLocalDataJson();
            }
            else
            {
                Clips = new List<ClipItem>();
                Save();
            }
        }

        private List<ClipItem> LoadFromLocalDataJson() => LoadFromJsonCore(LocalDataJsonClipsFile);

        private static List<ClipItem> LoadFromJsonCore(string jsonFilePath)
        {
            using (var reader = new StreamReader(jsonFilePath))
            {
                string content;
                if (!String.IsNullOrWhiteSpace((content = reader.ReadToEnd())))
                {
                    try
                    {
                        return JsonSerializer.Deserialize<List<ClipItem>>(content);
                    }
                    catch (Exception) { }
                }
            }

            return new List<ClipItem>();
        }

        private void Save()
        {
            using (var writer = new StreamWriter(LocalDataJsonClipsFile))
            {
                string json = JsonSerializer.Serialize<List<ClipItem>>(Clips, new JsonSerializerOptions() { WriteIndented = true });
                writer.Write(json);
            }
        }

        public IEnumerator<ClipItem> GetEnumerator() => this.Clips.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Clips.GetEnumerator();
    }
}
