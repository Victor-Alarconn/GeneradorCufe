using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Movimiento
    {
        public string? Nit { get; set; }
        public decimal Valor { get; set; }
        public decimal Valor_iva { get; set; }
        public decimal Valor_dsto { get; set; }
        public decimal Valor_neto { get; set; }
        public decimal Exentas { get; set; }
        public DateTime Fecha_Factura { get; set; }
        public string? Hora_dig { get; set; }
        public decimal Retiene { get; set; }

    }
}
