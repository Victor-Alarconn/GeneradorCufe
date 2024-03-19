using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Encabezado
    {
        public int Id_enc { get; set; }
        public string Factura_enc { get; set; }
        public DateTime Fecha_enc { get; set; }
        public decimal Valor_net { get; set; }
        public decimal Vlr_iva { get; set; }
        public decimal Vlr_ipo { get; set; }
        public decimal Vlr_dsto { get; set; }
        public string? Notas { get; set; }
        public string? Cufe { get; set; }
        public string? Cude { get; set; }
    }
}
