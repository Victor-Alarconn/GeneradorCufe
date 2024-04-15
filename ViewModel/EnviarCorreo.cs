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
        public static void Enviar(string remitente, string destinatario, string asunto, string cuerpoMensaje, byte[] archivoAdjunto)
        {
            // Configurar el cliente SMTP
            SmtpClient clienteSmtp = new SmtpClient("mail.rmsoft.com.co");
            clienteSmtp.Port = 465;
            clienteSmtp.Credentials = new NetworkCredential("facturaelectronica@rmsoft.com.co", "9%TJ?+e7uugX");
            clienteSmtp.EnableSsl = true; // Habilitar SSL


            // Crear el mensaje
            MailMessage mensaje = new MailMessage(remitente, destinatario, asunto, cuerpoMensaje);

            // Adjuntar el archivo ZIP al mensaje
            mensaje.Attachments.Add(new Attachment(new MemoryStream(archivoAdjunto), "documentos.zip"));

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
