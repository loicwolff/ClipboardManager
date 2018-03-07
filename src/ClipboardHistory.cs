using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ClipboardManager
{
    public class NewClipItemEventEventArgs : EventArgs
    {
        public ClipItem PreviousClipItem { get; set; }
        public ClipItem ClipItem { get; set; }
    }

    public class ClipboardHistory : IEnumerable<ClipItem>
    {
        public event EventHandler<NewClipItemEventEventArgs> OnHistoryChanged;

        private const string JsonClipsFile = "clips.json";
        
        private const int MaxClipCount = 30;
        
        /// <summary>
        /// Dossier de l'application dans ApplicationData
        /// </summary>
        private string LocalDataFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Clipboard Manager");
            }
        }

        /// <summary>
        /// Indique si la sauvegarde est activée
        /// </summary>
        public bool SavingEnabled { get; private set; }

        /// <summary>
        /// Fichier JSON dans le dossier LocalData
        /// </summary>
        private string LocalDataJsonClipsFile
        {
            get
            {
                if (!Directory.Exists(LocalDataFolder))
                {
                    Directory.CreateDirectory(LocalDataFolder);
                }

                return Path.Combine(LocalDataFolder, "clips.json");
            }
        }

        private List<ClipItem> Clips { get; set; }

        public ClipboardHistory()
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
                ClipItem previousClip = CurrentClip;

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
        
        private List<ClipItem> LoadFromLocalDataJson()
        {
            return LoadFromJsonCore(LocalDataJsonClipsFile);
        }

        private List<ClipItem> LoadFromJsonCore(string jsonFilePath)
        {
            List<ClipItem> items = new List<ClipItem>();

            using (StreamReader reader = new StreamReader(jsonFilePath))
            {
                string content;
                if (!String.IsNullOrWhiteSpace((content = reader.ReadToEnd())))
                {
                    try
                    {
                        items = JsonConvert.DeserializeObject<List<ClipItem>>(content);
                    }
                    catch (Exception)
                    {
                        //Trace.TraceError("Erreur de lecture de clips.json");
                        items = new List<ClipItem>();
                    }
                }
            }

            return items;
        }

        private void Save()
        {
            using (StreamWriter writer = new StreamWriter(LocalDataJsonClipsFile))
            {
                string json = JsonConvert.SerializeObject(Clips, Formatting.Indented);
                writer.Write(json);
            }
        }

        public IEnumerator<ClipItem> GetEnumerator()
        {
            return this.Clips.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Clips.GetEnumerator();
        }
    }
}
