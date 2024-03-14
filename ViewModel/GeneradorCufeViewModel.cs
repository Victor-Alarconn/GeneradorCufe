using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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

            // Actualizaciones para 'PartyTaxScheme'
            var partyTaxSchemeElement = xmlDoc.Descendants(cac + "PartyTaxScheme").FirstOrDefault();
            if (partyTaxSchemeElement != null)
            {
                partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue("RM SOFT CASA DE SOFTWARE S.A.S");

                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetValue("900770401");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeID", "8");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeName", "31");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyID", "195");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");

                partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("R-99-PN");

                var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                if (registrationAddressElement != null)
                {
                    registrationAddressElement.Element(cbc + "ID")?.SetValue("05380");
                    registrationAddressElement.Element(cbc + "CityName")?.SetValue("LA ESTRELLA");
                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("055460");
                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue("Antioquia");
                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue("05");
                    registrationAddressElement.Element(cac + "AddressLine").Element(cbc + "Line")?.SetValue("Crarera");

                    var countryElement = registrationAddressElement.Element(cac + "Country");
                    if (countryElement != null)
                    {
                        countryElement.Element(cbc + "IdentificationCode")?.SetValue("CO");
                        var nameElement = countryElement.Element(cbc + "Name");
                        if (nameElement != null)
                        {
                            nameElement.SetValue("Colombia");
                            nameElement.SetAttributeValue("languageID", "es");
                        }
                    }

                }

                var taxSchemeElement = partyTaxSchemeElement.Element(cac + "TaxScheme");
                if (taxSchemeElement != null)
                {
                    taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                    taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                }
            }

            // Mapeo para PartyLegalEntity
            var partyLegalEntityElement = xmlDoc.Descendants(cac + "PartyLegalEntity").FirstOrDefault();
            if (partyLegalEntityElement != null)
            {
                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue("RM SOFT CASA DE");
                partyLegalEntityElement.Element(cbc + "CompanyID")?.SetValue("900770401");

                var corporateRegistrationSchemeElement = partyLegalEntityElement.Element(cac + "CorporateRegistrationScheme");
                if (corporateRegistrationSchemeElement != null)
                {
                    corporateRegistrationSchemeElement.Element(cbc + "ID")?.SetValue("SETT");
                    corporateRegistrationSchemeElement.Element(cbc + "Name")?.SetValue("1485596");
                }
            }

            // Mapeo para Contact
            var contactElement = xmlDoc.Descendants(cac + "Contact").FirstOrDefault();
            if (contactElement != null)
            {
                contactElement.Element(cbc + "ElectronicMail")?.SetValue("xxxxx@xxxxx.com.correo");
            }

            MapAccountingCustomerParty(xmlDoc); // informacion del adquiriente

            // Información del medio de pago
            var paymentMeansElement = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();
            if (paymentMeansElement != null)
            {
                paymentMeansElement.Element(cbc + "ID")?.SetValue("1");
                paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
            }

            // Información total de impuestos
            var taxTotalElement = xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault();
            if (taxTotalElement != null)
            {
                taxTotalElement.Element(cbc + "TaxAmount")?.SetValue("19000.00");

                // Información del subtotal del impuesto
                var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                if (taxSubtotalElement != null)
                {
                    taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue("100000.00"); // Base imponible
                    taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue("19000.00"); // Valor del impuesto

                    // Información de la categoría del impuesto
                    var taxCategoryElement = taxSubtotalElement.Element(cac + "TaxCategory");
                    if (taxCategoryElement != null)
                    {
                        taxCategoryElement.Element(cbc + "Percent")?.SetValue("19.00");

                        // Información del esquema del impuesto
                        var taxSchemeElement = taxCategoryElement.Element(cac + "TaxScheme");
                        if (taxSchemeElement != null)
                        {
                            taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                            taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                        }
                    }
                }
            }

            var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "LegalMonetaryTotal").FirstOrDefault();
            if (legalMonetaryTotalElement != null)
            {
                legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue("100000.00"); // Total Valor Bruto antes de tributos
                legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue("100000.00"); // Total Valor Base Imponible
                legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue("119000.00"); // Total Valor Bruto más tributos
                legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue("119000.00"); // Total Valor a Pagar
            }

            // Llamada a la función para mapear la información de InvoiceLine
            MapInvoiceLine(xmlDoc);


        }

        private void MapInvoiceLine(XDocument xmlDoc)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            var invoiceLineElement = xmlDoc.Descendants(cac + "InvoiceLine").FirstOrDefault();
            if (invoiceLineElement != null)
            {
                invoiceLineElement.Element(cbc + "ID")?.SetValue("1.00");
                invoiceLineElement.Element(cbc + "InvoicedQuantity")?.SetValue("100000.00");
                invoiceLineElement.Element(cbc + "LineExtensionAmount")?.SetValue("19000.00");

                var taxTotalElement = invoiceLineElement.Element(cac + "TaxTotal");
                if (taxTotalElement != null)
                {
                    taxTotalElement.Element(cbc + "TaxAmount")?.SetValue("100000.00");

                    var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                    if (taxSubtotalElement != null)
                    {
                        taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue("1");
                        taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue("19000.00");

                        var taxCategoryElement = taxSubtotalElement.Element(cac + "TaxCategory");
                        if (taxCategoryElement != null)
                        {
                            taxCategoryElement.Element(cbc + "Percent")?.SetValue("19.00");

                            var taxSchemeElement = taxCategoryElement.Element(cac + "TaxScheme");
                            if (taxSchemeElement != null)
                            {
                                taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                                taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                            }
                        }
                    }
                }

                var itemElement = invoiceLineElement.Element(cac + "Item");
                if (itemElement != null)
                {
                    itemElement.Element(cbc + "Description")?.SetValue("Frambuesas");

                    var standardItemIdentificationElement = itemElement.Element(cac + "StandardItemIdentification");
                    if (standardItemIdentificationElement != null)
                    {
                        standardItemIdentificationElement.Element(cbc + "ID")?.SetValue("M123445");
                    }
                }

                var priceElement = invoiceLineElement.Element(cac + "Price");
                if (priceElement != null)
                {
                    priceElement.Element(cbc + "PriceAmount")?.SetValue("100000.00");
                    priceElement.Element(cbc + "BaseQuantity")?.SetValue("1.00");
                }
            }
            // Buscar el elemento <DATA> dentro del elemento <Invoice> con el espacio de nombres completo
            XNamespace invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var dataElement = xmlDoc.Descendants(invoiceNs + "DATA").FirstOrDefault();

            // Verificar si se encontró el elemento <DATA>
            if (dataElement != null)
            {
                // Modificar los elementos dentro de <DATA>
                dataElement.Element(invoiceNs + "UBL21")?.SetValue("true");

                // Buscar el elemento <Partnership> dentro de <DATA> con el espacio de nombres completo
                var partnershipElement = dataElement.Descendants(invoiceNs + "Partnership").FirstOrDefault();
                if (partnershipElement != null)
                {
                    partnershipElement.Element(invoiceNs + "ID")?.SetValue("11111111");
                    partnershipElement.Element(invoiceNs + "TechKey")?.SetValue("fc8eac422eba16e22ffd8c6f94b3f40a6e38162c");
                    partnershipElement.Element(invoiceNs + "SetTestID")?.SetValue("esf162");
                }
            }




        }

        private void MapAccountingCustomerParty(XDocument xmlDoc)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Información del adquiriente
            var accountingCustomerPartyElement = xmlDoc.Descendants(cac + "AccountingCustomerParty").FirstOrDefault();
            if (accountingCustomerPartyElement != null)
            {
                accountingCustomerPartyElement.Element(cbc + "AdditionalAccountID")?.SetValue("2");

                // Información de identificación del adquiriente
                var partyIdentificationElement = accountingCustomerPartyElement.Element(cac + "PartyIdentification");
                if (partyIdentificationElement != null)
                {
                    partyIdentificationElement.Element(cbc + "ID")?.SetValue("1017173008");
                    // Asegúrate de ajustar el valor de schemeName según corresponda
                    partyIdentificationElement.Element(cbc + "ID").SetAttributeValue("schemeName", "13");
                }

                // Información del adquiriente
                var partyElement = accountingCustomerPartyElement.Element(cac + "Party");
                if (partyElement != null)
                {
                    partyElement.Element(cac + "PartyName")?.Element(cbc + "Name")?.SetValue("ADQUIRIENTE DE EJEMPLO");

                    // Información de ubicación física del adquiriente
                    var physicalLocationElement = partyElement.Element(cac + "PhysicalLocation");
                    if (physicalLocationElement != null)
                    {
                        physicalLocationElement.Element(cac + "Address")?.Element(cbc + "ID")?.SetValue("66001");
                        physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CityName")?.SetValue("PEREIRA");
                        physicalLocationElement.Element(cac + "Address")?.Element(cbc + "PostalZone")?.SetValue("54321");
                        physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentity")?.SetValue("Risaralda");
                        physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentityCode")?.SetValue("66");
                        physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                        physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                        physicalLocationElement.Element(cac + "Address")?.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue("CR 9 A N0 99 - 07 OF 802");
                    }

                    // Información tributaria del adquiriente
                    var partyTaxSchemeElement = partyElement.Element(cac + "PartyTaxScheme");
                    if (partyTaxSchemeElement != null)
                    {
                        partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue("ADQUIRIENTE DE VICTOR");
                        partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetValue("1017173008");
                        partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("R-99-PN");

                        // Información de ubicación de registro tributario del adquiriente
                        var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                        if (registrationAddressElement != null)
                        {
                            registrationAddressElement.Element(cbc + "ID")?.SetValue("66001");
                            registrationAddressElement.Element(cbc + "CityName")?.SetValue("PEREIRA");
                            registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("54321");
                            registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue("Risaralda");
                            registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue("66");
                            registrationAddressElement.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                            registrationAddressElement.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                            registrationAddressElement.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue("Riosucio, Caldas");
                        }

                        // Información del esquema tributario del adquiriente
                        var taxSchemeElement = partyTaxSchemeElement.Element(cac + "TaxScheme");
                        if (taxSchemeElement != null)
                        {
                            taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                            taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                        }
                    }

                    // Información legal del adquiriente
                    var partyLegalEntityElement = partyElement.Element(cac + "PartyLegalEntity");
                    if (partyLegalEntityElement != null)
                    {
                        partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue("ADQUIRIENTE DE ANGEE");
                        partyLegalEntityElement.Element(cbc + "CompanyID")?.SetValue("1017173008");
                        partyLegalEntityElement.Element(cac + "CorporateRegistrationScheme")?.Element(cbc + "Name")?.SetValue("1485596");
                    }

                    // Información de contacto del adquiriente
                    var contactElement = partyElement.Element(cac + "Contact");
                    if (contactElement != null)
                    {
                        contactElement.Element(cbc + "ElectronicMail")?.SetValue("xxxx@xxxx.comutp");
                    }
                }
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
