using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using SubirFicheros.Models;
using SubirFicheros.Utils;

namespace SubirFicheros.Controllers
{
    public class HomeController : Controller
    {
        List<TipoFichero> tipos = new List<TipoFichero>()
        {
            new TipoFichero() {id = 1, nombre = "Imagen"},
            new TipoFichero() {id = 2, nombre = "Cualquier otra cosa"},

        };

        ficherosEntities db = new ficherosEntities();
        // GET: Home
        public ActionResult Index()
        {
            var data = db.Ficheros;
            return View(data);
        }

        public ActionResult Subida(int almacen = 1)
        {
            var tipoAlmacen = "";
            if (almacen == 1)
                tipoAlmacen = "interno";
            if (almacen == 2)
                tipoAlmacen = "base64";
            if (almacen == 3)
                tipoAlmacen = "binario";
            if (almacen == 4)
                tipoAlmacen = "azure";

            ViewBag.almacen = tipoAlmacen;

            ViewBag.tipoFichero = new SelectList(tipos, "id", "nombre");


            return View(new Ficheros());
        }

        public ActionResult GetBase64Azure(String nombre)
        {
            var cu = ConfigurationManager.AppSettings["cuentaAzureStorage"];
            var cl = ConfigurationManager.AppSettings["claveAzureStorage"];
            var co = ConfigurationManager.AppSettings["contenedorAzureStorage"];

            var sto = new AzureStorageUtils(cu, cl, co);
            var data = sto.RecuperarFichero(nombre,co);
            var f = new FicheroBase64() {Contenido = Convert.ToBase64String(data)};
            return View(f);
        }
        public FileResult DownloadFile(int id, int tipo = 0)
        {
            byte[] fichero = new byte[] {};
            var f = db.Ficheros.Find(id);
            if (tipo == 0)
            {
                fichero = Convert.FromBase64String(f.datos);
            }
            else if(tipo==1)
            {
                fichero = f.datosb;
            }
            else if (tipo == 2)
            {
                var cu = ConfigurationManager.AppSettings["cuentaAzureStorage"];
                var cl = ConfigurationManager.AppSettings["claveAzureStorage"];
                var co = ConfigurationManager.AppSettings["contenedorAzureStorage"];

                var sto = new AzureStorageUtils(cu, cl, co);

                fichero = sto.RecuperarFichero(f.datos, co);
            }


            return File(fichero, MediaTypeNames.Application.Octet, f.nombre);
        }

        [HttpPost]
        public ActionResult Subida(Ficheros model,
            HttpPostedFileBase fichero)
        {
            if (model.tipo == "interno")
            {
                var n = GestionarFicheros.GuardarFicheroDisco(fichero, Server);
                if (n != null)
                {
                    model.datos = n;
                    model.datosb=new byte[] {1};
                    db.Ficheros.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else if (model.tipo == "base64")
            {
                var data = GestionarFicheros.ToBinario(fichero);
                if (data != null)
                {
                    model.datos = Convert.ToBase64String(data);
                    model.datosb = new byte[] { 1 };
                    model.nombre = fichero.FileName;
                    db.Ficheros.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else if (model.tipo == "binario")
            {
                var datab = GestionarFicheros.ToBinario(fichero);
                if (datab != null)
                {
                    model.datos = "";
                    model.datosb = datab;
                    model.nombre = fichero.FileName;
                    db.Ficheros.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

            }
            else if (model.tipo == "azure")
            {
                var cu = ConfigurationManager.AppSettings["cuentaAzureStorage"];
                var cl = ConfigurationManager.AppSettings["claveAzureStorage"];
                var co = ConfigurationManager.AppSettings["contenedorAzureStorage"];

                var az=new AzureStorageUtils(cu,cl,co);

                var n = Guid.NewGuid();
                var ext = fichero.FileName.Substring(fichero.FileName.LastIndexOf("."));

                az.SubirFichero(fichero.InputStream,n+ext);

                model.datos = n+ext;
                model.nombre = fichero.FileName;

                db.Ficheros.Add(model);
                db.SaveChanges();

            }

        
            return RedirectToAction("Index");


        }
    }
}