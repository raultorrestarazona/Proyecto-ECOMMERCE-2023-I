using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PROYECTO_E_COMERCE.Models;
namespace PROYECTO_E_COMERCE.Controllers
{
    public class ECommerceController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

        IEnumerable<Producto> productos(string nombre = null)
        {
            List<Producto> temporal = new List<Producto>();

            if (nombre == null) return temporal;
            //si nombre no es null

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("proc_listar_productos", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Producto reg = new Producto()
                    {
                        idproducto = dr.GetInt32(0),
                        nombreproducto = dr.GetString(1),
                        descripcion = dr.GetString(2),
                        medida = dr.GetString(3),
                        precio = dr.GetDecimal(4),
                        stock = dr.GetInt16(5),
                    };
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }
        public ActionResult Tienda(string nombre = "")
        {
            //dentro del Action definir el Session, solo se ejecuta una vez
            //si es null, entonces lo definimos con una Lista de Item
            if (Session["carrito"] == null)
            {
                Session["carrito"] = new List<Item>();
            }
            //asignar a Session["usuario"] en nombre del cliente InicioSesion()
            Session["usuario"] = InicioSesion();

            //enviar los productos y su parametro

            return View(productos(nombre));

        }

        public ActionResult Selecciona(int? id = null)
        {
            if (id == null) return RedirectToAction("Tienda");

            //envio a la Vista el producto seleccionado
            Producto reg = productos("").Where(p => p.idproducto == id).FirstOrDefault();
            return View(reg);
        }

        [HttpPost]
        public ActionResult Selecciona(int id = 0, int cantidad = 0)

        {
            //1.recuperar los datos del producto seleccionado
            Producto reg = productos("").Where(p => p.idproducto == id).FirstOrDefault();
            //2. instanciar Item y almacenar los datos
            Item it = new Item()
            {
                idproducto = reg.idproducto,
                nombreproducto = reg.nombreproducto,
                descripcion = reg.descripcion,
                medida = reg.medida,
                precio = reg.precio,
                cantidad = cantidad,
            };

            //3.agrego it al Session[canasta]; definir una referencia al Session

            //creando al temporal
            List<Item> temporal = (List<Item>)Session["carrito"];

            //Agregando al session
            temporal.Add(it);

            ViewBag.mensaje = "Item agregado al carrito";

            return View(reg);

        }

        public ActionResult Carrito()
        {
            if (Session["carrito"] == null) return RedirectToAction("Tienda");

            //enviar al Session["carrito"]
            return View((List<Item>)Session["carrito"]);
        }
        public ActionResult Eliminar(int id = 0)
        {
            //eliminar el Item de idproducto id del Session carrito

            List<Item> lista = (List<Item>)Session["carrito"];
            Item del = lista.Where(i => i.idproducto == id).FirstOrDefault();
            lista.Remove(del);
            return RedirectToAction("Carrito");

        }

        string InicioSesion()
        {
            if (Session["login"] == null)
                return null;
            else
                return (Session["login"] as Cliente).nombrecia;
        }

        Cliente Buscar(string login, string clave)
        {
            Cliente reg = null; //inicializar
            SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString);
            SqlCommand cmd = new SqlCommand("sp_login", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", login);
            cmd.Parameters.AddWithValue("@clave", clave);
            cn.Open();

            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                reg = new Cliente();
                reg.idcliente = dr["idcliente"].ToString();
                reg.nombrecia = dr["nombrecia"].ToString();
                reg.direccion = dr["direccion"].ToString();
                reg.telefono = dr["telefono"].ToString();
            }
            dr.Close();
            cn.Close();
            return reg;
        }

        public ActionResult Login()
        {
            return View(); //en blanco
        }

        [HttpPost]
        public ActionResult Login(string login, string clave)
        {
            {
                //ejecutar el Buscar y lo almaceno en Session["login"]
                Session["login"] = Buscar(login, clave);
                if (Session["login"] == null)
                {
                    ViewBag.mensaje = "Usuario o Clave Incorrecta";
                    return View();
                }
                else
                {
                    //si lo encontro, nos direcccionamos a la Tienda
                    return RedirectToAction("Tienda");
                }
            }
        }
        public ActionResult Cerrar()
        {
            //cerrar la sesion del login del usuario
            Session["login"] = null;
            return RedirectToAction("Tienda");
        }

        public ActionResult Comprar()
        {
            //verifico si Session es null
            if (Session["login"] == null)
                return RedirectToAction("Login");
            else
            {
                //envio a la vista los siguiente datos Comprar
                ViewBag.usuario = InicioSesion();
                ViewBag.carrito = Session["carrito"] as List<Item>;
                return View(Session["login"] as Cliente);
            }

        }

        string autogenerado()
        {
            //ejecutar la funcion del autogenerado
            string cod = "";
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("select dbo.autogenera()", cn);
                cn.Open();
                //ejecuta y retorna el valor (object) convertido a string
                cod = (string)cmd.ExecuteScalar();
                cn.Close();
            }
            return cod;
        }

        public ActionResult Pedido()
        {
            string nropedido = autogenerado();
            string mensaje = ""; //almaceno el mensaje del proceso
            string idcliente = (Session["login"] as Cliente).idcliente;

            //definir la transaccion

            SqlConnection cn = new SqlConnection(cadena);
            cn.Open();
            SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                //insertar un registro a tb_pedidos, utilice el Session["login"]; idcliente
                SqlCommand cmd = new SqlCommand("SP_INSERTAR_TB_PEDIDOS", cn, tr);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@n", nropedido);
                cmd.Parameters.AddWithValue("@id", idcliente);
                cmd.ExecuteNonQuery();


                //insertar los registros a tb_pedidos_deta utilizando el Session["carrito"]

                foreach (Item reg in Session["carrito"] as List<Item>)
                {
                    cmd = new SqlCommand("SP_INSERTAR_TB_PEDIDO_DETALLE", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@n", nropedido);
                    cmd.Parameters.AddWithValue("@id", reg.idproducto);
                    cmd.Parameters.AddWithValue("@pre", reg.precio);
                    cmd.Parameters.AddWithValue("@can", reg.cantidad);
                    cmd.ExecuteNonQuery();
                }
                //si todo esta Ok
                tr.Commit();
                mensaje = string.Format("El pedido {0} ha sido registrado", nropedido);
            }
            catch (SqlException ex)
            {
                mensaje = ex.Message;
                tr.Rollback(); //deshacer la operacion
            }
            finally { cn.Close(); }
            //este pedido direcciona a una Ventana Final despues de ejecutar el Pedido
            return RedirectToAction("Mensajes", new { m = mensaje });

        }

        public ActionResult Mensajes(string m)
        {
            //envio el de m
            ViewBag.mensaje = m;
            //finalizo la sesion
            Session.Abandon();
            return View();
        }

        public ActionResult Registro()
        {
            return View();
        }
        [HttpPost] public ActionResult Registro(Cliente reg, string usuario, string contraseña)
        {
            string mensaje = "";
            SqlConnection cn = new SqlConnection(cadena);
            cn.Open();
            SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_Insertar_Clientes", cn, tr);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombreCia", reg.nombrecia);
                cmd.Parameters.AddWithValue("@apellido", reg.apellido);
                cmd.Parameters.AddWithValue("@direccion", reg.direccion);
                cmd.Parameters.AddWithValue("@idpais", "002");
                cmd.Parameters.AddWithValue("@telefono", reg.telefono);
                cmd.Parameters.AddWithValue("@idlogin", usuario);
                cmd.Parameters.AddWithValue("@clave", contraseña);
                int i = cmd.ExecuteNonQuery();
                tr.Commit();
                mensaje = string.Format("Se ha registrado {0} exitosamente", i);
            }
            catch (SqlException ex)
            {
                mensaje = ex.Message;
                tr.Rollback();
            }
            finally
            {
                cn.Close();
                ViewBag.salida = mensaje;
            }
            return View(reg);
        }
    }
}