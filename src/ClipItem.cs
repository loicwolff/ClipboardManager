namespace ClipboardManager
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;

    [DebuggerDisplay("Text = {Text}")]
    public class ClipItem
    {
        public static readonly ClipItem Empty = new ClipItem(text: string.Empty);

        protected ClipItem()
        {

        }

        public ClipItem(string text)
        {
            this.Text = text;
        }

        [JsonProperty("text")]
        public string? Text { get; }

        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrWhiteSpace(this.Text);

        public override bool Equals(object? obj) => obj is ClipItem clipItem && clipItem.Text == this.Text;

        public override int GetHashCode() => HashCode.Combine(this.Text);

        public static bool operator ==(ClipItem? c1, ClipItem? c2) => Equals(c1, c2);

        public static bool operator !=(ClipItem? c1, ClipItem? c2) => !Equals(c1, c2);
    }
}
