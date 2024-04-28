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

        public static async Task EjecutarGeneracionXML(Emisor emisor, Factura factura)
        {
            // Generar el XML y la versión base64
            (string xmlContent, string base64Content, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) = GenerateXMLAndBase64(emisor, factura);

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

            string url;

            // Verificar si emisor.Url_emisor es igual a "docum" sin importar mayúsculas o minúsculas
            if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
            {
                // Si es "docum", utilizar la primera URL
                url = "https://apivp.efacturacadena.com/v1/vp/documentos/proceso/alianzas";
            }
            else
            {
                // Si no es "docum", utilizar la segunda URL
                url = "https://apivp.efacturacadena.com/staging/vp/documentos/proceso/alianzas";
            }
            string response = await SendPostRequest(url, base64Content, emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);
        }

        

        private static async Task<string> SendPostRequest(string url, string base64Content, Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado)
        {
            // Crear una instancia de la clase Respuesta_Consulta
            Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());
            try
            {
               using (WebClient client = new WebClient())
                {
                    if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                    {
                        // Si es "docum", establecer el encabezado efacturaAuthorizationToken para "RtFGzoqD-5dab-BVQl-qHaQ-ICPjsQnP4Q1K"
                        client.Headers["efacturaAuthorizationToken"] = "RtFGzoqD-5dab-BVQl-qHaQ-ICPjsQnP4Q1K";
                    }
                    else
                    {
                        client.Headers["efacturaAuthorizationToken"] = "RNimIzV6-emyM-sQ2b-mclA-S9DWbc84jKCV";
                    }

                    // Convertir el contenido base64 en bytes
                    byte[] bytes = Encoding.UTF8.GetBytes(base64Content);

                    // Realizar la solicitud POST y obtener la respuesta
                    byte[] responseBytes = client.UploadData(url, "POST", bytes);

                    // Convertir la respuesta a string
                    string response = Encoding.UTF8.GetString(responseBytes);

                    // Mostrar un mensaje de éxito con el código de estado
                    MessageBox.Show("Solicitud POST exitosa.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Realizar una solicitud GET para consultar el XML después de la solicitud POST exitosa
                    ConsultarXML(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);

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


        private static async Task ConsultarXML(Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado)
        {
            try
            {
                string nit = emisor.Nit_emisor?.Replace("-0", "");
                string url = "https://apivp.efacturacadena.com/staging/vp/consulta/documentos";
                string partnershipId = "900770401";
                string nitEmisor = nit;
                string idDocumento;
                string codigoTipoDocumento;
                string PrefijoNC = "";
                string recibo = "0";

                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                    idDocumento = PrefijoNC;
                    codigoTipoDocumento = "91";
                    recibo = factura.Recibo;
                }
                else
                {
                    idDocumento = factura.Facturas;
                    codigoTipoDocumento = "01";
                }

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
                            string nombreArchivoPDF;
                            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                            {
                                nombreArchivoPDF = $"nc{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{PrefijoNC:D8}.pdf";
                            }
                            else
                            {
                                nombreArchivoPDF = $"fv{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{factura.Facturas:D8}.pdf";
                            }

                            // Construir el nombre del archivo XML
                            string nombreArchivoXML;
                            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                            {
                                nombreArchivoXML = $"ad{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{PrefijoNC:D8}.xml";
                            }
                            else
                            {
                                nombreArchivoXML = $"ad{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{factura.Facturas:D8}.xml";
                            }


                            // Crear el PDF
                            string directorioProyecto = Directory.GetCurrentDirectory();
                            string rutaArchivoPDF = Path.Combine(directorioProyecto, nombreArchivoPDF);
                            GeneradorPDF.CrearPDF(rutaArchivoPDF, emisor, factura, listaProductos, cufe, adquiriente, movimiento, encabezado);

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
                                bool correoEnviado = await EnviarCorreo.Enviar(emisor, adquiriente, factura, zipStream.ToArray(), nombreArchivoXML);


                                if (correoEnviado)
                                {
                                    // Crear una instancia de la clase Respuesta_Consulta
                                    Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());

                                    // Guardar la respuesta en la base de datos
                                    bool respuestaGuardada = respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, documentBase64, recibo, cufe, idDocumento);

                                    // Verificar si la respuesta se guardó correctamente en la base de datos
                                    if (respuestaGuardada)
                                    {
                                        // Borrar la respuesta de la base de datos solo si se guardó correctamente
                                        respuestaConsulta.BorrarEnBD(cadenaConexion, idDocumento, recibo);
                                    }
                                    else
                                    {
                                        // Si la respuesta no se guardó correctamente, mostrar un mensaje de error o manejar la situación según sea necesario
                                        Console.WriteLine("Error: La respuesta no se guardó correctamente en la base de datos.");
                                    }
                                }
                                else
                                {
                                    // Si el correo no se envió correctamente, puedes mostrar un mensaje de error o manejar la situación según sea necesario
                                    Console.WriteLine("Error: El correo no se envió correctamente.");
                                }

                            }
                        }
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



        public static (string cadenaConexion, string CUFE, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) UpdateXmlWithViewModelData(XDocument xmlDoc, Emisor emisor, Factura factura)
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
            Adquiriente_Consulta adquirienteConsulta = new Adquiriente_Consulta();

            string cadenaConexion = "";

            if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
            {
                cadenaConexion = $"Database={factura.Empresa}; Data Source={factura.Ip_base}; User Id=RmSoft20X;Password=*LiLo89*; ConvertZeroDateTime=True;";
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

            string construir = GeneradorCufe_Cude.ConstruirCadenaCUFE(movimiento, listaProductos, factura, horaformateada, Nit, emisor); // se puede resumir 
            string CUFE = GeneradorCufe_Cude.GenerarCUFE(construir);

           

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
                                                                                          // Obtener el valor del perfil de ejecución del XML
            string perfilEjecucionID = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? "1" : "2";

            // Actualizar el valor del perfil de ejecución en el XML
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue(perfilEjecucionID);


            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(factura.Facturas);
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(CUFE);
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(movimiento.Fecha_Factura.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(horaformateada);
            xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01"); // Código de tipo de factura (01 para factura de venta)
                                                                                          // Crear un nuevo elemento Note para la nota adicional
            var nuevaNotaElement = new XElement(cbc + "Note");

            // Verificar si se cumple la condición para agregar una nota adicional
            if (movimiento.Retiene > 0.00m && emisor.Retiene_emisor == 3)
            {
                // Agregar una nota adicional
                string notaAdicional = " Retencion del " + movimiento.Retiene + " del % 3.5";

                // Establecer el valor de la nota adicional en el nuevo elemento Note
                nuevaNotaElement.SetValue(notaAdicional);

                // Agregar el nuevo elemento Note al XML
                xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.AddAfterSelf(nuevaNotaElement);
            }

            // Establecer la nota original
            
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


            string ciudadCompleta = emisor.Ciudad_emisor ?? ""; // se cambia por el codigo de la ciudad
            string[] partesCiudad = ciudadCompleta.Split(',');
 
            Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta); // Consulta para obtener los códigos de ciudad y departamento

            GenerarEmisor.MapearInformacionEmisor(xmlDoc, emisor, encabezado, codigos, listaProductos);  // Información del emisor

            string nitValue = listaProductos[0].Nit;
            Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente(nitValue, cadenaConexion);
            GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion, adquiriente, codigos); // Información del adquiriente


            // Información del medio de pago
            //var paymentMeansElement = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();
            //if (paymentMeansElement != null)
            //{
            //    paymentMeansElement.Element(cbc + "ID")?.SetValue("1");
            //    paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
            //    paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
            //}
            GenerarFormasPago.GenerarFormaPagos(xmlDoc, listaFormaPago); // Información de las formas de pago
           
            GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento); // Calcular el total del IVA de todos los productos


            decimal retiene = movimiento.Retiene;


            // Obtener el elemento WithholdingTaxTotal si existe
            var withholdingTaxTotalElement = xmlDoc.Descendants(cac + "WithholdingTaxTotal").FirstOrDefault();

            // Verificar si retiene es mayor que 0.00 y si el movimiento.Retiene es igual a 2
            if (retiene > 0.00m && emisor.Retiene_emisor== 2) 
            {
                // Reemplazar los valores del elemento WithholdingTaxTotal si retiene es mayor que 0.00
                withholdingTaxTotalElement?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxableAmount")?.SetValue(movimiento.Valor_neto);
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cbc + "Percent")?.SetValue("2.50");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "ID")?.SetValue("06");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "Name")?.SetValue("ReteFuente");
            }
            else
            {
                // Eliminar el elemento WithholdingTaxTotal si retiene no es mayor que 0.00 o si el movimiento.Retiene no es igual a 2
                withholdingTaxTotalElement?.Remove();
            }


            decimal Valor = 0;

            if (emisor.Retiene_emisor == 2 && movimiento.Retiene != 0)
            {
                Valor = Math.Round(movimiento.Valor + movimiento.Retiene, 2);
            }
            else
            {
                Valor = movimiento.Valor;
            }

            var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "LegalMonetaryTotal").FirstOrDefault();
            if (legalMonetaryTotalElement != null)
            {
                legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Bruto antes de tributos 
                legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Base Imponible
                legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(Valor); // Total Valor Bruto más tributos
                legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(Valor); // Total Valor a Pagar // cufe ValTot
            }

            if(factura.Recibo != "0" && !string.IsNullOrEmpty(factura.Recibo) )
            {
                
            }
            
            GenerarProductos.MapInvoiceLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine

            // Buscar el elemento <DATA> dentro del elemento <Invoice> con el espacio de nombres completo
            XNamespace invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var dataElement = xmlDoc.Descendants(invoiceNs + "DATA").FirstOrDefault();

            // Verificar si se encontró el elemento <DATA>
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

                    // Verificar si emisor.Url_emisor es igual a "docum" sin importar mayúsculas o minúsculas
                    if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                    {
                        // Eliminar la línea que establece el valor de TechKey si emisor.Url_emisor es igual a "docum"
                        partnershipElement.Element(invoiceNs + "SetTestID")?.Remove();
                    }
                    else
                    {
                        partnershipElement.Element(invoiceNs + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5");
                    }

                    
                }
            }


            // Retornar la cadena de conexión
            return (cadenaConexion, CUFE, listaProductos, adquiriente, movimiento, encabezado);
        }





        public static (string xmlContent, string base64Content, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) GenerateXMLAndBase64(Emisor emisor, Factura factura)
        {
            // Define la ruta al archivo XML base
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string baseXmlFilePath = Path.Combine(basePath, "Plantilla", "XML.xml");
            string xmlTemplatePath = ""; // Declaración fuera del bloque if

             string cadenaConexion = "";
            string cufe = "";
            List<Productos> listaProductos = null;
            Adquiriente adquiriente = null;
            Movimiento movimiento = null;
            Encabezado encabezado = null;

            XDocument xmlDoc; // Declarar xmlDoc fuera del bloque try

            // Verificar la condición para determinar la plantilla y la acción a utilizar
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
            {
                // Si se cumple la condición, usar la plantilla de nota de crédito
                xmlTemplatePath = Path.Combine(basePath, "Plantilla_NC", "NC.xml");
                xmlDoc = XDocument.Load(xmlTemplatePath); 

                // Llamar a la acción para generar nota de crédito y asignar sus valores de retorno a cadenaConexion y cufe
                (cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorNC.GeneradorNotaCredito(xmlDoc, emisor, factura);
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
                    (cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado) = UpdateXmlWithViewModelData(xmlDoc, emisor, factura);
                }
                catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
                {
                    // Muestra un diálogo de error si la plantilla XML no se puede cargar
                    MessageBox.Show("Error: La plantilla para generar el XML es incorrecta o nula.", "Error de Plantilla XML", MessageBoxButton.OK, MessageBoxImage.Error);

                    return (string.Empty, string.Empty, string.Empty, string.Empty, new List<Productos>(), null, null, null);

                }
            }

            // Convertir el XML actualizado a string
            string xmlContent = xmlDoc.ToString();

            // Convertir el contenido XML en base64
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
            string base64Encoded = Convert.ToBase64String(bytes);

            // Devolver la tupla con todos los valores
            return (xmlContent, base64Encoded, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);
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
