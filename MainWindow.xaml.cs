﻿using GeneradorCufe.ViewModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

        public MainWindow()
        {
            InitializeComponent();
            string loadedData = DataSerializer.LoadData();
            string[] parts = loadedData.Split(',');

            // Inicializa el ViewModel sin importar el número de partes.
            _viewModel = new InvoiceViewModel();

            if (parts.Length > 0) _viewModel.NumeroFactura = parts[0];
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) _viewModel.FechaFactura = DateTime.ParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (parts.Length > 2) _viewModel.ValorSubtotal = parts[2];
            if (parts.Length > 3) _viewModel.HoraGeneracion = parts[3];
            if (parts.Length > 4) _viewModel.ValorIVA = parts[4];
            if (parts.Length > 5) _viewModel.ValorImpuesto2 = parts[5];
            if (parts.Length > 6) _viewModel.ValorImpuesto3 = parts[6];
            if (parts.Length > 7) _viewModel.TotalPagar = parts[7];
            if (parts.Length > 8) _viewModel.NITFacturadorElectronico = parts[8];
            if (parts.Length > 9) _viewModel.NumeroIdentificacionCliente = parts[9];
            if (parts.Length > 10) _viewModel.ClaveTecnicaControl = parts[10];
            if (parts.Length > 11) _viewModel.CadenaCUFE = parts[11];
            if (parts.Length > 12) _viewModel.CUFE = parts[12];
            if (parts.Length > 13) _viewModel.SetTestId = parts[13];

            DataContext = _viewModel;

            Closing += MainWindow_Closing;
        }


        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Añade _viewModel.CadenaCUFE y _viewModel.CUFE al final
            string dataToSave = $"{_viewModel.NumeroFactura},{_viewModel.FechaFactura.ToString("yyyy-MM-dd")},{_viewModel.ValorSubtotal},{_viewModel.HoraGeneracion},{_viewModel.ValorIVA},{_viewModel.ValorImpuesto2},{_viewModel.ValorImpuesto3},{_viewModel.TotalPagar},{_viewModel.NITFacturadorElectronico},{_viewModel.NumeroIdentificacionCliente},{_viewModel.ClaveTecnicaControl},{_viewModel.CadenaCUFE},{_viewModel.CUFE},{_viewModel.SetTestId}";

            DataSerializer.SaveData(dataToSave);
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

        private void MiTextBoxHora_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool ValidarDatos()
        {
            // Validar número de factura
            if (string.IsNullOrWhiteSpace(TxtFactura.Text))
            {
                MessageBox.Show("El número de factura es obligatorio.");
                return false;
            }

            // Validar fecha
         

            // Validar hora (asegúrate de que el formato es correcto)
           


            return true;
        }

        private void BtnCufe_Click_1(object sender, RoutedEventArgs e)
        {
            if (ValidarDatos())
            {
                // Construir la cadena CUFE y generar el CUFE
                _viewModel.CadenaCUFE = ConstruirCadenaCUFE();
                _viewModel.CUFE = GenerarCUFE(_viewModel.CadenaCUFE);

                // Mostrar la cadena y el CUFE en los TextBoxes (opcional)
                TxtCadenaCUFE.Text = _viewModel.CadenaCUFE;
                TxtCadenaCUFE.Visibility = Visibility.Visible;
                TxtCUFE.Text = "CUFE generado: " + _viewModel.CUFE;
                TxtCUFE.Visibility = Visibility.Visible;
            }

        }

        private string ConstruirCadenaCUFE()
        {
            // Asegúrate de convertir los valores a los formatos correctos y de manejar posibles valores nulos
            string numeroFactura = TxtFactura.Text;
            string fechaFactura = PikerFecha.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
            string horaFactura = TextHora.Text; // formato correcto
            string valorSubtotal = Subtotal.Text;
            string codigo = "01";
            string iva = Iva.Text;
            string codigo2 = "04";
            string impuesto2 = Impuesto2.Text;
            string codigo3 = "03";
            string impuesto3 = Impuesto3.Text;
            string total = Total.Text;
            string nitFacturador = NITFacturador.Text;
            string numeroIdentificacionCliente = NumeroIdentificacion.Text;
            string clavetecnica = Clave.Text;
            int tipodeambiente = 2;
            //  string tipoIdentificacionCliente = TipoDocumento.list;
            // Concatenar los valores en el orden correcto
            string cadenaCUFE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{clavetecnica}{tipodeambiente}"; 
            return cadenaCUFE;
        }

        private string GenerarCUFE(string cadenaCUFE)
        {
            using (SHA384 sha384 = SHA384.Create())
            {
                // Convertir la cadena en un array de bytes
                byte[] bytesCadena = Encoding.UTF8.GetBytes(cadenaCUFE);

                // Aplicar SHA-384
                byte[] hashBytes = sha384.ComputeHash(bytesCadena);

                // Convertir el resultado del hash en una cadena hexadecimal en minúsculas
                string cufe = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return cufe;
            }
        }


        private void BtnCufe_Click_2(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as InvoiceViewModel;
            if (viewModel != null)
            {
                // Generar el XML y la versión base64
                (string xmlContent, string base64Content) = viewModel.GenerateXMLAndBase64(viewModel.MyInvoice);

                // Directorio donde se guardarán los archivos
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo directoryInfo = new DirectoryInfo(appDirectory);
                string projectDirectory = directoryInfo.Parent.Parent.Parent.FullName;
                string xmlDirectory = System.IO.Path.Combine(projectDirectory, "xml");

                // Ruta del archivo XML
                string xmlFilePath = System.IO.Path.Combine(xmlDirectory, "archivo.xml");

                // Ruta del archivo base64
                string base64FilePath = System.IO.Path.Combine(xmlDirectory, "base64.txt");

                // Crear el directorio si no existe
                if (!System.IO.Directory.Exists(xmlDirectory))
                {
                    System.IO.Directory.CreateDirectory(xmlDirectory);
                }

                // Guardar el XML en el archivo
                viewModel.SaveXMLToFile(xmlContent, xmlFilePath);

                // Guardar el contenido base64 en el archivo
                File.WriteAllText(base64FilePath, base64Content);

                // Mostrar mensaje de confirmación
                MessageBox.Show("XML generado y guardado en: " + xmlFilePath + "\n" +
                                "Archivo base64 generado y guardado en: " + base64FilePath);
            }
        }






    }
}