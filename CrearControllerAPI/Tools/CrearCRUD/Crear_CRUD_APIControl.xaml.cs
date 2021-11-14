using CrearControllerAPI.Negocio;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace CrearControllerAPI.Tools.CrearCRUD
{
    public partial class Crear_CRUD_APIControl : UserControl
    {
        private IServiceProvider _serviceProvider;

        public Crear_CRUD_APIControl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            this.InitializeComponent();
        }

        private void btoCrearCRUD_Click(object sender, RoutedEventArgs e)
        {
            Helper helper = new Helper(_serviceProvider);

            if (!string.IsNullOrEmpty(cbxContexto.SelectedItem.ToString()) && !string.IsNullOrEmpty(cbxModelo.SelectedItem.ToString()) && !string.IsNullOrEmpty(txtPrimaryKey.Text))
            {
                helper.GenerarCRUD(cbxContexto.SelectedItem.ToString(), cbxModelo.SelectedItem.ToString(), txtPrimaryKey.Text);
            }
            else
            {
                helper.Msg("Todos los campos son requeridos");
            }
        }

        private void btoLoadCombos_Click(object sender, RoutedEventArgs e)
        {
            Helper helper = new Helper(_serviceProvider);

            helper.PopularCombos(cbxContexto, cbxModelo);

        }
    }
}