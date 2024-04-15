using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GeneradorCufe.ViewModel
{
    public  class EnviarCorreo
    {
        public static void Enviar(string remitente, string destinatario, string asunto, string cuerpoMensaje, byte[] archivoAdjunto, string cufe)
        {
            // Configurar el cliente SMTP
            SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com");
            clienteSmtp.Port = 587;
            clienteSmtp.Credentials = new NetworkCredential("sistemas.rmsoft@gmail.com", "ektq xifn kjsc mwoy");
            clienteSmtp.EnableSsl = true; // Habilitar SSL

            // Crear el mensaje
            MailAddress direccionRemitente = new MailAddress(remitente, "RAUL OSVALDO RAMOS MELCHOR");
            MailAddress direccionDestinatario = new MailAddress(destinatario);
            MailMessage mensaje = new MailMessage(direccionRemitente, direccionDestinatario);
            mensaje.Subject = asunto;

            // Construir el cuerpo del mensaje
            StringBuilder cuerpo = new StringBuilder();
            cuerpo.AppendLine("<div style='text-align: center;'>");
            cuerpo.AppendLine("<strong>ESTIMADO/A CLIENTE,</strong>");
            cuerpo.AppendLine("<br/><br/>");
            cuerpo.AppendLine("<strong>VICTOR ALARCON ALARCON</strong>");
            cuerpo.AppendLine("<br/><br/>");
            cuerpo.AppendLine("Ha recibido una Factura o Nota Electrónica adjunta a este correo, a continuación encontrará resumen de este documento:");
            cuerpo.AppendLine("<br/><br/>");
            cuerpo.AppendLine("<strong>Emisor:</strong> RAUL");
            cuerpo.AppendLine("<br/><strong>Prefijo y número del documento:</strong> SETT 5159");
            cuerpo.AppendLine("<br/><strong>Tipo de documento:</strong> FACTURA ELECTRONICA DE VENTA");
            cuerpo.AppendLine("<br/><strong>Fecha de emisión:</strong> 2024-04-11 09:04:38");
            cuerpo.AppendLine("</div>");
            cuerpo.AppendLine();
            cuerpo.AppendLine("En caso de tener alguna inquietud respecto a la información contenida en el documento por favor comunicarse con RAUL");
            cuerpo.AppendLine();
            cuerpo.AppendLine("NOTA CONFIDENCIAL: La información contenida en este e-mail y en todos sus archivos anexos es confidencial de RAUL, solo para uso individual del destinatario o entidad a quienes está dirigido. Si usted no es destinatario, cualquier almacenamiento, distribución, difusión o copia de este mensaje está estrictamente prohibida y sancionada por la ley. Si por error recibe este mensaje, le ofrecemos disculpas, por favor elimínela inmediatamente y notifique de su error a la persona que la envió, absteniéndose de divulgar su contenido.");
            mensaje.Body = cuerpo.ToString();

            // Adjuntar el archivo ZIP al mensaje
            mensaje.Attachments.Add(new Attachment(new MemoryStream(archivoAdjunto), $"{cufe}.zip"));

            try
            {
                // Enviar el mensaje
                clienteSmtp.Send(mensaje);
                Console.WriteLine("¡Correo electrónico enviado correctamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
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
