using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace PROYECTO_E_COMERCE.Models
{
    public class Cliente
    {
        [Display(Name = "Codigo")] public string idcliente { get; set; }
        [Display(Name = "Cliente")] public string nombrecia { get; set; }
        [Display(Name = "Cliente")] public string apellido { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
    }
}