using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubirFicheros.Controllers;
using SubirFicheros.Models;

namespace TestMvc
{
    public abstract class BasePruebas
    {
        protected HomeController Controller;
        protected ficherosEntities db;
        
        protected BasePruebas()
        {
            db=new ficherosEntities();
            Controller=new HomeController();
        }

    }
}
