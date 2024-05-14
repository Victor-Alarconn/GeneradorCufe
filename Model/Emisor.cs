using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Emisor
    {
        public string? Nombre_emisor { get; set; }
        public string? Codigo_FormaPago_emisor { get; set; }
        public string? cude { get; set; }
        public string? Nombre_municipio_emisor { get; set; }
        public string? Codigo_departamento_emisor { get; set; }
        public string? Nombre_departamento_emisor { get; set; }
        public string? Direccion_emisor { get; set; }
        public string? Codigo_postal_emisor { get; set; }
        public string? Nit_emisor { get; set; }
        public string? Responsable_emisor { get; set; }
        public string? Correo_emisor { get; set; }
        public decimal Tipo_emisor{ get; set; }
        public string? Telefono_emisor { get; set; }
        public decimal  Retiene_emisor { get; set; }
        public string? Url_emisor { get; set; }
        public string? Ciudad_emisor { get; set; }

    }
}
