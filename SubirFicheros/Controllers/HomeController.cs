﻿using System;
using System.Collections.Generic;
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
        List<TipoFichero> tipos=new List<TipoFichero>()
        {
            new TipoFichero() {id=1,nombre = "Imagen"},
            new TipoFichero() {id=2,nombre = "Cualquier otra cosa"},

        };
            ficherosEntities db=new ficherosEntities();
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

            ViewBag.almacen=tipoAlmacen;

            ViewBag.tipoFichero = new SelectList(tipos, "id", "nombre");


            return View(new Ficheros());
        }

        public FileResult DownloadFile(int id, int tipo = 0)
        {
            byte[] fichero;
            var f = db.Ficheros.Find(id);
            if (tipo == 0)
            {
                fichero = Convert.FromBase64String(f.datos);
            }
            else
            {
                fichero = f.datosb;
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
            return RedirectToAction("Index");
        }
        


    }
}