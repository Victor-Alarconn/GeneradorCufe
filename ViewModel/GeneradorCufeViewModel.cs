﻿using GeneradorCufe.Consultas;
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

        public static async Task EjecutarGeneracionXML(Emisor emisor, Factura factura)
        {
            // Generar el XML y la versión base64
            (string xmlContent, string base64Content, string cadenaConexion, string cufe) = GenerateXMLAndBase64(emisor, factura);

            // Verificar que el contenido XML no esté vacío antes de continuar
            if (string.IsNullOrEmpty(xmlContent))
            {
                MessageBox.Show("La generación del XML falló. Por favor, verifique que la plantilla XML exista y sea válida.", "Error de Generación XML", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Detiene la ejecución adicional si no se generó el XML
            }


            // Directorio donde se guardarán los archivos
            string xmlDirectory = System.IO.Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "xml");

            if (string.IsNullOrEmpty(xmlDirectory))
            {
                MessageBox.Show("Error al obtener el directorio para guardar los archivos XML.", "Error de Directorio", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Asegurarte de que el directorio 'xml' existe
            if (!Directory.Exists(xmlDirectory))
            {
                Directory.CreateDirectory(xmlDirectory);
            }

            // Generar el nombre del archivo ZIP
            string zipFileName;
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
            {
                zipFileName = $"NC_{factura.Recibo}.zip";
            }
            else
            {
                zipFileName = $"Archivos_{factura.Facturas}.zip";
            }
            string zipFilePath = Path.Combine(xmlDirectory, zipFileName);

            // Verificar si el archivo ZIP ya existe y eliminarlo si es necesario
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

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


            // Mostrar mensaje informativo de éxito
            //var successMessage = $"La generación de archivos XML se ha completado con éxito. Los archivos se han guardado en: {zipFilePath}";
            //await Task.Delay(3000);
            //MessageBox.Show(successMessage);

            // Realizar la solicitud POST y esperar la tarea
            string url = "https://apivp.efacturacadena.com/staging/vp/documentos/proceso/alianzas";
            string response = await SendPostRequest(url, base64Content, emisor, factura, cadenaConexion, cufe);
        }

        

        private static async Task<string> SendPostRequest(string url, string base64Content, Emisor emisor, Factura factura, string cadenaConexion, string cufe)
        {
            // Crear una instancia de la clase Respuesta_Consulta
            Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());
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

                    // Mostrar un mensaje de éxito con el código de estado
                    MessageBox.Show("Solicitud POST exitosa.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Realizar una solicitud GET para consultar el XML después de la solicitud POST exitosa
                    ConsultarXML(emisor, factura, cadenaConexion, cufe);

                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                // Manejar cualquier error de la solicitud POST
                MessageBox.Show($"Error al enviar la solicitud POST:\n\n{ex.Message}", "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
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

                            // Guardar el error en la base de datos
                            respuestaConsulta.GuardarErrorEnBD(cadenaConexion, statusCode, errorResponse, factura);

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


        private static async Task ConsultarXML(Emisor emisor, Factura factura, string cadenaConexion, string cufe)
        {
            try
            {
                string nit = emisor.Nit_emisor?.Replace("-0", "");
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

                        // Decodificar el documento base64 a un array de bytes (XML)
                        byte[] xmlBytes = Convert.FromBase64String(documentBase64);

                        // Guardar el archivo XML en un flujo de memoria
                        using (MemoryStream xmlStream = new MemoryStream(xmlBytes))
                        {
                    
                            int añoActual = DateTime.Now.Year;
                            // Construir el nombre del archivo PDF
                            string nombreArchivoPDF = $"fv{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{factura.Facturas:D8}.pdf";

                            // Construir el nombre del archivo XML
                            string nombreArchivoXML = $"ad{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{factura.Facturas:D8}.xml";

                            // Crear el PDF
                            string directorioProyecto = Directory.GetCurrentDirectory();
                            string rutaArchivoPDF = Path.Combine(directorioProyecto, nombreArchivoPDF);
                            GeneradorPDF.CrearPDF(rutaArchivoPDF, emisor, factura);

                            // Comprimir el PDF y el XML en un archivo ZIP en un flujo de memoria
                            using (MemoryStream zipStream = new MemoryStream())
                            {
                                using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                                {
                                    zip.CreateEntryFromFile(rutaArchivoPDF, nombreArchivoPDF);

                                    // Agregar el XML al archivo ZIP desde el flujo de memoria
                                    var entry = zip.CreateEntry(nombreArchivoXML, CompressionLevel.Fastest);
                                    using (Stream entryStream = entry.Open())
                                    {
                                        xmlStream.Seek(0, SeekOrigin.Begin); // Reiniciar el flujo de memoria del XML
                                        await xmlStream.CopyToAsync(entryStream);
                                    }
                                }

                                // Enviar el archivo ZIP por correo electrónico
                                EnviarCorreo.Enviar("soporte3.rmsoft@gmail.com", "soporte3.rmsoft@gmail.com", "Asunto del correo", "Cuerpo del mensaje", zipStream.ToArray(), nombreArchivoPDF);
                            }


                        }

                        // Crear una instancia de la clase Respuesta_Consulta
                        Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());
                        respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, documentBase64, idDocumento);
                        respuestaConsulta.BorrarEnBD(cadenaConexion, idDocumento);
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


        public static (string cadenaConexion, string CUFE) UpdateXmlWithViewModelData(XDocument xmlDoc, Emisor emisor, Factura factura)
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
            FormaPago_Consulta formaPagoConsulta = new FormaPago_Consulta();

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
            List<FormaPago> listaFormaPago = formaPagoConsulta.ConsultarFormaPago(factura, cadenaConexion);
            // Obtener la hora en formato DateTimeOffset
            DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(movimiento.Hora_dig, "HH:mm:ss", CultureInfo.InvariantCulture);

            // Agregar el desplazamiento horario
            string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
            string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

            string construir = GeneradorCufe_Cude.ConstruirCadenaCUFE(movimiento, listaProductos, factura, horaformateada, Nit);
            string CUFE = GenerarCUFE(construir);


            string nota = $"Factura de Venta Emitida por {nitCompleto}-{emisor.Nombre_emisor}";
            

            xmlDoc.Descendants(sts + "InvoiceAuthorization").FirstOrDefault()?.SetValue(encabezado.Autorizando);
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
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(movimiento.Fecha_Factura.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(horaformateada);
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01"); // Código de tipo de factura (01 para factura de venta)
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue(nota);
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");
            // Verificar si movimiento.Numero_bolsa tiene un valor diferente de 0
            if (movimiento.Numero_bolsa != 0)
            {
                // Sumar 1 al conteo de productos si hay un valor en movimiento.Numero_bolsa
                xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count + 1);
            }
            else
            {
                // Establecer el conteo de productos sin sumar 1 si movimiento.Numero_bolsa es 0
                xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count);
            }


            string ciudadCompleta = emisor.Nombre_municipio_emisor ?? "";
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)
            Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta); // Consulta para obtener los códigos de ciudad y departamento

            GenerarEmisor.MapearInformacionEmisor(xmlDoc, emisor, encabezado, codigos);  // Información del emisor


            string nitValue = listaProductos[0].Nit;
            GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion); // Información del adquiriente


            // Información del medio de pago
            var paymentMeansElement = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();
            if (paymentMeansElement != null)
            {
                paymentMeansElement.Element(cbc + "ID")?.SetValue("1");
                paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
            }
            //var paymentMeansElementParent = xmlDoc.Descendants(cac + "Invoice").FirstOrDefault()?.Element(cac + "PaymentMeans");

            //// Crear una plantilla de PaymentMeans
            //var paymentMeansTemplate = new XElement(cac + "PaymentMeans",
            //    new XElement(cbc + "ID", "1")); // ID fijo

            //// Agregar la plantilla al XML antes del bucle foreach
            //paymentMeansElementParent?.Add(paymentMeansTemplate);

            //if (listaFormaPago != null && listaFormaPago.Count > 0)
            //{
            //    foreach (var formaPago in listaFormaPago)
            //    {
            //        // Clonar la plantilla para cada forma de pago
            //        var paymentMeansElement = new XElement(paymentMeansTemplate);

            //        // Asignar PaymentMeansCode y PaymentID según el Id_forma de la forma de pago
            //        if (formaPago.Id_forma == "00")
            //        {
            //            paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
            //            paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
            //        }
            //        else if (formaPago.Id_forma == "01")
            //        {
            //            paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("49");
            //            paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Tarjeta Débito");
            //        }
            //        else if (formaPago.Id_forma == "99")
            //        {
            //            paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("48");
            //            paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Tarjeta Crédito");
            //            paymentMeansElement.Element(cbc + "PaymentDueDate")?.SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
            //        }

            //        // Agregar PaymentMeans al XML
            //        paymentMeansElementParent?.Add(paymentMeansElement);
            //    }
            //}

            //// Eliminar la plantilla del XML después del bucle foreach
            //paymentMeansTemplate?.Remove();


            GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento); // Calcular el total del IVA de todos los productos


            decimal retiene = movimiento.Retiene;

            // Obtener el elemento WithholdingTaxTotal si existe
            var withholdingTaxTotalElement = xmlDoc.Descendants(cac + "WithholdingTaxTotal").FirstOrDefault();

            // Verificar si retiene es igual a 0.00 y si existe el elemento WithholdingTaxTotal
            if (retiene == 0.00m && withholdingTaxTotalElement != null)
            {
                // Eliminar el elemento WithholdingTaxTotal si retiene es igual a 0.00
                withholdingTaxTotalElement.Remove();
            }
            else if (retiene != 0.00m)
            {
                // Reemplazar los valores del elemento WithholdingTaxTotal si retiene es diferente de 0.00
                withholdingTaxTotalElement?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxableAmount")?.SetValue(movimiento.Valor_neto);
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cbc + "Percent")?.SetValue("3.50");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "ID")?.SetValue("06");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "Name")?.SetValue("ReteFuente");

                // Si el elemento WithholdingTaxTotal no existe, crear uno nuevo
                if (withholdingTaxTotalElement == null)
                {
                    withholdingTaxTotalElement = new XElement(cac + "WithholdingTaxTotal",
                        new XElement(cbc + "TaxAmount", retiene.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount", "3361344.00"),
                            new XElement(cbc + "TaxAmount", "84033.60"),
                            new XElement(cac + "TaxCategory",
                                new XElement(cbc + "Percent", "3.50"),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "06"),
                                    new XElement(cbc + "Name", "ReteFuente")
                                )
                            )
                        )
                    );

                    // Agregar el elemento WithholdingTaxTotal al XML
                    xmlDoc.Root?.Add(withholdingTaxTotalElement);
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

            if(factura.Recibo != "0" && !string.IsNullOrEmpty(factura.Recibo) )
            {
                
            }
            
            GenerarProductos.MapInvoiceLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine

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

            // Retornar la cadena de conexión
            return (cadenaConexion, CUFE);
        }





        public static (string xmlContent, string base64Content, string cadenaConexion, string cufe) GenerateXMLAndBase64(Emisor emisor, Factura factura)
        {
            // Define la ruta al archivo XML base
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string baseXmlFilePath = Path.Combine(basePath, "Plantilla", "XML.xml");
            string xmlTemplatePath = ""; // Declaración fuera del bloque if

            string cadenaConexion = "";
            string cufe = "";

            XDocument xmlDoc; // Declarar xmlDoc fuera del bloque try

            // Verificar la condición para determinar la plantilla y la acción a utilizar
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
            {
                // Si se cumple la condición, usar la plantilla de nota de crédito
                xmlTemplatePath = Path.Combine(basePath, "Plantilla_NC", "NC.xml");
                xmlDoc = XDocument.Load(xmlTemplatePath); 

                // Llamar a la acción para generar nota de crédito y asignar sus valores de retorno a cadenaConexion y cufe
                (cadenaConexion, cufe) = GeneradorNC.GeneradorNotaCredito(xmlDoc, emisor, factura);
            }
            else
            {
                // Si no se cumple, usar la plantilla normal
                xmlTemplatePath = baseXmlFilePath;

                try
                {
                    // Intenta cargar el documento XML base
                    xmlDoc = XDocument.Load(xmlTemplatePath);

                    // Actualizar el documento XML con los datos dinámicos
                    (cadenaConexion, cufe) = UpdateXmlWithViewModelData(xmlDoc, emisor, factura);
                }
                catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
                {
                    // Muestra un diálogo de error si la plantilla XML no se puede cargar
                    MessageBox.Show("Error: La plantilla para generar el XML es incorrecta o nula.", "Error de Plantilla XML", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Devuelve una tupla vacía o con valores predeterminados para evitar más errores
                    return (string.Empty, string.Empty, string.Empty, string.Empty);
                }
            }

            // Convertir el XML actualizado a string
            string xmlContent = xmlDoc.ToString();

            // Convertir el contenido XML en base64
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
            string base64Encoded = Convert.ToBase64String(bytes);

            // Devolver la tupla con todos los valores
            return (xmlContent, base64Encoded, cadenaConexion, cufe);
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
