﻿using GeneradorCufe.Consultas;
using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

    public class InvoiceViewModel
    {

        public static void EjecutarGeneracionXML(Emisor emisor, Factura factura)
        {

            // Generar el XML y la versión base64 sin pasar MyInvoice
            (string xmlContent, string base64Content) = GenerateXMLAndBase64(emisor, factura);

            // Verificar que el contenido XML no esté vacío antes de continuar
            if (string.IsNullOrEmpty(xmlContent))
            {
                MessageBox.Show("La generación del XML falló. Por favor, verifique que la plantilla XML exista y sea válida.", "Error de Generación XML", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Detiene la ejecución adicional si no se generó el XML
            }

            // Directorio donde se guardarán los archivos
            string xmlDirectory = System.IO.Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "xml");

            // Asegurarte de que el directorio 'xml' existe
            if (!Directory.Exists(xmlDirectory))
            {
                Directory.CreateDirectory(xmlDirectory);
            }

            // Generar el nombre del archivo ZIP usando la fecha y hora actual
            string zipFileName = $"Archivos_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.zip";
            string zipFilePath = System.IO.Path.Combine(xmlDirectory, zipFileName);

            // Crear un archivo ZIP y agregar los archivos XML y base64
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                // Agregar el archivo XML
                var xmlEntry = zipArchive.CreateEntry("archivo.xml");
                using (var writer = new StreamWriter(xmlEntry.Open()))
                {
                    writer.Write(xmlContent);
                }

                // Agregar el archivo base64
                var base64Entry = zipArchive.CreateEntry("base64.txt");
                using (var writer = new StreamWriter(base64Entry.Open()))
                {
                    writer.Write(base64Content);
                }
            }

            // Mostrar mensaje de confirmación
            MessageBox.Show($"Archivos generados y guardados en: {zipFilePath}");

            // Realizar la solicitud POST
            string url = "https://apivp.efacturacadena.com/staging/vp/documentos/proceso/alianzas";
            string response = SendPostRequest(url, base64Content);
        }

        private static string SendPostRequest(string url, string base64Content)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Establecer el encabezado efacturaAuthorizationToken
                    client.Headers["efacturaAuthorizationToken"] = "RNimIzV6-emyM-sQ2b-mclA-S9DWbc84jKCV";

                    // Convertir el contenido base64 en bytes
                    byte[] bytes = Encoding.UTF8.GetBytes(base64Content);

                    // Realizar la solicitud POST y obtener la respuesta
                    byte[] responseBytes = client.UploadData(url, "POST", bytes);

                    // Convertir la respuesta a string
                    string response = Encoding.UTF8.GetString(responseBytes);

                    // Mostrar un mensaje de éxito
                    MessageBox.Show("Solicitud POST exitosa", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    return response;
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)webEx.Response).StatusCode;
                    using (var stream = webEx.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string errorResponse = reader.ReadToEnd();
                            MessageBox.Show($"Error al enviar la solicitud POST. Código de estado: {statusCode}\nMensaje de error: {errorResponse}", "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                            return "";
                        }
                    }
                }
                else
                {
                    // Manejar cualquier otro error de la solicitud POST
                    MessageBox.Show("Error al enviar la solicitud POST:\n\n" + webEx.Message, "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }



        public static void UpdateXmlWithViewModelData(XDocument xmlDoc, Emisor emisor, Factura factura)
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            FacturaElectronica facturaElectronica = new FacturaElectronica(); // Crear una instancia de la clase FacturaElectronica
            List<InvoiceLineData> listaProductos = facturaElectronica.ObtenerProductos();

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
            string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

            string ciudadCompleta = emisor.Nombre_municipio_emisor ?? "";
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)



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
                partyNameElement.SetValue(emisor.Nombre_emisor);
            }

            // Actualizar detalles de 'PhysicalLocation'
            var physicalLocationElement = xmlDoc.Descendants(cac + "PhysicalLocation").FirstOrDefault();
            if (physicalLocationElement != null)
            {
                physicalLocationElement.Descendants(cbc + "ID").FirstOrDefault()?.SetValue("05380");
                physicalLocationElement.Descendants(cbc + "CityName").FirstOrDefault()?.SetValue(Municipio);
                physicalLocationElement.Descendants(cbc + "PostalZone").FirstOrDefault()?.SetValue("055460");
                physicalLocationElement.Descendants(cbc + "CountrySubentity").FirstOrDefault()?.SetValue(Departamento);
                physicalLocationElement.Descendants(cbc + "CountrySubentityCode").FirstOrDefault()?.SetValue("05");
                physicalLocationElement.Descendants(cac + "AddressLine")
                                       .Descendants(cbc + "Line").FirstOrDefault()?.SetValue(emisor.Direccion_emisor);
            }

            // Actualizaciones para 'PartyTaxScheme'
            var partyTaxSchemeElement = xmlDoc.Descendants(cac + "PartyTaxScheme").FirstOrDefault();
            if (partyTaxSchemeElement != null)
            {
                partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue(emisor.Nombre_emisor);

                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetValue(Nit);
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeID", Dv);
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeName", "31");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyID", "195");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");

                partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("R-99-PN");

                var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                if (registrationAddressElement != null)
                {
                    registrationAddressElement.Element(cbc + "ID")?.SetValue("05380");
                    registrationAddressElement.Element(cbc + "CityName")?.SetValue(Municipio);
                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("055460");
                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue(Departamento);
                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue("05");
                    registrationAddressElement.Element(cac + "AddressLine").Element(cbc + "Line")?.SetValue(emisor.Direccion_emisor);

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
                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue(emisor.Nombre_emisor);
                // Establecer el valor de CompanyID y sus atributos si existe
                var companyIDElement = partyLegalEntityElement.Element(cbc + "CompanyID");
                if (companyIDElement != null)
                {
                    companyIDElement.SetValue(Nit);
                    companyIDElement.SetAttributeValue("schemeID", Dv);
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

        private static void MapInvoiceLine(XDocument xmlDoc) // Informacion de los productor o servicios
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

        private static void MapAccountingCustomerParty(XDocument xmlDoc) // Información del adquiriente 
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


        public static void UpdateXmlNotaCreditoWithViewModelData(XDocument xmlDoc, Emisor emisor)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Actualizar 'CustomizationID'
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("");

            // Actualizar 'ProfileExecutionID'
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("");

            // Actualizar 'ID'
            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue("");

            // Actualizar 'UUID'
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue("");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeID", "2");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeName", "CUDE-SHA384");

            // Actualizar 'IssueDate' y 'IssueTime'
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue("");
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue("");

            // Actualizar 'CreditNoteTypeCode'
            xmlDoc.Descendants(cbc + "CreditNoteTypeCode").FirstOrDefault()?.SetValue("");

            // Actualizar 'Note'
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("");

            // Actualizar 'DocumentCurrencyCode'
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");

            // Actualizar 'LineCountNumeric'
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue("");

            // Actualizar 'DiscrepancyResponse'
            var discrepancyResponseElement = xmlDoc.Descendants(cac + "DiscrepancyResponse").FirstOrDefault();
            if (discrepancyResponseElement != null)
            {
                discrepancyResponseElement.Element(cbc + "ReferenceID")?.SetValue("");
                discrepancyResponseElement.Element(cbc + "ResponseCode")?.SetValue("");
                discrepancyResponseElement.Element(cbc + "Description")?.SetValue("");
            }
        }

        public static (string xmlContent, string base64Content) GenerateXMLAndBase64(Emisor emisor, Factura factura)
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
                UpdateXmlWithViewModelData(xmlDoc, emisor, factura);
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
