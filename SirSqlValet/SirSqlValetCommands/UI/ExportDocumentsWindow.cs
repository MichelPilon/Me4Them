namespace SirSqlValetCommands.UI
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

    [Guid("E3464FD1-8B67-41B1-ABD6-706737E98333")]
    public class ExportDocumentsWindow : ToolWindowPane
    {
        public ExportDocumentsControl Control { get { return this.Content as ExportDocumentsControl; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1"/> class.
        /// </summary>
        public ExportDocumentsWindow() : base(null)
        {
            this.Caption = "Schema Search";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ExportDocumentsControl();
        }

        public void Intialize()
        {
            this.Caption = "Export documents";
            this.Control.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.Content = null;
            GC.Collect();
        }
    }
}
