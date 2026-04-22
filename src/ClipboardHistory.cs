using System.Collections;
using System.IO;
using System.Text.Json;

namespace ClipboardManager;

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

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Indicates whether saving is enabled.
    /// </summary>
    public bool SavingEnabled { get; private set; }

    /// <summary>
    /// JSON file path in the local application data folder.
    /// </summary>
    private readonly string localDataJsonClipsFile = Path.Combine(TaskbarApplication.LocalDataFolder, JsonClipsFile);

    private List<ClipItem> Clips { get; set; }

    public ClipboardHistoryCollection()
    {
        this.Clips = [];
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

            if (!string.IsNullOrWhiteSpace(clipItem.Text))
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
        this.Clips = [];
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
        if (File.Exists(this.localDataJsonClipsFile))
        {
            this.Clips = this.LoadFromLocalDataJson();
        }
        else
        {
            this.Clips = [];
            this.Save();
        }
    }

    private List<ClipItem> LoadFromLocalDataJson() => LoadFromJsonCore(this.localDataJsonClipsFile);

    private static List<ClipItem> LoadFromJsonCore(string jsonFilePath)
    {
        string content = File.ReadAllText(jsonFilePath);
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                return JsonSerializer.Deserialize<List<ClipItem>>(content, JsonOptions) ?? [];
            }
            catch (JsonException) { }
        }

        return [];
    }

    private void Save()
    {
        string json = JsonSerializer.Serialize(this.Clips, JsonOptions);
        File.WriteAllText(this.localDataJsonClipsFile, json);
    }

    public IEnumerator<ClipItem> GetEnumerator() => this.Clips.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.Clips.GetEnumerator();
}
