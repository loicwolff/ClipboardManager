namespace ClipboardManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    using Clipboard = SafeClipboard;

    [DebuggerDisplay("Name: {Name}")]
    public abstract class QuickAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Label shown by notification and menu buttons
        /// </summary>
        public virtual string OpenLabel { get; set; }

        /// <summary>
        /// Indicate if the link can be copied
        /// </summary>
        public virtual bool CanCopy => true;

        /// <summary>
        /// Constructor
        /// </summary>
        protected QuickAction(string name, string label)
        {
            this.Name = name;
            this.OpenLabel = label;
        }

        /// <summary>
        /// Indique si l'action est activée
        /// </summary>
        public virtual bool IsEnabled => true;

        public abstract void Run(IEnumerable<string>? optionalValues);

        public abstract void Copy(IEnumerable<string> urlValues);
    }


    public class UrlQuickAction : QuickAction
    {
        /// <summary>
        /// Uri to open
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// Optional method to complement the url
        /// </summary>
        public virtual Func<string>? GetUrlComplement { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UrlQuickAction(string name, string label, string url) : base(name, label)
        {
            this.Url = url;
        }

        /// <summary>
        /// Build the URL to open
        /// </summary>
        /// <param name="urlValues">Values to insert in the urls</param>
        /// <returns>Return the url formatted with the <paramref name="urlValues"/></returns>
        protected virtual string GetFormattedUrl(IEnumerable<string> urlValues)
        {
            if (this.Url == null)
            {
                throw new Exception();
            }

            string url = String.Format(CultureInfo.InvariantCulture, this.Url, urlValues.ToArray());

            if (this.GetUrlComplement != null)
            {
                url = String.Concat(url, this.GetUrlComplement());
            }

            return url;
        }

        public override void Copy(IEnumerable<string> urlValues)
            => Clipboard.SetText(this.GetFormattedUrl(urlValues));

        public override void Run(IEnumerable<string>? urlValues)
            => Process.Start(new ProcessStartInfo(this.GetFormattedUrl(urlValues ?? Enumerable.Empty<string>())) { UseShellExecute = true });
    }



    ///// <summary>
    ///// Quick action qui ouvre une application installée sur le poste
    ///// </summary>
    //public class AppQuickAction : QuickAction
    //{
    //    public AppQuickAction(string name, string label) : base(name, label)
    //    {
    //        this.CanCopy = false;
    //    }

    //    /// <summary>
    //    /// Indique s'il faut dire à l'application recevant d'utiliser le presse-papier
    //    /// </summary>
    //    public bool UseClipboard { get; set; }

    //    /// <summary>
    //    /// Arguments à passer à l'application
    //    /// </summary>
    //    public virtual string? Arguments { get; set; }

    //    /// <summary>
    //    /// Vérifie si l'application est installée
    //    /// </summary>
    //    public override bool IsEnabled => File.Exists(Url);

    //    /// <summary>
    //    /// Ouvre l'application avec les paramètres nécessaires
    //    /// </summary>
    //    /// <param name="urlValues"></param>
    //    public override void Start(IEnumerable<string> urlValues)
    //    {
    //        try
    //        {
    //            string arguments = String.Format( this.Arguments, urlValues.ToArray());

    //            if (UseClipboard && arguments.Length > 30000)
    //            {
    //                Process.Start(Url, "/clip");
    //            }
    //            else
    //            {
    //                Process.Start(Url, arguments);
    //            }
    //        }
    //        catch (Win32Exception ex)
    //        {
    //            MessageBox.Show(ex.Message, "Cannot open application", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Quick Action qui permet d'extraire et formater une partie du presse papier
    ///// </summary>
    //public class ExtractQuickAction : QuickAction
    //{
    //    public ExtractQuickAction(string name, string label) : base(name, label)
    //    {

    //    }

    //    public override bool CanCopy => false;

    //    public override void Run(IEnumerable<string>? urlValues)
    //    {



    //        Clipboard.SetText(value);
    //    }
    //}
}
