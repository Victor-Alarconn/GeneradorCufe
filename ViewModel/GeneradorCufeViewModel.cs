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
                 //   MessageBox.Show("La generación del XML falló. Por favor, verifique que la plantilla XML exista y sea válida.", "Error de Generación XML", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Detiene la ejecución adicional si no se generó el XML
                }


                // Directorio donde se guardarán los archivos
               // string xmlDirectory = Path.Combine(@"C:\inetpub\xml", "xml");
                string xmlDirectory = System.IO.Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "xml");


                // Asegurarte de que el directorio 'xml' existe
                if (!Directory.Exists(xmlDirectory))
                {
                    Directory.CreateDirectory(xmlDirectory);
                }

                // Generar el nombre del archivo ZIP
                string zipFileName;
                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento != "ND")
                {
                    zipFileName = $"NC_{factura.Recibo}.zip";
                }
                else if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
                {
                    zipFileName = $"ND_{factura.Recibo}.zip";
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
                Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());
                respuestaConsulta.GuardarCufe(cufe, factura);
                string response = await SendPostRequest(url, base64Content, emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
              //  MessageBox.Show($"Error1: {ex.Message}", "Error:1", MessageBoxButton.OK, MessageBoxImage.Error);
                facturaConsulta.MarcarComoConError(factura, ex);
            }
        }



        private static async Task<string> SendPostRequest(string url, string base64Content, Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado)
        {
            // Crear una instancia de la clase Respuesta_Consulta
            Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data());

            try
            {

                if (factura.Estado == 6)
                {
                    // Guardar la respuesta en la base de datos y realizar la consulta del XML sin enviar la solicitud POST
                    respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, cufe, factura, emisor);
                    ConsultarXML(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);

                    return "No se envió la solicitud POST porque el estado del registro es 6.";
                }

                using (WebClient client = new WebClient())
                {
                    client.Headers["efacturaAuthorizationToken"] = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase)
                        ? "RtFGzoqD-5dab-BVQl-qHaQ-ICPjsQnP4Q1K"
                        : "RNimIzV6-emyM-sQ2b-mclA-S9DWbc84jKCV";

                    // Convertir el contenido base64 en bytes y realizar la solicitud POST
                    byte[] responseBytes = client.UploadData(url, "POST", Encoding.UTF8.GetBytes(base64Content));
                    string response = Encoding.UTF8.GetString(responseBytes);

                    // Guardar la respuesta en la base de datos y realizar la consulta del XML
                    respuestaConsulta.GuardarRespuestaEnBD(cadenaConexion, cufe, factura, emisor);
                    ConsultarXML(emisor, factura, cadenaConexion, cufe, listaProductos, adquiriente, movimiento, encabezado);

                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                ManejarErrorSolicitud(ex, factura, "Error de Solicitud POST", "Error al enviar la solicitud POST");
                return "";
            }
            catch (WebException webEx)
            {
                return ManejarErrorWebException(webEx, respuestaConsulta, cadenaConexion, factura);
            }
        }

        private static void ManejarErrorSolicitud(Exception ex, Factura factura, string titulo, string mensaje)
        {
         //   MessageBox.Show($"{mensaje}:\n\n{ex.Message}", titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            new Factura_Consulta().MarcarComoConError(factura, ex);
        }

        private static string ManejarErrorWebException(WebException webEx, Respuesta_Consulta respuestaConsulta, string cadenaConexion, Factura factura)
        {
            if (webEx.Response != null)
            {
                HttpStatusCode statusCode = ((HttpWebResponse)webEx.Response).StatusCode;
                using (var stream = webEx.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string errorResponse = reader.ReadToEnd();
                 //   MessageBox.Show($"Error al enviar la solicitud POST. Código de estado: {statusCode}\nMensaje de error: {errorResponse}", "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                    respuestaConsulta.GuardarErrorEnBD(cadenaConexion, statusCode, errorResponse, factura);
                    new Factura_Consulta().MarcarComoConError(factura, webEx);
                    return "";
                }
            }
            else
            {
               // MessageBox.Show("Error al enviar la solicitud POST:\n\n" + webEx.Message, "Error de Solicitud POST", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }



        public static async Task ConsultarXML(Emisor emisor, Factura factura, string cadenaConexion, string cufe, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado)
        {
            try
            {
                await Task.Delay(5000);

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

                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "NC")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                    idDocumento = PrefijoNC;
                    codigoTipoDocumento = "91";
                    recibo = factura.Recibo;
                    Nota_credito = true;

                }
                else if(!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
                {
                    PrefijoNC = "ND" + factura.Recibo;
                    idDocumento = PrefijoNC;
                    codigoTipoDocumento = "92";
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
                            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "NC")
                            {
                                nombreArchivoPDF = $"nc{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{PrefijoNC:D8}.pdf";
                            }
                            else if(!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
                            {
                                nombreArchivoPDF = $"nd{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{PrefijoNC:D8}.pdf";
                            }
                            else
                            {
                                nombreArchivoPDF = $"fv{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{factura.Facturas:D8}.pdf";
                            }

                            // Construir el nombre del archivo XML
                            string nombreArchivoXML;
                            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "NC")
                            {
                                nombreArchivoXML = $"ad{nitEmisor.TrimStart('0')}{añoActual.ToString().Substring(2)}000{PrefijoNC:D8}.xml";
                            }
                            else if(!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
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

                                    Respuesta_Consulta respuestaConsulta = new Respuesta_Consulta(new Conexion.Data()); 

                                      // Borrar la respuesta de la base de datos solo si se guardó correctamente
                                   //   respuestaConsulta.BorrarEnBD(cadenaConexion, idDocumento, recibo, Nota_credito, factura);
                                    
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
             string basePath = @"C:\inetpub\xml";
          //  string basePath = AppDomain.CurrentDomain.BaseDirectory;
          //  string parentPath = Directory.GetParent(basePath).Parent.Parent.Parent.FullName;


            //  string cadenaConexion = Data.ConstruirCadenaConexion(factura);
            string xmlTemplatePath;
            XDocument xmlDoc = null;

            string cufe = "";
            List<Productos> listaProductos = null;
            Adquiriente adquiriente = null;
            Movimiento movimiento = null;
            Encabezado encabezado = null;

            // Determinar la ruta de la plantilla según el tipo de movimiento
            switch (factura.Tipo_movimiento)
            {
                case "SO1":
                    xmlTemplatePath = Path.Combine(basePath, "Plantilla", "XML.xml");
                    break;
                case "NC":
                    xmlTemplatePath = Path.Combine(basePath, "Plantilla_NC", "NC.xml");
                    break;
                case "ND":
                    xmlTemplatePath = Path.Combine(basePath, "Plantilla_ND", "ND.xml");
                    break;
                default:
                    xmlTemplatePath = Path.Combine(basePath, "Plantilla", "XML.xml");
                    break;
            }

            try
            {
                xmlDoc = XDocument.Load(xmlTemplatePath);

                // Llamar al método apropiado según el tipo de movimiento
                if (factura.Tipo_movimiento == "NC")
                {
                    (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorNC.GeneradorNotaCredito(xmlDoc, emisor, factura);
                }
                else if (factura.Tipo_movimiento == "ND")
                {
                    (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorND.GeneradorNotaDebito(xmlDoc, emisor, factura);
                }
                else if(factura.Tipo_movimiento == "SO1")
                {
                    (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorDS.GeneradorDocSoporte(xmlDoc, emisor, factura);
                }
                else
                {
                    (cufe, listaProductos, adquiriente, movimiento, encabezado) = GeneradorFE.UpdateXmlWithViewModelData(xmlDoc, emisor, factura);
                }
            }
            catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
              //  MessageBox.Show($"Error al cargar la plantilla XML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                facturaConsulta.MarcarComoConError(factura, ex);
                return (string.Empty, string.Empty, string.Empty, string.Empty, new List<Productos>(), null, null, null);
            }

            // Convertir el XML actualizado a string
            string xmlContent = xmlDoc?.ToString() ?? "";

            // Convertir el contenido XML en base64
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
            string base64Encoded = Convert.ToBase64String(bytes);

            // Devolver la tupla con todos los valores
            return (xmlContent, base64Encoded, null, cufe, listaProductos, adquiriente, movimiento, encabezado);
        }




    }


}
