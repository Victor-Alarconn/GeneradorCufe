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

namespace GeneradorCufe.ViewModel
{
    public  class EnviarCorreo
    {
        public static async Task<bool> Enviar(Emisor emisor, Adquiriente adquiriente, Factura factura, byte[] archivoAdjunto, string cufe)
        {
            // Configurar el cliente SMTP
            SmtpClient clienteSmtp = new SmtpClient("mail.rmsoft.com.co");
            clienteSmtp.Port = 587;
            clienteSmtp.Credentials = new NetworkCredential("facturaelectronica@rmsoft.com.co", "J%[XE9.X0i]{");
            clienteSmtp.EnableSsl = true; // Habilitar SSL
           

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : "";

            string PrefijoNC = "";
            string Documento = "";
            string tipo_documento = "FACTURA ELECTRONICA DE VENTA";
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
            {
                PrefijoNC = "NC" + factura.Recibo;
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
            if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
            {
                mensaje.Subject = $"{Nit}; {emisor.Nombre_emisor}; {PrefijoNC}; 91; {emisor.Nombre_emisor}";
                Documento = PrefijoNC;
                tipo_documento = "NOTA CREDITO";
            }
            else
            {
                mensaje.Subject = $"{Nit}; {emisor.Nombre_emisor}; {factura.Facturas}; 01; {emisor.Nombre_emisor}";
                Documento = factura.Facturas;
            }


            mensaje.IsBodyHtml = true; // Establecer el cuerpo del mensaje como HTML

            // Construir el cuerpo del mensaje en formato HTML
            string cuerpo = $@"
                     <div style='text-align: center;'>
                     <strong>ESTIMADO/A CLIENTE,</strong><br/><br/>
                     <strong>{adquiriente.Nombre_adqu}</strong><br/><br/>
                     Ha recibido una Factura o Nota Electrónica adjunta a este correo, a continuación encontrará resumen de este documento:<br/><br/>
                     <strong>Emisor:</strong> {emisor.Nombre_emisor}<br/>
                     <strong>Prefijo y número del documento:</strong> {Documento}<br/>
                     <strong>Tipo de documento:</strong> {tipo_documento}<br/>
                     <strong>Fecha de emisión:</strong> {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}<br/>
                     </div>
                     <br/>
                     En caso de tener alguna inquietud respecto a la información contenida en el documento por favor comunicarse con {emisor.Nombre_emisor}<br/><br/>
                     NOTA CONFIDENCIAL: La información contenida en este e-mail y en todos sus archivos anexos es confidencial de {emisor.Nombre_emisor}, solo para uso individual del destinatario o entidad a quienes está dirigido. Si usted no es destinatario, cualquier almacenamiento, distribución, difusión o copia de este mensaje está estrictamente prohibida y sancionada por la ley. Si por error recibe este mensaje, le ofrecemos disculpas, por favor elimínela inmediatamente y notifique de su error a la persona que la envió, absteniéndose de divulgar su contenido.";

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
                // Manejar la excepción SmtpException
                Console.WriteLine("Error al enviar el correo: " + ex.Message);
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
