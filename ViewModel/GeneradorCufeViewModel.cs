using GeneradorCufe.Consultas;
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


        private void UpdateXmlWithViewModelData(XDocument xmlDoc)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            FacturaElectronica facturaElectronica = new FacturaElectronica(); // Crear una instancia de la clase FacturaElectronica
            List<InvoiceLineData> listaProductos = facturaElectronica.ObtenerProductos();

            // Actualizar el elemento 'InvoiceAuthorization'
            xmlDoc.Descendants(sts + "InvoiceAuthorization").FirstOrDefault()?.SetValue("18760000001");

            // Actualizar los elementos 'StartDate' y 'EndDate'
            xmlDoc.Descendants(cbc + "StartDate").FirstOrDefault()?.SetValue("2019-01-19");
            xmlDoc.Descendants(cbc + "EndDate").FirstOrDefault()?.SetValue("2030-01-19");


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

            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue("SETT74");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue("04f450bc11eaea9181f71e30fb81db4eacf9828455cdffae168b333eb00a65d9b8ab66053fbccfa07c61dfc0914c3ff0");
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(now.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue("00:00:00-05:00");
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01");
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("Prueba Factura Electronica Datos de victor");
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count);

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
                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue("RM SOFT CASA DE SOFTWARE S.A.S");
                // Establecer el valor de CompanyID y sus atributos si existe
                var companyIDElement = partyLegalEntityElement.Element(cbc + "CompanyID");
                if (companyIDElement != null)
                {
                    companyIDElement.SetValue("900770401");
                    companyIDElement.SetAttributeValue("schemeID", "8");
                    companyIDElement.SetAttributeValue("schemeName", "31");
                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                }

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
                taxTotalElement.Element(cbc + "RoundingAmount")?.SetValue("0.000");

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

        private void MapInvoiceLine(XDocument xmlDoc) // Informacion de los productor o servicios
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            FacturaElectronica facturaElectronica = new FacturaElectronica(); // Crear una instancia de la clase FacturaElectronica
            List<InvoiceLineData> listaProductos = facturaElectronica.ObtenerProductos();

            // Obtener el elemento 'cac:InvoiceLine' para utilizarlo como plantilla para agregar nuevos productos
            var invoiceLineTemplate = xmlDoc.Descendants(cac + "InvoiceLine").FirstOrDefault();

            // Verificar si la plantilla existe
            if (invoiceLineTemplate != null)
            {


                // Iterar sobre cada producto en la lista y agregarlos al XML
                foreach (var producto in listaProductos)
                {
                    // Crear un nuevo elemento 'cac:InvoiceLine' basado en la plantilla
                    var invoiceLineElement = new XElement(invoiceLineTemplate);

                    // Establecer los valores del producto en el nuevo elemento
                    invoiceLineElement.Element(cbc + "ID")?.SetValue(producto.InvoiceLineID);
                    invoiceLineElement.Element(cbc + "InvoicedQuantity")?.SetValue(producto.InvoiceLineInvoicedQuantity);
                    invoiceLineElement.Element(cbc + "LineExtensionAmount")?.SetValue(producto.InvoiceLineLineExtensionAmount);

                    // Establecer los valores del impuesto
                    var taxTotalElement = invoiceLineElement.Element(cac + "TaxTotal");
                    if (taxTotalElement != null)
                    {
                        taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.InvoiceLineTaxAmount);

                        var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                        if (taxSubtotalElement != null)
                        {
                            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue(producto.InvoiceLineTaxableAmount);
                            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.InvoiceLineTaxAmount);

                            var taxCategoryElement = taxSubtotalElement.Element(cac + "TaxCategory");
                            if (taxCategoryElement != null)
                            {
                                taxCategoryElement.Element(cbc + "Percent")?.SetValue(producto.InvoiceLinePercent);

                                // Agregar la parte faltante para TaxScheme dentro de TaxCategory
                                var taxSchemeElement = taxCategoryElement.Element(cac + "TaxScheme");
                                if (taxSchemeElement == null)
                                {
                                    // Si no existe el elemento TaxScheme, lo creamos
                                    taxSchemeElement = new XElement(cac + "TaxScheme");
                                    taxCategoryElement.Add(taxSchemeElement);
                                }

                                // Establecer los valores de ID y Name dentro de TaxScheme
                                taxSchemeElement.Element(cbc + "ID")?.SetValue(producto.TaxSchemeID);
                                taxSchemeElement.Element(cbc + "Name")?.SetValue(producto.TaxSchemeName);
                            }
                        }
                    }

                    // Establecer los valores del ítem
                    var itemElement = invoiceLineElement.Element(cac + "Item");
                    if (itemElement != null)
                    {
                        itemElement.Element(cbc + "Description")?.SetValue(producto.ItemDescription);

                        var standardItemIdentificationElement = itemElement.Element(cac + "StandardItemIdentification");
                        if (standardItemIdentificationElement != null)
                        {
                            standardItemIdentificationElement.Element(cbc + "ID")?.SetValue(producto.ItemID);
                        }
                    }

                    var priceElement = invoiceLineElement.Element(cac + "Price");
                    if (priceElement != null)
                    {
                        priceElement.Element(cbc + "PriceAmount")?.SetValue(producto.PricePriceAmount);
                        priceElement.Element(cbc + "BaseQuantity")?.SetValue(producto.PriceBaseQuantity);
                    }

                    // Agregar el nuevo elemento 'cac:InvoiceLine' al XML
                    xmlDoc.Descendants(cac + "InvoiceLine").LastOrDefault()?.AddAfterSelf(invoiceLineElement);
                }
                invoiceLineTemplate.Remove();
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
                    partnershipElement.Element(invoiceNs + "ID")?.SetValue("900770401");
                    partnershipElement.Element(invoiceNs + "TechKey")?.SetValue("fc8eac422eba16e22ffd8c6f94b3f40a6e38162c");
                    partnershipElement.Element(invoiceNs + "SetTestID")?.SetValue("af771a36-bdac-4fd4-97c7-14d225b3b948");
                }
            }




        }

        private void MapAccountingCustomerParty(XDocument xmlDoc) // Información del adquiriente 
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Consultar la información del adquiriente desde la base de datos
          //  Adquiriente_Consulta adquirienteConsulta = new Adquiriente_Consulta();
          //  Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente();

            // Ejemplo 
          //  accountingCustomerPartyElement.Element(cac + "Party")?.Element(cac + "PartyName")?.Element(cbc + "Name")?.SetValue(adquiriente.Nombre_adqu);

            // Información del adquiriente
            var accountingCustomerPartyElement = xmlDoc.Descendants(cac + "AccountingCustomerParty").FirstOrDefault();
            if (accountingCustomerPartyElement != null)
            {
                accountingCustomerPartyElement.Element(cbc + "AdditionalAccountID")?.SetValue("2");

                // Información del adquiriente
                var partyElement = accountingCustomerPartyElement.Element(cac + "Party");
                if (partyElement != null)
                {
                    var partyIdentificationElement = partyElement.Element(cac + "PartyIdentification");
                    if (partyIdentificationElement != null)
                    {
                        var idElement = partyIdentificationElement.Element(cbc + "ID");
                        if (idElement != null)
                        {
                            idElement.Value = "1017173008";
                            idElement.SetAttributeValue("schemeName", "13");
                        }


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
