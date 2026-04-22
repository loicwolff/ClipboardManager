using System.Diagnostics;
using System.Globalization;

using Clipboard = ClipboardManager.SafeClipboard;

namespace ClipboardManager;

[DebuggerDisplay("Name: {Name}")]
public abstract class QuickAction
{
    /// <summary>
    /// Action name.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Label shown by notification and menu buttons.
    /// </summary>
    public virtual string OpenLabel { get; set; }

    /// <summary>
    /// Indicates whether the link can be copied.
    /// </summary>
    public virtual bool CanCopy => true;

    /// <summary>
    /// Constructor.
    /// </summary>
    protected QuickAction(string name, string label)
    {
        this.Name = name;
        this.OpenLabel = label;
    }

    /// <summary>
    /// Indicates whether the action is enabled.
    /// </summary>
    public virtual bool IsEnabled => true;

    public abstract void Run(IEnumerable<string>? optionalValues);

    public abstract void Copy(IEnumerable<string> urlValues);
}

public class UrlQuickAction : QuickAction
{
    /// <summary>
    /// URI to open.
    /// </summary>
    public virtual string Url { get; set; }

    /// <summary>
    /// Optional method to complement the URL.
    /// </summary>
    public virtual Func<string>? GetUrlComplement { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public UrlQuickAction(string name, string label, string url) : base(name, label)
    {
        this.Url = url;
    }

    /// <summary>
    /// Build the URL to open.
    /// </summary>
    /// <param name="urlValues">Values to insert in the URL.</param>
    /// <returns>The URL formatted with the <paramref name="urlValues"/>.</returns>
    protected virtual string GetFormattedUrl(IEnumerable<string> urlValues)
    {
        string url = string.Format(CultureInfo.InvariantCulture, this.Url, urlValues.ToArray());

        if (this.GetUrlComplement != null)
        {
            url = string.Concat(url, this.GetUrlComplement());
        }

        return url;
    }

    public override void Copy(IEnumerable<string> urlValues)
        => Clipboard.SetText(this.GetFormattedUrl(urlValues));

    public override void Run(IEnumerable<string>? urlValues)
        => Process.Start(new ProcessStartInfo(this.GetFormattedUrl(urlValues ?? [])) { UseShellExecute = true });
}
