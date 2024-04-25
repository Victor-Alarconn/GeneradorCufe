using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Encabezado
    {
        public string? Autorizando { get; set; }
        public DateTime Fecha_inicio { get; set; }
        public DateTime Fecha_termina { get; set; }
        public int R_inicio { get; set; }
        public int R_termina { get; set; }
        public string? Prefijo{ get; set; }
        public string? Resolucion { get; set; }
        public string? Notas { get; set; }
        public string? Nota_fin { get; set; }

    }
}
