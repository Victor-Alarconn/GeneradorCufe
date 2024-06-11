using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class CorreoCuenta
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int CorreosEnviados { get; set; }
        public DateTime UltimoEnvio { get; set; }

        public CorreoCuenta(string email, string password)
        {
            Email = email;
            Password = password;
            CorreosEnviados = 0;
            UltimoEnvio = DateTime.MinValue;
        }

        public void ActualizarEnvio()
        {
            CorreosEnviados++;
            UltimoEnvio = DateTime.Now;
        }

        public bool PuedeEnviar()
        {
            if (CorreosEnviados < 120) return true;
            if ((DateTime.Now - UltimoEnvio).TotalHours >= 1)
            {
                CorreosEnviados = 0;
                return true;
            }
            return false;
        }
    }

}
