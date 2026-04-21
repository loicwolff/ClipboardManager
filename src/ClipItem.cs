using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ClipboardManager;

[DebuggerDisplay("Text = {Text}")]
public class ClipItem
{
    public static readonly ClipItem Empty = new(text: string.Empty);

    [JsonConstructor]
    public ClipItem(string text)
    {
        this.Text = text;
    }

    [JsonPropertyName("text")]
    public string? Text { get; }

    [JsonIgnore]
    public bool IsEmpty => string.IsNullOrWhiteSpace(this.Text);

    public override bool Equals(object? obj) => obj is ClipItem clipItem && clipItem.Text == this.Text;

    public override int GetHashCode() => HashCode.Combine(this.Text);

    public static bool operator ==(ClipItem? c1, ClipItem? c2) => Equals(c1, c2);

    public static bool operator !=(ClipItem? c1, ClipItem? c2) => !Equals(c1, c2);
}
