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
using System.Windows;
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
            get { return _setTestId; }
            set { _setTestId = value; OnPropertyChanged(nameof(SetTestId)); }
        }

        private string _autorizacion;
        public string Autorizacion
        {
            get { return _autorizacion; }
            set
            {
                _autorizacion = value;
                OnPropertyChanged(nameof(Autorizacion));
            }
        }



        private DateTime _fechaInicio;
        public DateTime FechaInicio
        {
            get { return _fechaInicio; }
            set { _fechaInicio = value; OnPropertyChanged(); }
        }

        private DateTime _fechaFin;
        public DateTime FechaFin
        {
            get { return _fechaFin; }
            set { _fechaFin = value; OnPropertyChanged(); }
        }

        private string _tipoOperacion;
        public string TipoOperacion
        {
            get { return _tipoOperacion; }
            set
            {
                _tipoOperacion = value;
                OnPropertyChanged(nameof(TipoOperacion));
            }
        }

        private string _prefijo;
        public string Prefijo
        {
            get { return _prefijo; }
            set
            {
                _prefijo = value;
                OnPropertyChanged(nameof(Prefijo));
            }
        }

        private string _rangoInicial;
        public string RangoInicial
        {
            get { return _rangoInicial; }
            set
            {
                _rangoInicial = value;
                OnPropertyChanged(nameof(RangoInicial));
            }
        }

        private string _rangoFinal;
        public string RangoFinal
        {
            get { return _rangoFinal; }
            set
            {
                _rangoFinal = value;
                OnPropertyChanged(nameof(RangoFinal));
            }
        }


        private string _ambiente;
        public string Ambiente
        {
            get { return _ambiente; }
            set
            {
                _ambiente = value;
                OnPropertyChanged(nameof(Ambiente));
            }
        }

        private string _tipoFactura;
        public string TipoFactura
        {
            get { return _tipoFactura; }
            set
            {
                _tipoFactura = value;
                OnPropertyChanged(nameof(TipoFactura));
            }
        }

        private string _infoAdicional;
        public string InfoAdicional
        {
            get { return _infoAdicional; }
            set
            {
                _infoAdicional = value;
                OnPropertyChanged(nameof(InfoAdicional));
            }
        }

        private string _divisa;
        public string Divisa
        {
            get { return _divisa; }
            set
            {
                _divisa = value;
                OnPropertyChanged(nameof(Divisa));
            }
        }

        private string _tipo_Organizacion;
        public string Tipo_Organizacion
        {
            get { return _tipo_Organizacion; }
            set
            {
                _tipo_Organizacion = value;
                OnPropertyChanged(nameof(Tipo_Organizacion));
            }
        }

        private string _nombreEmisor;
        public string NombreEmisor
        {
            get { return _nombreEmisor; }
            set
            {
                _nombreEmisor = value;
                OnPropertyChanged(nameof(NombreEmisor));
            }
        }


        private string _codigo_municipio_emisor;
        public string Codigo_municipio_emisor
        {
            get { return _codigo_municipio_emisor; }
            set
            {
                _codigo_municipio_emisor = value;
                OnPropertyChanged(nameof(Codigo_municipio_emisor));
            }
        }

        private string _nombre_ciudad_emisor;
        public string Nombre_ciudad_emisor
        {
            get { return _nombre_ciudad_emisor; }
            set
            {
                _nombre_ciudad_emisor = value;
                OnPropertyChanged(nameof(Nombre_ciudad_emisor));
            }
        }

        private string _codigo_Postal_emisor;
        public string Codigo_Postal_emisor
        {
            get { return _codigo_Postal_emisor; }
            set
            {
                _codigo_Postal_emisor = value;
                OnPropertyChanged(nameof(Codigo_Postal_emisor));
            }
        }

        private string _nombre_Departamento_emisor;
        public string Nombre_Departamento_emisor
        {
            get { return _nombre_Departamento_emisor; }
            set
            {
                _nombre_Departamento_emisor = value;
                OnPropertyChanged(nameof(Nombre_Departamento_emisor));
            }
        }

        private string _codigo_departamento_emisor;
        public string Codigo_departamento_emisor
        {
            get { return _codigo_departamento_emisor; }
            set
            {
                _codigo_departamento_emisor = value;
                OnPropertyChanged(nameof(Codigo_departamento_emisor));
            }
        }

        private string _direccion_emisor;
        public string Direccion_emisor
        {
            get { return _direccion_emisor; }
            set
            {
                _direccion_emisor = value;
                OnPropertyChanged(nameof(Direccion_emisor));
            }
        }

        private string _razon_social_emisor;
        public string Razon_social_emisor
        {
            get { return _razon_social_emisor; }
            set
            {
                _razon_social_emisor = value;
                OnPropertyChanged(nameof(Razon_social_emisor));
            }
        }

        private string _dv_emisor;
        public string Dv_emisor
        {
            get { return _dv_emisor; }
            set
            {
                _dv_emisor = value;
                OnPropertyChanged(nameof(Dv_emisor));
            }
        }

        private string _nit_identificacion_emisor;
        public string Nit_identificacion_emisor
        {
            get { return _nit_identificacion_emisor; }
            set
            {
                _nit_identificacion_emisor = value;
                OnPropertyChanged(nameof(Nit_identificacion_emisor));
            }
        }

        private string _regimen_emisor;
        public string Regimen_emisor
        {
            get { return _regimen_emisor; }
            set
            {
                _regimen_emisor = value;
                OnPropertyChanged(nameof(Regimen_emisor));
            }
        }

        private string _atributo_emisor;
        public string Atributo_emisor
        {
            get { return _atributo_emisor; }
            set
            {
                _atributo_emisor = value;
                OnPropertyChanged(nameof(Atributo_emisor));
            }
        }

        private string _nombre_atributo_emisor;
        public string Nombre_atributo_emisor
        {
            get { return _nombre_atributo_emisor; }
            set
            {
                _nombre_atributo_emisor = value;
                OnPropertyChanged(nameof(Nombre_atributo_emisor));
            }
        }

        private string _prefijo_facturacion;
        public string Prefijo_facturacion
        {
            get { return _prefijo_facturacion; }
            set
            {
                _prefijo_facturacion = value;
                OnPropertyChanged(nameof(Prefijo_facturacion));
            }
        }


        private string _matricula_mercantil;
        public string Matricula_mercantil
        {
            get { return _matricula_mercantil; }
            set
            {
                _matricula_mercantil = value;
                OnPropertyChanged(nameof(Matricula_mercantil));
            }
        }

        private string _correo_emisor;
        public string Correo_emisor
        {
            get { return _correo_emisor; }
            set
            {
                _correo_emisor = value;
                OnPropertyChanged(nameof(Correo_emisor));
            }
        }

        private string _tipo_persona;
        public string Tipo_persona
        {
            get { return _tipo_persona; }
            set
            {
                _tipo_persona = value;
                OnPropertyChanged(nameof(Tipo_persona));
            }
        }

        private string _documento_cliente;
        public string Documento_cliente
        {
            get { return _documento_cliente; }
            set
            {
                _documento_cliente = value;
                OnPropertyChanged(nameof(Documento_cliente));
            }
        }

        private string _documento_Adquiriente;
        public string Documento_Adquiriente
        {
            get { return _documento_Adquiriente; }
            set
            {
                _documento_Adquiriente = value;
                OnPropertyChanged(nameof(Documento_Adquiriente));
            }
        }

        private string _numero_documento;
        public string Numero_documento
        {
            get { return _numero_documento; }
            set
            {
                _numero_documento = value;
                OnPropertyChanged(nameof(Numero_documento));
            }
        }

        private string _nombre_cliente;
        public string Nombre_cliente
        {
            get { return _nombre_cliente; }
            set
            {
                _nombre_cliente = value;
                OnPropertyChanged(nameof(Nombre_cliente));
            }
        }

        private string _codigo_municipio_adquiriente;
        public string Codigo_municipio_adquiriente
        {
            get { return _codigo_municipio_adquiriente; }
            set
            {
                _codigo_municipio_adquiriente = value;
                OnPropertyChanged(nameof(Codigo_municipio_adquiriente));
            }
        }


        private string _nombre_ciudad_adquiriente;
        public string Nombre_ciudad_adquiriente
        {
            get { return _nombre_ciudad_adquiriente; }
            set
            {
                _nombre_ciudad_adquiriente = value;
                OnPropertyChanged(nameof(Nombre_ciudad_adquiriente));
            }
        }

        private string _codigo_Postal_adquiriente;
        public string Codigo_Postal_adquiriente
        {
            get { return _codigo_Postal_adquiriente; }
            set
            {
                _codigo_Postal_adquiriente = value;
                OnPropertyChanged(nameof(Codigo_Postal_adquiriente));
            }
        }

        private string _nombre_Departamento_adquiriente;
        public string Nombre_Departamento_adquiriente
        {
            get { return _nombre_Departamento_adquiriente; }
            set
            {
                _nombre_Departamento_adquiriente = value;
                OnPropertyChanged(nameof(Nombre_Departamento_adquiriente));
            }
        }

        private string _codigo_departamento_adquiriente;
        public string Codigo_departamento_adquiriente
        {
            get { return _codigo_departamento_adquiriente; }
            set
            {
                _codigo_departamento_adquiriente = value;
                OnPropertyChanged(nameof(Codigo_departamento_adquiriente));
            }
        }

        private string _direccion_adquiriente;
        public string Direccion_adquiriente
        {
            get { return _direccion_adquiriente; }
            set
            {
                _direccion_adquiriente = value;
                OnPropertyChanged(nameof(Direccion_adquiriente));
            }
        }

        private string _razon_social_adquiriente;
        public string Razon_social_adquiriente
        {
            get { return _razon_social_adquiriente; }
            set
            {
                _razon_social_adquiriente = value;
                OnPropertyChanged(nameof(Razon_social_adquiriente));
            }
        }

        private string _dv_adquiriente;
        public string Dv_adquiriente
        {
            get { return _dv_adquiriente; }
            set
            {
                _dv_adquiriente = value;
                OnPropertyChanged(nameof(Dv_adquiriente));
            }
        }


        private string _nit_identificacion_adquiriente;
        public string Nit_identificacion_adquiriente
        {
            get { return _nit_identificacion_adquiriente; }
            set
            {
                _nit_identificacion_adquiriente = value;
                OnPropertyChanged(nameof(Nit_identificacion_adquiriente));
            }
        }

        private string _regimen_adquiriente;
        public string Regimen_adquiriente
        {
            get { return _regimen_adquiriente; }
            set
            {
                _regimen_adquiriente = value;
                OnPropertyChanged(nameof(Regimen_adquiriente));
            }
        }

        private string _atributo_adquiriente;
        public string Atributo_adquiriente
        {
            get { return _atributo_adquiriente; }
            set
            {
                _atributo_adquiriente = value;
                OnPropertyChanged(nameof(Atributo_adquiriente));
            }
        }

        private string _nombre_atributo_adquiriente;
        public string Nombre_atributo_adquiriente
        {
            get { return _nombre_atributo_adquiriente; }
            set
            {
                _nombre_atributo_adquiriente = value;
                OnPropertyChanged(nameof(Nombre_atributo_adquiriente));
            }
        }

        private string _correo_adquiriente;
        public string Correo_adquiriente
        {
            get { return _correo_adquiriente; }
            set
            {
                _correo_adquiriente = value;
                OnPropertyChanged(nameof(Correo_adquiriente));
            }
        }

        private string _metodo_pago;
        public string Metodo_pago
        {
            get { return _metodo_pago; }
            set
            {
                _metodo_pago = value;
                OnPropertyChanged(nameof(Metodo_pago));
            }
        }

        private string _codigo_metodo;
        public string Codigo_metodo
        {
            get { return _codigo_metodo; }
            set
            {
                _codigo_metodo = value;
                OnPropertyChanged(nameof(Codigo_metodo));
            }
        }

        private string _tipo_documento;
        public string Tipo_documento
        {
            get { return _tipo_documento; }
            set
            {
                _tipo_documento = value;
                OnPropertyChanged(nameof(Tipo_documento));
            }
        }



        private void UpdateXmlWithViewModelData(XDocument xmlDoc)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";


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
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("10");

            // Actualizar 'ProfileExecutionID'
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("2");

            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(this.NumeroFactura);
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(this.CUFE);
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(now.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(now.ToString("HH:mm:sszzz"));
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01");
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("Prueba Factura Electronica Datos de victor");
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue("1");

            // Actualizar 'AdditionalAccountID'
            xmlDoc.Descendants(cbc + "AdditionalAccountID").FirstOrDefault()?.SetValue("1");

            // Actualizar 'PartyName'
            var partyNameElement = xmlDoc.Descendants(cac + "Party")
                                         .Descendants(cac + "PartyName")
                                         .Descendants(cbc + "Name").FirstOrDefault();
            if (partyNameElement != null)
            {
                partyNameElement.SetValue("RM SOFT CASA DE SOFTWARE S.A.S");
            }

            // Actualizar detalles de 'PhysicalLocation'
            var physicalLocationElement = xmlDoc.Descendants(cac + "PhysicalLocation").FirstOrDefault();
            if (physicalLocationElement != null)
            {
                physicalLocationElement.Descendants(cbc + "ID").FirstOrDefault()?.SetValue("05380");
                physicalLocationElement.Descendants(cbc + "CityName").FirstOrDefault()?.SetValue("LA ESTRELLA");
                physicalLocationElement.Descendants(cbc + "PostalZone").FirstOrDefault()?.SetValue("055460");
                physicalLocationElement.Descendants(cbc + "CountrySubentity").FirstOrDefault()?.SetValue("Antioquia");
                physicalLocationElement.Descendants(cbc + "CountrySubentityCode").FirstOrDefault()?.SetValue("05");
                physicalLocationElement.Descendants(cac + "AddressLine")
                                       .Descendants(cbc + "Line").FirstOrDefault()?.SetValue("Cra. 50 #97a Sur-180 a 97a Sur-394");
            }
        }



        public (string xmlContent, string base64Content) GenerateXMLAndBase64()
        {
            // Define la ruta al archivo XML base
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string baseXmlFilePath = Path.Combine(basePath, "Plantilla", "XML.xml");
            XDocument xmlDoc;

            try
            {
                // Intenta cargar el documento XML base
                xmlDoc = XDocument.Load(baseXmlFilePath);

                // Actualizar el documento XML con los datos dinámicos
                UpdateXmlWithViewModelData(xmlDoc);
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                // Muestra un diálogo de error si la plantilla XML no se puede cargar
                MessageBox.Show("Error: La plantilla para generar el XML es incorrecta o nula.", "Error de Plantilla XML", MessageBoxButton.OK, MessageBoxImage.Error);

                // Devuelve una tupla vacía o con valores predeterminados para evitar más errores
                return (string.Empty, string.Empty);
            }

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
