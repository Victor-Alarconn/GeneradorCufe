﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class Movimiento
    {
        public decimal Valor { get; set; }
        public decimal Valor_iva { get; set; }
        public decimal Valor_dsto { get; set; }
        public decimal Valor_neto { get; set; }
        public decimal Exentas { get; set; }

    }
}
