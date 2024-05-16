using GeneradorCufe.Consultas;
using GeneradorCufe.ViewModel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeneradorCufe
{

    public partial class MainWindow : Window
    {
        private InvoiceViewModel _viewModel;
        private Factura_Consulta _facturaConsulta; // Declarar una instancia de Factura_Consulta

        public MainWindow()
        {
            InitializeComponent();
            // Crear una instancia de Factura_Consulta y comenzar a ejecutar la consulta
            _facturaConsulta = new Factura_Consulta();
           

            // Inicializa el ViewModel sin importar el número de partes.
            _viewModel = new InvoiceViewModel();
          
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "HH:MM:SS")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "HH:MM:SS";
                textBox.Foreground = Brushes.Gray;
            }
        }

       

        private void BtnCufe_Click_1(object sender, RoutedEventArgs e)
        {
            //if (ValidarDatos())
            //{
            //    // Construir la cadena CUFE y generar el CUFE
            //    _viewModel.CadenaCUFE = ConstruirCadenaCUFE();
            //    _viewModel.CUFE = GenerarCUFE(_viewModel.CadenaCUFE);

            //    // Mostrar la cadena y el CUFE en los TextBoxes (opcional)
            //    TxtCadenaCUFE.Text = _viewModel.CadenaCUFE;
            //    TxtCadenaCUFE.Visibility = Visibility.Visible;
            //    TxtCUFE.Text = "CUFE generado: " + _viewModel.CUFE;
            //    TxtCUFE.Visibility = Visibility.Visible;
            //}

        }

        


        private void BtnCufe_Click_2(object sender, RoutedEventArgs e)
        {
            //EjecutarGeneracionXML();
        }


        private void ConvertirXML_Click(object sender, RoutedEventArgs e)
        {
            // Crear un cuadro de diálogo para seleccionar archivos XML
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos XML (*.xml)|*.xml";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Directorio inicial

            // Mostrar el cuadro de diálogo
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Obtener la ruta del archivo XML seleccionado
                string xmlFilePath = openFileDialog.FileName;

                try
                {
                    // Leer el contenido del archivo XML
                    string xmlContent = File.ReadAllText(xmlFilePath);

                    // Convertir el contenido a base64
                    byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
                    string base64Content = Convert.ToBase64String(bytes);

                    // Obtener el nombre del archivo sin la extensión
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(xmlFilePath);

                    // Guardar el contenido base64 en un archivo de texto con el mismo nombre
                    string base64FilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(xmlFilePath), fileNameWithoutExtension + ".txt");
                    File.WriteAllText(base64FilePath, base64Content);

                    // Mostrar mensaje de éxito
                    MessageBox.Show("El archivo XML se ha convertido a base64 y se ha guardado en: " + base64FilePath, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Mostrar mensaje de error si ocurre una excepción
                    MessageBox.Show("Error al convertir el archivo XML a base64: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MiTextBoxHora_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
      
    }
}