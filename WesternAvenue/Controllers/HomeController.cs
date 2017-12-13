using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq; 
using System.Web.Mvc;
using WesternAvenue.Models;
using Location = WesternAvenue.Models.Location;
using WesternAvenueModels = WesternAvenue.Models.WesternAvenueModels;

namespace WesternAvenue.Controllers
{ 
    public class HomeController : Controller
    {
        private WesternAvenueModels Model = new WesternAvenueModels();
        
        public List<Location> lstLocations;
         
        public HomeController()
        {
            lstLocations = Model.GetRoutesOnTheWayToWesternAvenue();
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllLocation()
        {  
            return new JsonResult
            {
                Data = lstLocations,
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