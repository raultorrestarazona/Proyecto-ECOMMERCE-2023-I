using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace PROYECTO_E_COMERCE.Models
{
    public class Producto
    {
        public int idproducto { get; set; }

        public string nombreproducto { get; set; }

        public string descripcion { get; set; }

        public string medida { get; set; }

        public decimal precio { get; set; }

        public Int16 stock { get; set; }
    }
}