using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ClipboardManager
{
    [DebuggerDisplay("Text = {Text}")]
    public class ClipItem
    {
        public static readonly ClipItem Empty = new ClipItem { Text = String.Empty };
        
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonIgnore]
        public bool IsEmpty => String.IsNullOrWhiteSpace(Text);

        public override bool Equals(object obj)
        {
            if (!(obj is ClipItem))
                return false;

            ClipItem clipItem = (ClipItem)obj;

            return clipItem.Text == this.Text;
        }

        public override int GetHashCode() => this.Text.GetHashCode();

        public static bool operator ==(ClipItem c1, ClipItem c2) => c1.Equals(c2);

        public static bool operator !=(ClipItem c1, ClipItem c2) => !c1.Equals(c2);
    }
}
