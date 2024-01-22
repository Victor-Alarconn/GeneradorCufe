﻿using GeneradorCufe.ViewModel;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new InvoiceViewModel();
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
                // Construir la cadena CUFE
                string cadenaCUFE = ConstruirCadenaCUFE();

                // Mostrar la cadena en el TextBox (opcional)
                TxtCadenaCUFE.Text = cadenaCUFE;
                TxtCadenaCUFE.Visibility = Visibility.Visible;

                // Generar el hash CUFE usando SHA-384
                string cufe = GenerarCUFE(cadenaCUFE);

                // Mostrar el CUFE en el TextBox
                TxtCUFE.Text = "CUFE generado: " + cufe;
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
            int codigo = 01;
            string iva = Iva.Text;
            int codigo2 = 04;
            string impuesto2 = Impuesto2.Text;
            int codigo3 = 03;
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

                // Convertir el resultado del hash en una cadena hexadecimal
                string cufe = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                return cufe;
            }
        }

        private void BtnCufe_Click_2(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as InvoiceViewModel;
            if (viewModel != null)
            {
                string xmlContent = viewModel.GenerateXML(viewModel.MyInvoice);

                // Utilizar el espacio de nombres completo para System.IO.Path
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo directoryInfo = new DirectoryInfo(appDirectory);
                string projectDirectory = directoryInfo.Parent.Parent.Parent.FullName; // Ajusta según la estructura de tu proyecto
                string xmlDirectory = System.IO.Path.Combine(projectDirectory, "xml");
                string filePath = System.IO.Path.Combine(xmlDirectory, "archivo.xml");

                // Crear el directorio si no existe
                if (!System.IO.Directory.Exists(xmlDirectory))
                {
                    System.IO.Directory.CreateDirectory(xmlDirectory);
                }

                viewModel.SaveXMLToFile(xmlContent, filePath);

                MessageBox.Show("XML generado y guardado en: " + filePath);
            }
        }





    }
}