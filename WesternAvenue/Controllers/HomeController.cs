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

        private static List<Location> RoutesData;
         
        public HomeController()
        {
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllLocation()
        {
            RoutesData = Model.GetRoutesOnTheWayToWesternAvenue();

            return new JsonResult
            {
                Data = RoutesData,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            }; 
        }

        //This method get data for specific marker
        public JsonResult GetMarkerData(int locationID)
        { 
            Location l = null;
            l = RoutesData.Where(a => a.LocationID.Equals(locationID)).FirstOrDefault();

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