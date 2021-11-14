
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
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Reflection;

    public class Helper
    {

        IServiceProvider _serviceProvider;

        public Helper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async void PopularCombos(ComboBox contexto, ComboBox Modelo)
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
                        var root = project.FullName;

                        /* Contexto */
                        List<string> listContexto = new List<string>();
                        listContexto = GetContextProyect(root);
                        contexto.ItemsSource = listContexto;

                        /* Contexto */
                        List<string> listModels = new List<string>();
                        listModels = GetModelsProyect(root);
                        Modelo.ItemsSource = listModels;

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

        public List<string> GetContextProyect(string rootPath)
        {
            List<string> listContexto = new List<string>();

            try
            {
                DirectoryInfo Proyecto = new DirectoryInfo(rootPath);
                IEnumerable<FileInfo> files = Proyecto.Parent.GetFiles("*.cs", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    foreach (var item in File.ReadLines(file.FullName))
                    {
                        if (item.Contains("DbContext"))
                        {
                            listContexto.Add(Path.GetFileNameWithoutExtension(file.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Msg(ex.Message);
            }

            return listContexto;
        }

        public List<string> GetModelsProyect(string rootPath)
        {
            List<string> listModels = new List<string>();

            try
            {
                DirectoryInfo Proyecto = new DirectoryInfo(rootPath);
                DirectoryInfo[] Directory = Proyecto.Parent.GetDirectories("Models", SearchOption.AllDirectories);

                foreach (DirectoryInfo file in Directory)
                {
                    IEnumerable<FileInfo> files = file.GetFiles("*.cs", SearchOption.AllDirectories);

                    foreach (FileInfo item in files)
                    {
                        listModels.Add(Path.GetFileNameWithoutExtension(item.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                Msg(ex.Message);
            }

            return listModels;
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
                        var Interface = Path.Combine(rootProject.FullName, "Data\\Interface");
                        var newPath = Path.Combine(rootProject.FullName, "Data\\Services", $"Services{NombreModelo}.cs");

                        /* Si no exite la carpeta Services la crea */
                        bool exists = System.IO.Directory.Exists(folder);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(folder);

                        /* Si no exite la carpeta Interface la crea */
                        bool existsInterface = System.IO.Directory.Exists(Interface);
                        if (!existsInterface)
                            System.IO.Directory.CreateDirectory(Interface);


                        /* Crea el services */
                        var tmpStringFile = ReadAndRemplace(rootProject.FullName, NombreContexto, NombreModelo, NombrePrimaryKey);
                        using (FileStream fs = File.Create(newPath, 1024, FileOptions.Asynchronous))
                        {
                            byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmpStringFile);

                            fs.Write(tmpFile, 0, tmpFile.Length);
                        }

                        /* Registra el services */
                        var tmp = ReadAndRegisterService(rootProject.FullName, NombreModelo);
                        if (!string.IsNullOrWhiteSpace(tmp))
                        {
                            using (FileStream fs = File.Create($"{Path.Combine(rootProject.FullName, "Startup.cs")}", 1024, FileOptions.Asynchronous))
                            {
                                byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmp);

                                fs.Write(tmpFile, 0, tmpFile.Length);
                            }
                        }
                        else
                        {
                            var tmp2 = ReadAndNewRegisterService(rootProject.FullName, NombreModelo);
                            using (FileStream fs = File.Create($"{Path.Combine(rootProject.FullName, "Program.cs")}", 1024, FileOptions.Asynchronous))
                            {
                                byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmp2);

                                fs.Write(tmpFile, 0, tmpFile.Length);
                            }
                        }


                        /* Crea la interface */
                        var InterfaceFile = $"{rootProject.FullName}\\Data\\Interface\\IServiceScopeFactory.cs";
                        var tmpInterfaceFile = ReadAndCreateIServicesFactory(rootProject.FullName);
                        using (FileStream fs = File.Create(InterfaceFile, 1024, FileOptions.Asynchronous))
                        {
                            byte[] tmpFile = new UTF8Encoding(true).GetBytes(tmpInterfaceFile);

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

            var isExit = File.Exists(template);

            if (isExit)
            {
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
            }
            return str.ToString();
        }
        public string ReadAndCreateIServicesFactory(string rootFolder)
        {
            StringBuilder str = new StringBuilder();

            var tmpUriInterfaceFile = Path.Combine(AssemblyDirectoryResource, "IServiceScopeFactory.txt");
            var tmpNamespace = rootFolder.Split(new char[] { '\\' }).Last();

            string text = File.ReadAllText(tmpUriInterfaceFile);
            text = text.Replace("::{namespace}::", tmpNamespace);

            str.Append(text);

            return str.ToString();
        }
        public string ReadAndNewRegisterService(string rootFolder, string NombreModelo)
        {
            StringBuilder str = new StringBuilder();

            var template = Path.Combine(rootFolder, "Program.cs");

            foreach (var Line in File.ReadLines(template))
            {
                if ((Line.RemoveWhitespace()).ToUpper() == "//##Services##".ToUpper())
                {
                    str.AppendLine(Line);
                    str.Append($"           builder.Services.AddScoped<Services{NombreModelo}>(); //Generado Automaticamente");
                    str.AppendLine();
                }
                else
                {
                    str.AppendLine(Line);
                }

            }
            return str.ToString();
        }

        public string ReadAndRemplace(string rootFolder, string NombreContexto, string NombreModelo, string NombrePrimaryKey)
        {
            StringBuilder str = new StringBuilder();

            var template = Path.Combine(AssemblyDirectoryResource, "template2.txt");
            var tmpNamespace = rootFolder.Split(new char[] { '\\' }).Last();

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
            Project project = selectedItemObject as Project;
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