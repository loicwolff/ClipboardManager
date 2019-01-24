namespace ClipboardManager
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using Clipboard = SafeClipboard;

    [DebuggerDisplay("Name: {Name}")]
    public class QuickAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Uri to open
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// Label shown by notification and menu buttons
        /// </summary>
        public virtual string OpenLabel { get; set; }

        /// <summary>
        /// Indicate if the link can be copied
        /// </summary>
        public virtual bool CanCopy { get; set; }

        /// <summary>
        /// Optional method to complement the url
        /// </summary>
        public virtual Func<string> GetUrlComplement { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QuickAction() => CanCopy = true;

        /// <summary>
        /// Build the URL to open
        /// </summary>
        /// <param name="urlValues">Values to insert in the urls</param>
        /// <returns>Return the url formatted with the <paramref name="urlValues"/></returns>
        protected virtual string GetFormattedUrl(string[] urlValues)
        {
            string url = String.Format(Url, urlValues);

            if (GetUrlComplement != null)
            {
                url = String.Concat(url, GetUrlComplement());
            }

            return url;
        }

        /// <summary>
        /// Method to copy the url in the clipboard
        /// </summary>
        /// <param name="urlValues">Les données extraites du presse-papier</param>
        public virtual void Copy(string[] urlValues) => Clipboard.SetText(GetFormattedUrl(urlValues));

        /// <summary>
        /// Méthode pour ouvrir l'URL formattée
        /// </summary>
        /// <param name="urlValues">Les données extraites du presse-papier</param>
        public virtual void Start(string[] urlValues) => Process.Start(GetFormattedUrl(urlValues));

        /// <summary>
        /// Indique si l'action est activée
        /// </summary>
        public virtual bool IsEnabled => true;
    }
    
    /// <summary>
    /// Quick action qui ouvre une application installée sur le poste
    /// </summary>
    public class AppQuickAction : QuickAction
    {
        public AppQuickAction() => CanCopy = false;

        /// <summary>
        /// Indique s'il faut dire à l'application recevant d'utiliser le presse-papier
        /// </summary>
        public bool UseClipboard { get; set; }

        /// <summary>
        /// Arguments à passer à l'application
        /// </summary>
        public virtual string Arguments { get; set; }

        /// <summary>
        /// Vérifie si l'application est installée
        /// </summary>
        public override bool IsEnabled => File.Exists(Url);

        /// <summary>
        /// Ouvre l'application avec les paramètres nécessaires
        /// </summary>
        /// <param name="urlValues"></param>
        public override void Start(string[] urlValues)
        {
            try
            {
                string arguments = String.Format(this.Arguments, urlValues);

                if (UseClipboard && arguments.Length > 30000)
                {
                    Process.Start(Url, "/clip");
                }
                else
                {
                    Process.Start(Url, arguments);
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot open application", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Quick Action qui permet d'extraire et formater une partie du presse papier
    /// </summary>
    public class ExtractQuickAction : QuickAction
    {
        public override bool CanCopy => false;

        public override void Start(string[] urlValues)
        {
            string value = String.Format(this.Url, urlValues);

            Clipboard.SetText(value);
        }
    }
}
