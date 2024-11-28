using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMSmvc.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult TrackReturn()
        {
            return View();
        }

        public ActionResult Product()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        public ActionResult Supplier()
        { 
            return View();
        }

        public ActionResult AuditLog()
        {
            return View();
        }
        public ActionResult TrackStock()
        {
            return View();
        }
    }
}