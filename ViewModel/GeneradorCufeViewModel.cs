using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static GeneradorCufe.Model.GeneradorCufeModel;

namespace GeneradorCufe.ViewModel
{

    public class InvoiceViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _numeroFactura;
        public string NumeroFactura
        {
            get { return _numeroFactura; }
            set { _numeroFactura = value; OnPropertyChanged(); }
        }

        private DateTime _fechaFactura;
        public DateTime FechaFactura
        {
            get { return _fechaFactura; }
            set { _fechaFactura = value; OnPropertyChanged(); }
        }

        // Nuevas propiedades
        private string _valorSubtotal;
        public string ValorSubtotal
        {
            get { return _valorSubtotal; }
            set { _valorSubtotal = value; OnPropertyChanged(); }
        }

        private string _horaGeneracion;
        public string HoraGeneracion
        {
            get { return _horaGeneracion; }
            set { _horaGeneracion = value; OnPropertyChanged(); }
        }

        private string _valorIVA;
        public string ValorIVA
        {
            get { return _valorIVA; }
            set { _valorIVA = value; OnPropertyChanged(); }
        }

        private string _valorImpuesto2;
        public string ValorImpuesto2
        {
            get { return _valorImpuesto2; }
            set { _valorImpuesto2 = value; OnPropertyChanged(); }
        }

        private string _valorImpuesto3;
        public string ValorImpuesto3
        {
            get { return _valorImpuesto3; }
            set { _valorImpuesto3 = value; OnPropertyChanged(); }
        }

        private string _totalPagar;
        public string TotalPagar
        {
            get { return _totalPagar; }
            set { _totalPagar = value; OnPropertyChanged(); }
        }

        private string _nitFacturadorElectronico;
        public string NITFacturadorElectronico
        {
            get { return _nitFacturadorElectronico; }
            set { _nitFacturadorElectronico = value; OnPropertyChanged(); }
        }

        private string _numeroIdentificacionCliente;
        public string NumeroIdentificacionCliente
        {
            get { return _numeroIdentificacionCliente; }
            set { _numeroIdentificacionCliente = value; OnPropertyChanged(); }
        }

        private string _claveTecnicaControl;
        public string ClaveTecnicaControl
        {
            get { return _claveTecnicaControl; }
            set { _claveTecnicaControl = value; OnPropertyChanged(); }
        }

        private string _cadenaCUFE;
        public string CadenaCUFE
        {
            get { return _cadenaCUFE; }
            set { _cadenaCUFE = value; OnPropertyChanged(nameof(CadenaCUFE)); }
        }

        private string _cufe;
        public string CUFE
        {
            get { return _cufe; }
            set { _cufe = value; OnPropertyChanged(nameof(CUFE)); }
            
        }

        private string _setTestId;
        public string SetTestId
        {
            get { return _setTestId;}
            set { _setTestId = value; OnPropertyChanged(nameof(SetTestId)); }
        }


        private void UpdateXmlWithViewModelData(XDocument xmlDoc)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            // Actualizar el elemento 'InvoiceAuthorization'
            xmlDoc.Descendants(sts + "InvoiceAuthorization").FirstOrDefault()?.SetValue(this.NumeroFactura);

            // Actualizar los elementos 'StartDate' y 'EndDate'
            xmlDoc.Descendants(cbc + "StartDate").FirstOrDefault()?.SetValue(this.FechaFactura.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "EndDate").FirstOrDefault()?.SetValue(this.FechaFactura.ToString("yyyy-MM-dd"));

            // Actualizaciones para 'AuthorizedInvoices'
            var authorizedInvoicesElement = xmlDoc.Descendants(sts + "AuthorizedInvoices").FirstOrDefault();
            if (authorizedInvoicesElement != null)
            {
                authorizedInvoicesElement.Element(sts + "Prefix")?.SetValue("SETT");
                authorizedInvoicesElement.Element(sts + "From")?.SetValue("1");
                authorizedInvoicesElement.Element(sts + "To")?.SetValue("5000000");
            }
            DateTimeOffset now = DateTimeOffset.Now;
            // Actualizar 'CustomizationID'
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("10"); // Asume que 'this.CustomizationID' existe en tu ViewModel

            // Actualizar 'ProfileExecutionID'
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("2"); // Asume que 'this.ProfileExecutionID' existe en tu ViewModel

            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(this.NumeroFactura);
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(this.CUFE); // Asegúrate de que el valor UUID incluya los atributos schemeID y schemeName si son necesarios
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(now.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(now.ToString("HH:mm:sszzz")); // Asume que IssueTime es un DateTime o TimeSpan
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01");
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("Prueba Factura Electronica Datos de victor");
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue("1");
        }


        public (string xmlContent, string base64Content) GenerateXMLAndBase64()
        {
            // Define la ruta al archivo XML base
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string baseXmlFilePath = Path.Combine(basePath, "Plantilla", "XML.xml");

            // Cargar el documento XML base
            XDocument xmlDoc = XDocument.Load(baseXmlFilePath);

            // Actualizar el documento XML con los datos dinámicos
            UpdateXmlWithViewModelData(xmlDoc);

            // Convertir el XML actualizado a string
            string xmlContent = xmlDoc.ToString();

            // Convertir el contenido XML en base64
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
            string base64Encoded = Convert.ToBase64String(bytes);

            return (xmlContent, base64Encoded);
        }

    }

    public static class DataSerializer
    {
        private const string FilePath = "data.txt";

        public static void SaveData(string data)
        {
            // Escribe los datos en el archivo de texto
            File.WriteAllText(FilePath, data);
        }

        public static string LoadData()
        {
            if (!File.Exists(FilePath))
            {
                // Si el archivo no existe, devuelve una cadena vacía
                return "";
            }

            // Lee los datos desde el archivo de texto
            return File.ReadAllText(FilePath);
        }
    }


}
