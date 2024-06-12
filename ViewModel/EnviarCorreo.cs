using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GeneradorCufe.Model;
using iTextSharp.text;
using GeneradorCufe.Consultas;
using iTextSharp.text.pdf;

namespace GeneradorCufe.ViewModel
{
    public  class EnviarCorreo
    {
        public static async Task<bool> Enviar(Emisor emisor, Adquiriente adquiriente, Factura factura, byte[] archivoAdjunto, string cufe)
       {

            if (string.IsNullOrEmpty(adquiriente.Correo_adqui))
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConErrorCorreo(factura, new Exception("El adquiriente no tiene un correo electrónico registrado."));
                return false;
            }

            // Configurar el cliente SMTP
            SmtpClient clienteSmtp = new SmtpClient("mail.rmsoft.com.co");
            clienteSmtp.Port = 587;
            clienteSmtp.Credentials = new NetworkCredential("facturaelectronica@rmsoft.com.co", "p6;li17^wU02");
            // clienteSmtp.Credentials = new NetworkCredential("facturaselectronicas@rmsoft.com.co", "[]Q^nFwROD[6");
            clienteSmtp.EnableSsl = true; // Habilitar SSL
           

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : "";

            string PrefijoNC = "";
            string Documento = "";
            string tipo_documento = "FACTURA ELECTRONICA DE VENTA";
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "NC")
            {
                PrefijoNC = "NC" + factura.Recibo;
            }
            else if(!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
            {
                PrefijoNC = "ND" + factura.Recibo;
            }

            // Crear el mensaje
            MailAddress direccionRemitente = new MailAddress("facturaelectronica@rmsoft.com.co", adquiriente.Nombre_adqu);
            MailAddress direccionDestinatario = new MailAddress(adquiriente.Correo_adqui);
            MailMessage mensaje = new MailMessage(direccionRemitente, direccionDestinatario);

            if (!string.IsNullOrEmpty(adquiriente.Correo2))
            {
                MailAddress direccionDestinatarioSecundario = new MailAddress(adquiriente.Correo2);
                mensaje.To.Add(direccionDestinatarioSecundario);
            }
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "NC")
            {
                mensaje.Subject = $"{Nit}; {emisor.Nombre_emisor}; {PrefijoNC}; 91; {emisor.Nombre_emisor}";
                Documento = PrefijoNC;
                tipo_documento = "NOTA CREDITO";
            }
            else if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0" && factura.Tipo_movimiento == "ND")
            {
                mensaje.Subject = $"{Nit}; {emisor.Nombre_emisor}; {PrefijoNC}; 91; {emisor.Nombre_emisor}";
                Documento = PrefijoNC;
                tipo_documento = "NOTA DEBITO";
            }
            else
            {
                mensaje.Subject = $"{Nit}; {emisor.Nombre_emisor}; {factura.Facturas}; 01; {emisor.Nombre_emisor}";
                Documento = factura.Facturas;
            }


            mensaje.IsBodyHtml = true; // Establecer el cuerpo del mensaje como HTML
            /*string rutaImagen = @"C:\Users\hp\source\repos\GeneradorCufe\xml\logo.png";*/ // Victor
            string rutaImagen = @"C:\Users\Programacion01\source\repos\RepoVictor\GeneradorCufe\xml\logo.png"; // Oficina
                                                                                                               // string rutaImagen = @"C:\inetpub\xml\Imagenes\logo.png"; // Gigas
            if (File.Exists(rutaImagen))
            {
                // Agregar la imagen como un archivo adjunto al mensaje
                Attachment imagenAdjunta = new Attachment(rutaImagen);
                imagenAdjunta.ContentId = "qrCodeImage"; // Asignar un CID único para la imagen
                mensaje.Attachments.Add(imagenAdjunta);
            }
            else
            {
                // La imagen no existe, puedes manejar esta situación según tus necesidades
                Console.WriteLine("La imagen no existe en la ruta especificada.");
            }

            // Construir el cuerpo del mensaje en formato HTML
            string cuerpo = $@"
                        <div style='text-align: center; border: 5px solid #ccc; border-radius: 10px; padding: 10px;'>
                            <div style='background-color: red; display: inline-block; padding: 5px 10px; border-radius: 5px; vertical-align: middle;'>
                                <img src='cid:qrCodeImage' alt='Logo' style='width: 100px; height: auto; vertical-align: middle;' />
                                <strong style='color: white;'>¡HOLA, {adquiriente.Nombre_adqu}!</strong>
                            </div>
                            <br/><br/>
                            <span style='font-size: 18px; color: red; text-decoration: underline;'>HA RECIBIDO UN DOCUMENTO ELECTRÓNICO</span><br/>
                            ADJUNTO A ESTE CORREO, A CONTINUACIÓN ENCONTRARÁ RESUMEN DE ESTE DOCUMENTO:<br/><br/>
                            <strong>Emisor:</strong> {emisor.Nombre_emisor}<br/>
                            <strong>Prefijo y número del documento:</strong> {Documento}<br/>
                            <strong>Tipo de documento:</strong> {tipo_documento}<br/>
                            <strong>Fecha de emisión:</strong> {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}<br/>
                            <br/>
                            En caso de tener alguna inquietud respecto a la información contenida en el documento por favor comunicarse con {emisor.Nombre_emisor}<br/><br/>
                            <div style='background-color: #f2f2f2; padding: 10px;'>
                               <strong style='color: black;'>INFORMACIÓN CONFIDENCIAL:</strong> Este correo electrónico y todos sus archivos adjuntos contienen información confidencial propiedad de {emisor.Nombre_emisor}. Está destinado únicamente para el uso del destinatario o la entidad a la que está dirigido. Si usted no es el destinatario previsto, queda prohibida cualquier copia, distribución, divulgación o almacenamiento de este mensaje, y puede estar sujeto a acciones legales. Si ha recibido este mensaje por error, le pedimos que lo elimine inmediatamente y se ponga en contacto con el remitente para informar del error, absténgase de divulgar su contenido.
                            </div>
                            <p style='font-size: 9px;'>Este es un sistema automático, por favor no responda este mensaje al correo remitente.<br/>
                           Recuerde agregar esta dirección de correo a sus contactos y lista de remitentes seguros para asegurarse de recibir nuestros mensajes.</p>
                            <br/><br/>
                            <strong> Cadena S.A.</strong><br/>
                            Proveedor Tecnológico de Facturación Electrónica<br/>
                           <strong>RMSOFT CASA DE SOFTWARE S.A.S</strong><br/>
                            Software Comercial<br/>
                           <a href='https://rmsoft.com.co/' style='color: #2190E3;'>https://rmsoft.com.co/</a>
                        </div>";
            mensaje.Body = cuerpo;

            // Adjuntar el archivo ZIP al mensaje
            mensaje.Attachments.Add(new Attachment(new MemoryStream(archivoAdjunto), $"{cufe}.zip"));


            try
            {
                // Enviar el mensaje
                clienteSmtp.Send(mensaje);
                return true;
            }
            catch (SmtpException ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConErrorCorreo(factura, ex);
                return false;
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
                return false;
            }
            finally
            {
                // Liberar recursos
                mensaje.Dispose();
                clienteSmtp.Dispose();
            }

            
        }


    }
}
