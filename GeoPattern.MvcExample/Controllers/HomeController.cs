using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeoPattern.MvcExample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Svg(string pattern)
        {
            string svg = GeoPatternGenerator.Generate(new Dictionary<string, string> { { "generator", pattern }, { "color", "#939c3c" } });

            return Content(svg, "image/svg+xml");
        }
    }
}