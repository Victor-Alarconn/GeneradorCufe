using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.ViewModel
{
    public  class EnviarCorreo
    {
        public void Enviar(string remitente, string destinatario, string asunto, string cuerpoMensaje)
        {
            // Configura el cliente SMTP
            SmtpClient clienteSmtp = new SmtpClient("mail.rmsoft.com.co");
            clienteSmtp.Port = 465;
            clienteSmtp.Credentials = new NetworkCredential("tucorreo@tudominio.com", "tucontraseña");
            clienteSmtp.EnableSsl = true; // Habilita SSL

            // Crea el mensaje
            MailMessage mensaje = new MailMessage(remitente, destinatario, asunto, cuerpoMensaje);

            try
            {
                // Envía el mensaje
                clienteSmtp.Send(mensaje);
                Console.WriteLine("¡Correo electrónico enviado correctamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
            }
        }
    }
}
