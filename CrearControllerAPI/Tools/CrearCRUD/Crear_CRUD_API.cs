using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace CrearControllerAPI.Tools.CrearCRUD
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("ff38bb13-0c60-4867-b248-fc05a4834a07")]
    public class Crear_CRUD_API : ToolWindowPane, IServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crear_CRUD_API"/> class.
        /// </summary>
        public Crear_CRUD_API() : base(null)
        {
            this.Caption = "Crear CRUD API";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new Crear_CRUD_APIControl(this);
        }
    }
}
