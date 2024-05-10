using GeneradorCufe.Conexion;
using GeneradorCufe.Consultas;
using GeneradorCufe.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            try
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
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }
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
                //   MessageBox.Show("Solicitud POST exitosa.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Realizar una solicitud GET para consultar el XML después de la solicitud POST exitosa
                    ConsultarXML(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);

                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                // Manejar cualquier error de la solicitud POST
            //    MessageBox.Show($"Error al enviar la solicitud POST:\n\n{ex.Message}", "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
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
                         //   MessageBox.Show($"Error al enviar la solicitud POST. Código de estado: {statusCode}\nMensaje de error: {errorResponse}", "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);

                            // Guardar el error en la base de datos
                            respuestaConsulta.GuardarErrorEnBD(cadenaConexion, statusCode, errorResponse, factura);
                            Factura_Consulta facturaConsulta = new Factura_Consulta();
                            facturaConsulta.MarcarComoConError(factura, webEx);

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


        public static async Task ConsultarXML(Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado)
        {
            try
            {
                await Task.Delay(4000);

                string nitCompleto = emisor.Nit_emisor ?? "";
                string[] partesNit = nitCompleto.Split('-');
                string Nit = partesNit.Length > 0 ? partesNit[0] : "";
                string url = "";
                string token = "";
                string partnershipId = "900770401";
                string nitEmisor = Nit;
                string idDocumento;
                string codigoTipoDocumento;
                string PrefijoNC = "";
                string recibo = "";
                bool Nota_credito;

                if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://apivp.efacturacadena.com/v1/vp/consulta/documentos";
                    token = "RtFGzoqD-5dab-BVQl-qHaQ-ICPjsQnP4Q1K";
                }
                else
                {
                    url = "https://apivp.efacturacadena.com/staging/vp/consulta/documentos";
                    token = "RNimIzV6-emyM-sQ2b-mclA-S9DWbc84jKCV";
                }

                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                    idDocumento = PrefijoNC;
                    codigoTipoDocumento = "91";
                    recibo = factura.Recibo;
                    Nota_credito = true;

                }
                else
                {
                    idDocumento = factura.Facturas;
                    recibo = factura.Facturas;
                    codigoTipoDocumento = "01";
                    Nota_credito = false;
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
                    client.DefaultRequestHeaders.Add("efacturaAuthorizationToken", token);

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

                                Dictionary<int, EstadoProcesamiento> registroProcesando;

                                using (StreamReader reader = new StreamReader("registro_procesando.json"))
                                {
                                    string json = reader.ReadToEnd();
                                    registroProcesando = JsonConvert.DeserializeObject<Dictionary<int, EstadoProcesamiento>>(json);
                                }
                                int idEncabezado = factura.Id_encabezado.Value;
                                bool correoEnviado = false;

                                // Comprobar si se ha enviado un correo electrónico para esta factura
                                if (registroProcesando.ContainsKey(idEncabezado) && registroProcesando[idEncabezado].Envio == 0)
                                {
                                    // Enviar el archivo ZIP por correo electrónico
                                    correoEnviado = await EnviarCorreo.Enviar(emisor, adquiriente, factura, zipStream.ToArray(), nombreArchivoXML);

                                    if (correoEnviado)
                                    {
                                        // Actualizar el estado de "Envio" en el diccionario
                                        registroProcesando[idEncabezado].Envio = 1;

                                        // Guardar el diccionario actualizado en el archivo temporal
                                        string jsonOutput = JsonConvert.SerializeObject(registroProcesando);
                                        File.WriteAllText("registro_procesando.json", jsonOutput);
                                    }
                                }
                                else
                                {
                                    // Ya se ha enviado un correo electrónico para esta factura, no es necesario enviar otro.
                                    correoEnviado = true;
                                }

                                // Realizar otras operaciones solo si se envió correctamente el correo electrónico
                                if (correoEnviado)
                                {
                                    // Crear una instancia de la clase Respuesta_Consulta
                                    Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());

                                    // Guardar la respuesta en la base de datos
                                    bool respuestaGuardada = respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, documentBase64, recibo, cufe, idDocumento, Nota_credito, factura);

                                    // Verificar si la respuesta se guardó correctamente en la base de datos
                                    if (respuestaGuardada)
                                    {
                                        // Borrar la respuesta de la base de datos solo si se guardó correctamente
                                        respuestaConsulta.BorrarEnBD(cadenaConexion, idDocumento, recibo, Nota_credito, factura);
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        // Mostrar un mensaje de error si la solicitud no fue exitosa
                      //  MessageBox.Show($"Error al enviar la solicitud GET. Código de estado: {response.StatusCode}", "Error de Solicitud GET", MessageBoxButton.OK, MessageBoxImage.Error);
                        Factura_Consulta facturaConsulta = new Factura_Consulta();
                        facturaConsulta.ManejarIntentos(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado, response);
                    }

                }
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
            }
        }


        public static (string xmlContent, string base64Content, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) GenerateXMLAndBase64(Emisor emisor, Factura factura)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string xmlTemplatePath = Path.Combine(Directory.GetParent(basePath).Parent.Parent.Parent.FullName, "Plantilla", "XML.xml");

            string cadenaConexion = Data.ConstruirCadenaConexion(factura);
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
                xmlTemplatePath = Path.Combine(Directory.GetParent(basePath).Parent.Parent.Parent.FullName, "Plantilla_NC", "NC.xml");
                xmlDoc = XDocument.Load(xmlTemplatePath);

                // Llamar a la acción para generar nota de crédito y asignar sus valores de retorno a cadenaConexion y cufe
                (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorNC.GeneradorNotaCredito(xmlDoc, emisor, factura, cadenaConexion);
            }
            else
            {
                // Si no se cumple, usar la plantilla normal
                xmlTemplatePath = xmlTemplatePath;

                try
                {
                    // Intenta cargar el documento XML base
                    xmlDoc = XDocument.Load(xmlTemplatePath);

                    // Actualizar el documento XML con los datos dinámicos
                    (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorFE.UpdateXmlWithViewModelData(xmlDoc, emisor, factura, cadenaConexion);
                }
                catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
                {
                    Factura_Consulta facturaConsulta = new Factura_Consulta();
                    facturaConsulta.MarcarComoConError(factura, ex);

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


}
