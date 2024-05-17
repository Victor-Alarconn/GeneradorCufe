using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Productos
    {
        public string? Codigo { get; set; }
        public string? Recibo { get; set; }
        public string? Nit { get; set; }
        public string? Detalle { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Valor { get; set; }
        public decimal Neto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Iva { get; set; }
        public decimal IvaTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Consumo { get; set; }  
        public string? Hora_Digitada { get; set; }
        public int Excluido { get; set; }

        public DateTime Fecha { get; set; }
    }
}
