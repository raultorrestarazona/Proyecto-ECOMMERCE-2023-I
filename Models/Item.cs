using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROYECTO_E_COMERCE.Models
{
    public class Item
    {
        public int idproducto { get; set; }

        public string nombreproducto { get; set; }

        public string descripcion { get; set; }

        public string medida { get; set; }

        public decimal precio { get; set; }

        public int cantidad { get; set; }

        public decimal monto { get { return precio * cantidad; } }
    }
}