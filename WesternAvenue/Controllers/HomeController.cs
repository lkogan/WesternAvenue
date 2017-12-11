using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WesternAvenue.Controllers;
using System.Net;
using ProtoBuf;
using transit_realtime;
using System.Text;
using System.IO;
using Newtonsoft.Json;

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
     
    public class Trip
    {
        [JsonProperty("trip_id")]
        public string trip_id { get; set; }

        [JsonProperty("route_id")]
        public string route_id { get; set; }

        [JsonProperty("direction_id")]
        public object direction_id { get; set; }

        [JsonProperty("start_time")]
        public string start_time { get; set; }

        [JsonProperty("start_date")]
        public string start_date { get; set; }

        [JsonProperty("schedule_relationship")]
        public bool schedule_relationship { get; set; }
    }

    public class Vehicle2
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("label")]
        public string label { get; set; }

        [JsonProperty("license_plate")]
        public object license_plate { get; set; }
    }

    public class Position
    {
        [JsonProperty("latitude")]
        public double latitude { get; set; }

        [JsonProperty("longitude")]
        public double longitude { get; set; }

        [JsonProperty("bearing")]
        public object bearing { get; set; }

        [JsonProperty("odometer")]
        public object odometer { get; set; }

        [JsonProperty("speed")]
        public object speed { get; set; }
    }

    public class Timestamp
    {
        [JsonProperty("low")]
        public DateTime low { get; set; }

        [JsonProperty("high")]
        public int high { get; set; }				
				
        [JsonProperty("unsigned")]
        public bool unsigned { get; set; }				
    }				
				
    public class Vehicle
    {
        [JsonProperty("trip")]
        public Trip trip { get; set; }

        [JsonProperty("vehicle")]
        public Vehicle2 vehicle { get; set; }

        [JsonProperty("position")]
        public Position position { get; set; }

        [JsonProperty("current_stop_sequence")]
        public object current_stop_sequence { get; set; }

        [JsonProperty("stop_id")]
        public object stop_id { get; set; }

        [JsonProperty("current_status")]
        public int current_status { get; set; }

        [JsonProperty("timestamp")]
        public Timestamp timestamp { get; set; }

        [JsonProperty("congestion_level")]
        public object congestion_level { get; set; }

        [JsonProperty("occupancy_status")]
        public object occupancy_status { get; set; }				
    }				
				
    public class RootObject
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("is_deleted")]
        public bool is_deleted { get; set; }

        [JsonProperty("trip_update")]
        public object trip_update { get; set; }

        [JsonProperty("vehicle")]
        public Vehicle vehicle { get; set; }

        [JsonProperty("alert")]
        public object alert { get; set; }
    }



public class HomeController : Controller
    {
        public List<Location> lstLocations;

        public HomeController()
        { 
            lstLocations = new List<Location>();
             
            string apiURL = "https://gtfsapi.metrarail.com/gtfs/positions";
            string userName = "ddbb87512b3fc392b58a69c485ff8ce8";
            string password = "91b0f62e8049d0d51b35f27a494e5b46";
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password));
             
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(
                new Uri(apiURL), "Basic", new NetworkCredential(userName, password)
            );

            WebRequest req = HttpWebRequest.Create(apiURL);
            req.Method = "GET";
            req.Headers.Add("Authorization", "Basic " + encoded); 
            req.Credentials = credentialCache;
              
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string output = sr.ReadToEnd();

            List<RootObject> myobjList = JsonConvert.DeserializeObject<List<RootObject>>(output);
            
            for (int i = 0; i < myobjList.Count; i++)
            {
                Location loc = new Location
                {
                    LocationID = Convert.ToInt32(myobjList[i].id),

                    Title = myobjList[i].vehicle.trip.trip_id,

                    Lat = myobjList[i].vehicle.position.latitude.ToString(),

                    Long = myobjList[i].vehicle.position.longitude.ToString(),

                    Address = myobjList[i].vehicle.trip.trip_id + Environment.NewLine
                            + myobjList[i].vehicle.timestamp.low.ToLocalTime() + Environment.NewLine
                            + "Trip started: " + myobjList[i].vehicle.trip.start_date + " " + myobjList[i].vehicle.trip.start_time,

                    ImagePath = "http://individual.icons-land.com/IconsPreview/Transport/PNG/RailTransport/48x48/SteamLocomotive__Black.png"
                };

                lstLocations.Add(loc); 
            }
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