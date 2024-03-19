using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class FormaPago
    {
        public int Id_forma { get; set; }
        public string? Codigo_forma { get; set; }
        public decimal Valor_pago { get; set; }
        public DateTime Fecha_pago { get; set; }
        public string? Factura_pago { get; set; }
        public int Terceros_pago { get; set; }
}
}
