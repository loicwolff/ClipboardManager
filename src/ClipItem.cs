namespace ClipboardManager
{
    using System;
    using System.Diagnostics;
    using System.Text.Json.Serialization;

    [DebuggerDisplay("Text = {Text}")]
    public class ClipItem
    {
        public static readonly ClipItem Empty = new ClipItem { Text = String.Empty };
        
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonIgnore]
        public bool IsEmpty => String.IsNullOrWhiteSpace(Text);
        
        public override bool Equals(object obj) => obj is ClipItem clipItem && clipItem.Text == this.Text;

        public override int GetHashCode() => this.Text.GetHashCode();

        public static bool operator ==(ClipItem c1, ClipItem c2) => c1.Equals(c2);

        public static bool operator !=(ClipItem c1, ClipItem c2) => !c1.Equals(c2);
    }
}
