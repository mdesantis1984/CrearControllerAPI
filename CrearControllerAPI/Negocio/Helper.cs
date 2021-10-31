
namespace CrearControllerAPI.Negocio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnvDTE;
    using EnvDTE80;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Packaging;
    using System.Text;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class Helper
    {

        IServiceProvider _serviceProvider;

        public Helper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async void GenerarCRUD(string NombreContexto, string NombreModelo, string NombrePrimaryKey)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (DTE)_serviceProvider.GetService(typeof(DTE)) as DTE2;
            UIHierarchy uih = (UIHierarchy)dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            Array selectedItems = (Array)uih.SelectedItems;

            foreach (UIHierarchyItem selectedItem in selectedItems)
            {

                Project project = GetProject(selectedItem.Object);

                if (project != null)
                {
                    try
                    {
                        var rootPath = project.FullName;
                        DirectoryInfo info = new DirectoryInfo(rootPath);
                        DirectoryInfo rootProject = new DirectoryInfo(info.Parent.FullName);

                        var folder = Path.Combine(rootProject.FullName, "Data\\Services");
                        var newPath = Path.Combine(rootProject.FullName,"Data\\Services", $"Services{NombreModelo}.cs");

                        bool exists = System.IO.Directory.Exists(folder);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(folder);


                        var tmpStringFile = ReadAndRemplace(rootProject.FullName, NombreContexto, NombreModelo, NombrePrimaryKey);

                        using (FileStream fs = File.Create(newPath, 1024, FileOptions.Asynchronous))
                        {
                            byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmpStringFile);

                            fs.Write(tmpFile, 0, tmpFile.Length);
                        }

                        var tmp = ReadAndRegisterService(rootProject.FullName, NombreModelo);

                        using (FileStream fs = File.Create($"{Path.Combine(rootProject.FullName, "Startup.cs")}", 1024, FileOptions.Asynchronous))
                        {
                            byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmp);

                            fs.Write(tmpFile, 0, tmpFile.Length);
                        }

                        Msg("El Servicio fue creado y registrado correctamente !!!");
                    }
                    catch (Exception ex)
                    {
                        Msg(ex.Message);
                    }
                }
                else
                {
                    Msg("Detectamos que no se encontro el proyecto");
                }

            }
        }

        public static string AssemblyDirectoryResource
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                
                var root = Path.GetDirectoryName(path);
                var folder = Path.Combine(root, "Tools\\CrearCRUD\\Resources");

                return folder;
            }
        }

       

        /// <summary>
        ///  Inicia el registro despues de este comodin | //##Start##
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="NombreModelo"></param>
        /// <returns></returns>
        public string ReadAndRegisterService(string rootFolder, string NombreModelo)
        {
            StringBuilder str = new StringBuilder();

            var template = Path.Combine(rootFolder, "Startup.cs");
            //int Count = 0;

            foreach (var Line in File.ReadLines(template))
            {
                

                if ((Line.RemoveWhitespace()).ToUpper() == "//##Services##".ToUpper())
                {
                    str.AppendLine(Line);
                    str.Append($"           services.AddScoped<Services{NombreModelo}>(); //Generado Automaticamente");
                    str.AppendLine();
                }
                else
                {
                    str.AppendLine(Line);
                }

                //Count++; 
            } 
            
            return str.ToString();
        }

        public string ReadAndRemplace(string rootFolder, string NombreContexto, string NombreModelo, string NombrePrimaryKey)
        {
            StringBuilder str = new StringBuilder();

            var template = Path.Combine(AssemblyDirectoryResource, "template1.txt");
            var tmpNamespace = rootFolder.Split(new char[] {'\\'}).Last();

            string text = File.ReadAllText(template);
            text = text.Replace("::{namespace}::", tmpNamespace);
            text = text.Replace("::{nombreContexto}::", NombreContexto);
            text = text.Replace("::{nombreModelo}::", NombreModelo);
            text = text.Replace("::{nombrePrimaryKey}::", NombrePrimaryKey);

            str.Append(text);
            
            return str.ToString();
        }


        public void Msg(string msg)
        {
            VsShellUtilities.ShowMessageBox(
                _serviceProvider,
                msg,
                "Atencion!!!",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public Project GetProject(object selectedItemObject)
        {
            var project = selectedItemObject as Project;
            if (project != null)
                return project;

            var item = selectedItemObject as ProjectItem;
            if (item == null)
                return null;

            return item.SubProject;
        }

        public ProjectItem GetProjectIetm(object selectedItemObject)
        {
            var item = selectedItemObject as ProjectItem;
            if (item == null)
                return null;

            return item;
        }


    }

}