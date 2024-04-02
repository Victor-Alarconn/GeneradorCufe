using GeneradorCufe.Consultas;
using GeneradorCufe.Model;
using Newtonsoft.Json;
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
using System.Security.Cryptography;
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
            // Generar el XML y la versión base64
            (string xmlContent, string base64Content, string cadenaConexion) = GenerateXMLAndBase64(emisor, factura);

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
            string response = SendPostRequest(url, base64Content, emisor, factura, cadenaConexion);
        }

        private static string SendPostRequest(string url, string base64Content, Emisor emisor, Factura factura, string cadenaConexion)
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

                    // Obtener el código de estado de la respuesta
                    HttpStatusCode statusCode = HttpStatusCode.OK; 

                    // Convertir la respuesta a string
                    string response = Encoding.UTF8.GetString(responseBytes);

                    // Mostrar un mensaje de éxito con el código de estado
                    MessageBox.Show($"Solicitud POST exitosa. Código de estado: {(int)statusCode} - {statusCode}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Realizar una solicitud GET para consultar el XML después de la solicitud POST exitosa
                    ConsultarXML(emisor, factura, cadenaConexion);

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

        private static async Task ConsultarXML(Emisor emisor, Factura factura, string cadenaConexion)
        {
            try
            {
                string nit = emisor.Nit_emisor.Replace("-0", "");
                // Construir la URL completa con los parámetros necesarios
                string url = "https://apivp.efacturacadena.com/staging/vp/consulta/documentos";
                string partnershipId = "900770401";
                string nitEmisor = nit;
                string idDocumento = factura.Facturas;
                string codigoTipoDocumento = "01";

                // Construir los parámetros de la URL
                string parametros = $"?nit_emisor={nitEmisor}&id_documento={idDocumento}&codigo_tipo_documento={codigoTipoDocumento}";

                // Concatenar los parámetros a la URL
                url += parametros;

                // Crear la instancia de HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // Establecer los encabezados de la solicitud
                    client.DefaultRequestHeaders.Add("Partnership-Id", partnershipId);
                    client.DefaultRequestHeaders.Add("efacturaAuthorizationToken", "RNimIzV6-emyM-sQ2b-mclA-S9DWbc84jKCV");

                    // Realizar la solicitud GET y obtener la respuesta
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verificar si la solicitud fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        // Obtener la respuesta en formato JSON
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Convertir la respuesta JSON a un objeto dynamic para acceder a los campos
                        dynamic jsonResponseObject = JsonConvert.DeserializeObject(jsonResponse);

                        // Extraer el valor del parámetro 'document' (documento adjunto en base64)
                        string documentBase64 = jsonResponseObject.document;

                        // Decodificar el documento base64 si es necesario
                        byte[] documentBytes = Convert.FromBase64String(documentBase64);

                        // Guardar el documento en un archivo, por ejemplo
                        string filePath = "documento_adjunto.txt";
                        File.WriteAllBytes(filePath, documentBytes);

                        // Mostrar un mensaje de éxito
                        MessageBox.Show($"Documento adjunto guardado en '{filePath}'", "Consulta XML", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Crear una instancia de la clase Respuesta_Consulta
                        Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());

                        // Llamar al método para guardar la respuesta en la base de datos
                        respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, documentBase64, idDocumento);
                    }
                    else
                    {
                        // Mostrar un mensaje de error si la solicitud no fue exitosa
                        MessageBox.Show($"Error al enviar la solicitud GET. Código de estado: {response.StatusCode}", "Error de Solicitud GET", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier otra excepción
                MessageBox.Show("Error al enviar la solicitud GET:\n\n" + ex.Message, "Error de Solicitud GET", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public static string UpdateXmlWithViewModelData(XDocument xmlDoc, Emisor emisor, Factura factura) // emisor y factura
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Crear una instancia de la clase Productos_Consulta
            Productos_Consulta productosConsulta = new Productos_Consulta();
            Encabezado_Consulta encabezadoConsulta = new Encabezado_Consulta();
            Codigos_Consulta codigosConsulta = new Codigos_Consulta();
            Movimiento_Consulta movimientoConsulta = new Movimiento_Consulta();

            string cadenaConexion = "";

            if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
            {
                cadenaConexion = $"Database={factura.Empresa}; Data Source={factura.Ip_base}; User Id=RmSoft20X;Password=**LiLo89**; ConvertZeroDateTime=True;";
            }
            else if (factura.Ip_base == "192.190.42.191")
            {
                cadenaConexion = $"Database={factura.Empresa}; Data Source={factura.Ip_base}; User Id=root;Password=**qwerty**; ConvertZeroDateTime=True;";
            }
            // Llamar al método ConsultarProductosPorFactura para obtener la lista de productos
            Encabezado encabezado = encabezadoConsulta.ConsultarEncabezado(factura, cadenaConexion);
            Movimiento movimiento = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
            List<Productos> listaProductos = productosConsulta.ConsultarProductosPorFactura(factura, cadenaConexion);

            string construir = ConstruirCadenaCUFE(movimiento, listaProductos, factura);
            string CUFE = GenerarCUFE(construir);

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
            string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

            string ciudadCompleta = emisor.Nombre_municipio_emisor ?? "";
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)
            Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta); // Consulta para obtener los códigos de ciudad y departamento
            string nota = $"Factura de Venta Emitida por {nitCompleto}-{emisor.Nombre_emisor}";


            // Actualizar el elemento 'InvoiceAuthorization'
            xmlDoc.Descendants(sts + "InvoiceAuthorization").FirstOrDefault()?.SetValue(encabezado.Autorizando);

            // Actualizar los elementos 'StartDate' y 'EndDate'
            xmlDoc.Descendants(cbc + "StartDate").FirstOrDefault()?.SetValue(encabezado.Fecha_inicio.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "EndDate").FirstOrDefault()?.SetValue(encabezado.Fecha_termina.ToString("yyyy-MM-dd"));

            // Actualizaciones para 'AuthorizedInvoices'
            var authorizedInvoicesElement = xmlDoc.Descendants(sts + "AuthorizedInvoices").FirstOrDefault();
            if (authorizedInvoicesElement != null)
            {
                authorizedInvoicesElement.Element(sts + "Prefix")?.SetValue(encabezado.Prefijo);
                authorizedInvoicesElement.Element(sts + "From")?.SetValue(encabezado.R_inicio);
                authorizedInvoicesElement.Element(sts + "To")?.SetValue(encabezado.R_termina);
            }
            DateTimeOffset now = DateTimeOffset.Now;
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("10"); // Indicador del tipo de operación (10 para facturación electrónica)
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("2"); // Tipo de ambiente (2 para pruebas)

            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(factura.Facturas);
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(CUFE);
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(now.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue("00:00:00-05:00");
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01"); // Código de tipo de factura (01 para factura de venta)
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue(nota);
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count);

            xmlDoc.Descendants(cbc + "AdditionalAccountID").FirstOrDefault()?.SetValue("1"); // Identificador de tipo de organización jurídica de la de persona

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
                physicalLocationElement.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(codigos.Codigo_Municipio);
                physicalLocationElement.Descendants(cbc + "CityName").FirstOrDefault()?.SetValue(Municipio);
                physicalLocationElement.Descendants(cbc + "PostalZone").FirstOrDefault()?.SetValue("660001");
                physicalLocationElement.Descendants(cbc + "CountrySubentity").FirstOrDefault()?.SetValue(Departamento);
                physicalLocationElement.Descendants(cbc + "CountrySubentityCode").FirstOrDefault()?.SetValue(codigos.Codigo_Departamento);
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
                    registrationAddressElement.Element(cbc + "ID")?.SetValue(codigos.Codigo_Municipio);
                    registrationAddressElement.Element(cbc + "CityName")?.SetValue(Municipio);
                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("660001");
                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue(Departamento);
                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue(codigos.Codigo_Departamento);
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
                    taxSchemeElement.Element(cbc + "ID")?.SetValue("01"); // pendiente
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
                    corporateRegistrationSchemeElement.Element(cbc + "ID")?.SetValue(encabezado.Prefijo);
                }
            }

            // Mapeo para Contact
            var contactElement = xmlDoc.Descendants(cac + "Contact").FirstOrDefault();
            if (contactElement != null)
            {
                contactElement.Element(cbc + "ElectronicMail")?.SetValue("xxxxx@xxxxx.com."); // angee pendiente por correo 
            }

            if (listaProductos != null)
            {
                string nitValue = listaProductos[0].Nit;
                MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion);  // Información del adquiriente
            }
            else
            {
                Console.WriteLine("La lista de productos no tiene suficientes elementos para acceder al tercero.");
            }

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
                taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(movimiento.Valor_iva); // Valor total del impuesto
                //taxTotalElement.Element(cbc + "RoundingAmount")?.SetValue("2.000");

                // Información del subtotal del impuesto
                var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                if (taxSubtotalElement != null)
                {
                    taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue(movimiento.Valor_neto); // Base imponible gravada
                    taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue(movimiento.Valor_iva); // Valor del impuesto

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
                legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Bruto antes de tributos
                legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Base Imponible
                legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(movimiento.Valor); // Total Valor Bruto más tributos
                legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(movimiento.Valor); // Total Valor a Pagar // cufe ValTot
            }

            // Llamada a la función para mapear la información de InvoiceLine
            MapInvoiceLine(xmlDoc, listaProductos);

            // Retornar la cadena de conexión
            return cadenaConexion;
        }


        private static void MapInvoiceLine(XDocument xmlDoc, List<Productos> listaProductos) // Productos
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Obtener el elemento 'cac:InvoiceLine' para utilizarlo como plantilla para agregar nuevos productos
            var invoiceLineTemplate = xmlDoc.Descendants(cac + "InvoiceLine").FirstOrDefault();

            // Verificar si la plantilla existe
            if (invoiceLineTemplate != null)
            {
                // Iterar sobre cada producto en la lista y agregarlos al XML
                for (int i = 0; i < listaProductos.Count; i++)
                {
                    var producto = listaProductos[i];
                    // Crear un nuevo elemento 'cac:InvoiceLine' basado en la plantilla
                    var invoiceLineElement = new XElement(invoiceLineTemplate);

                    // Establecer los valores del producto en el nuevo elemento
                    string cantidadformateada = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                    invoiceLineElement.Element(cbc + "ID")?.SetValue((i + 1).ToString());
                    invoiceLineElement.Element(cbc + "InvoicedQuantity")?.SetValue(cantidadformateada);
                    invoiceLineElement.Element(cbc + "LineExtensionAmount")?.SetValue(producto.Neto); // cufe  ValFac

                    // Establecer los valores del impuesto
                    var taxTotalElement = invoiceLineElement.Element(cac + "TaxTotal");
                    if (taxTotalElement != null)
                    {
                        taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.IvaTotal); // cufe ValImp1

                        var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                        if (taxSubtotalElement != null)
                        {
                            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue(producto.Neto);
                            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.IvaTotal);

                            var taxCategoryElement = taxSubtotalElement.Element(cac + "TaxCategory");
                            if (taxCategoryElement != null)
                            {
                                // Formatear el valor del IVA con dos decimales
                                string formattedIva = producto.Iva.ToString("F2", CultureInfo.InvariantCulture);

                                // Establecer el valor formateado en el elemento Percent
                                taxCategoryElement.Element(cbc + "Percent")?.SetValue(formattedIva);

                                // Agregar la parte faltante para TaxScheme dentro de TaxCategory
                                var taxSchemeElement = taxCategoryElement.Element(cac + "TaxScheme");
                                if (taxSchemeElement == null)
                                {
                                    // Si no existe el elemento TaxScheme, lo creamos
                                    taxSchemeElement = new XElement(cac + "TaxScheme");
                                    taxCategoryElement.Add(taxSchemeElement);
                                }

                                // Establecer los valores de ID y Name dentro de TaxScheme
                                taxSchemeElement.Element(cbc + "ID")?.SetValue("01"); // yo pendiente
                                taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA"); // yo
                            }

                        }
                    }

                    // Establecer los valores del ítem
                    var itemElement = invoiceLineElement.Element(cac + "Item");
                    if (itemElement != null)
                    {
                        itemElement.Element(cbc + "Description")?.SetValue(producto.Detalle);

                        var standardItemIdentificationElement = itemElement.Element(cac + "StandardItemIdentification");
                        if (standardItemIdentificationElement != null)
                        {
                            standardItemIdentificationElement.Element(cbc + "ID")?.SetValue(producto.Codigo);
                        }
                    }

                    var priceElement = invoiceLineElement.Element(cac + "Price");
                    if (priceElement != null)
                    {
                        // Formatear el precio con dos decimales utilizando CultureInfo.InvariantCulture
                        string formattedPrice = producto.Valor.ToString("F2", CultureInfo.InvariantCulture);
                        // Asignar el precio formateado al elemento XML
                        priceElement.Element(cbc + "PriceAmount")?.SetValue(formattedPrice);

                        // Formatear la cantidad con dos decimales utilizando CultureInfo.InvariantCulture
                        string formattedQuantity = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                        // Asignar la cantidad formateada al elemento XML
                        priceElement.Element(cbc + "BaseQuantity")?.SetValue(formattedQuantity);
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
                    partnershipElement.Element(invoiceNs + "TechKey")?.SetValue("fc8eac422eba16e22ffd8c6f94b3f40a6e38162c"); // pregunta 
                    partnershipElement.Element(invoiceNs + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5"); // pregunta 
                }
            }
        }

        private static void MapAccountingCustomerParty(XDocument xmlDoc, string Nit, string cadenaConexion) // Información del adquiriente 
        { // esperelo aqui
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            Adquiriente_Consulta adquirienteConsulta = new Adquiriente_Consulta();
            Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente(Nit, cadenaConexion);

            // Información del adquiriente
            var accountingCustomerPartyElement = xmlDoc.Descendants(cac + "AccountingCustomerParty").FirstOrDefault();
            if (accountingCustomerPartyElement != null)
            {
                accountingCustomerPartyElement.Element(cbc + "AdditionalAccountID")?.SetValue("2"); // Identificador de tipo de adquiriente  jurídica de la de persona

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
                            idElement.Value = adquiriente.Nit_adqui;
                            idElement.SetAttributeValue("schemeName", "13"); // cambio de 31 a 13
                        }


                        if (partyElement != null)
                        {
                            partyElement.Element(cac + "PartyName")?.Element(cbc + "Name")?.SetValue(adquiriente.Nombre_adqu);

                            // Información de ubicación física del adquiriente
                            var physicalLocationElement = partyElement.Element(cac + "PhysicalLocation");
                            if (physicalLocationElement != null)
                            {
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "ID")?.SetValue(adquiriente.Codigo_municipio_adqui);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CityName")?.SetValue(adquiriente.Nombre_municipio_adqui);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "PostalZone")?.SetValue("660001");
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentity")?.SetValue(adquiriente.Nombre_departamento_adqui);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentityCode")?.SetValue(adquiriente.Codigo_departamento_adqui);
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue(adquiriente.Direccion_adqui);
                            }

                            // Información tributaria del adquiriente
                            var partyTaxSchemeElement = partyElement.Element(cac + "PartyTaxScheme");
                            if (partyTaxSchemeElement != null)
                            {
                                // Establecer el nombre de registro
                                partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue(adquiriente.Nombre_municipio_adqui);

                                // Establecer el ID de la compañía
                                var companyIDElement = partyTaxSchemeElement.Element(cbc + "CompanyID");
                                if (companyIDElement != null)
                                {
                                    companyIDElement.SetValue(adquiriente.Nit_adqui);
                                    companyIDElement.SetAttributeValue("schemeID", "");
                                    companyIDElement.SetAttributeValue("schemeName", "13");
                                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                                }

                                // Establecer el código de nivel de impuestos
                                partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("R-99-PN");

                                // Información de ubicación de registro tributario del adquiriente
                                var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                                if (registrationAddressElement != null)
                                {
                                    registrationAddressElement.Element(cbc + "ID")?.SetValue(adquiriente.Codigo_municipio_adqui);
                                    registrationAddressElement.Element(cbc + "CityName")?.SetValue(adquiriente.Nombre_municipio_adqui);
                                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("660001");
                                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue(adquiriente.Nombre_departamento_adqui);
                                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue(adquiriente.Codigo_departamento_adqui);
                                    registrationAddressElement.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                    registrationAddressElement.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                                    registrationAddressElement.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue(adquiriente.Direccion_adqui);
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
                                // Establecer el nombre de registro
                                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue(adquiriente.Nombre_adqu);

                                // Establecer el ID de la compañía
                                var companyIDElement = partyLegalEntityElement.Element(cbc + "CompanyID");
                                if (companyIDElement != null)
                                {
                                    companyIDElement.SetValue(adquiriente.Nit_adqui);
                                    companyIDElement.SetAttributeValue("schemeID", "");
                                    companyIDElement.SetAttributeValue("schemeName", "13");
                                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                                }
                            }


                            // Información de contacto del adquiriente
                            var contactElement = partyElement.Element(cac + "Contact");
                            if (contactElement != null)
                            {
                                contactElement.Element(cbc + "ElectronicMail")?.SetValue(adquiriente.Correo_adqui);
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


        public static (string xmlContent, string base64Content, string cadenaConexion) GenerateXMLAndBase64(Emisor emisor, Factura factura)
        {
            // Define la ruta al archivo XML base
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string baseXmlFilePath = Path.Combine(basePath, "Plantilla", "XML.xml");
            XDocument xmlDoc;
            string cadenaConexion = ""; // Declarar la variable fuera del bloque try

            try
            {
                // Intenta cargar el documento XML base
                xmlDoc = XDocument.Load(baseXmlFilePath);

                // Actualizar el documento XML con los datos dinámicos
                cadenaConexion = UpdateXmlWithViewModelData(xmlDoc, emisor, factura);
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                // Muestra un diálogo de error si la plantilla XML no se puede cargar
                MessageBox.Show("Error: La plantilla para generar el XML es incorrecta o nula.", "Error de Plantilla XML", MessageBoxButton.OK, MessageBoxImage.Error);

                // Devuelve una tupla vacía o con valores predeterminados para evitar más errores
                return (string.Empty, string.Empty, string.Empty);
            }

            // Convertir el XML actualizado a string
            string xmlContent = xmlDoc.ToString();

            // Convertir el contenido XML en base64
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
            string base64Encoded = Convert.ToBase64String(bytes);

            return (xmlContent, base64Encoded, cadenaConexion);
        }

        private static string ConstruirCadenaCUFE(Movimiento movimiento, List<Productos> listaProductos, Factura factura)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            // Asegúrate de convertir los valores a los formatos correctos y de manejar posibles valores nulos
            string numeroFactura = factura.Facturas;
            string fechaFactura = "2024-04-02";
            string horaFactura = "00:00:00-05:00";
            decimal valorSubtotal = movimiento.Valor_neto;
            string codigo = "01";
            decimal iva = movimiento.Valor_iva;
            string codigo2 = "04";
            string impuesto2 = "0.00";
            string codigo3 = "03";
            string impuesto3 = "0.00";
            //  string codigo4 = "06";
            //  string impuesto4 = "0.00";
            decimal total = movimiento.Valor;
            string nitFacturador = "1004994836";
            string numeroIdentificacionCliente = movimiento.Nit;
            string clavetecnica = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c";
            int tipodeambiente = 2;

            // Construir la cadena CUFE
            string cadenaCUFE = $"{numeroFactura}{fechaFactura}{horaFactura}{valorSubtotal}{codigo}{iva}{codigo2}{impuesto2}{codigo3}{impuesto3}{total}{nitFacturador}{numeroIdentificacionCliente}{clavetecnica}{tipodeambiente}";

            // Reemplazar comas por puntos en la cadena CUFE
            cadenaCUFE = cadenaCUFE.Replace(',', '.');

            return cadenaCUFE;
        }

        private static string GenerarCUFE(string cadenaCUFE)
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
