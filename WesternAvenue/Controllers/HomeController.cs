using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WesternAvenue.Controllers;

namespace WesternAvenue.Controllers
{
    public class Location
    {
        public int LocationID { get; set; }

        public string Title { get; set; }

        public string Lat { get; set; }

        public string Long { get; set; }

        public string Address { get; set; }

        public string ImagePath { get; set; }
    }

    public class HomeController : Controller
    {
        public List<Location> lstLocations;

        public HomeController()
        {
            lstLocations = new List<Location>();

            Location loc = new Location
            {
                LocationID = 1,

                Title = "Western Avenue Station",

                Lat = "41.889370",

                Long = "-87.688198",

                Address = "420 N Artesian Ave, Chicago, IL 60612",

                ImagePath = "https://www.iconexperience.com/_img/o_collection_png/green_dark_grey/32x32/plain/fortress_tower.png"
            };

            lstLocations.Add(loc);
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllLocation()
        {
            
            var v = lstLocations.OrderBy(a => a.Title).ToList();
            return new JsonResult
            {
                Data = v,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
           
        }

        //This method gets the markers info from database.  
        public JsonResult GetMarkerData(int locationID)
        {
            
            Location l = null;
            l = lstLocations.Where(a => a.LocationID.Equals(locationID)).FirstOrDefault();

            return new JsonResult
            {
                Data = l,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
             
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}