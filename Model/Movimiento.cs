using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Movimiento
    {
        public int Id_movimiento { get; set; }
        public string? Tipo_factura { get; set; }
        public string? Tercero_mvt { get; set; }
        public string? Factura_mvt { get; set; }
        public string? Recibo { get; set; }
        public string? Codigo_Articulo { get; set; }
        public string? Cantidad { get; set; }
        public string? Detalle { get; set; }
        public decimal Valor_unidad { get; set; }
        public decimal Iva { get; set; }
        public decimal Ipoconsumo { get; set; }
        public decimal Valor_iva { get; set; }
        public decimal Valor_ipo { get; set; }
        public string? Descuento { get; set; }
        public decimal Valor_dsto { get; set; }
        public DateTime Fecha_factura { get; set; }
        public string? Hora_factura { get; set; }
}
}
