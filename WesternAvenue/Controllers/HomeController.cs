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
using System.Globalization;
using static WesternAvenue.Models.TripPositionModels;
using static WesternAvenue.Models.AuxModels;
using static WesternAvenue.Models.TripUpdateModels;

namespace WesternAvenue.Controllers
{
    public class Location
    {
        public int LocationID { get; set; }

        public string TripID { get; set; }

        public string Lat { get; set; }

        public string Long { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }
    }
      
    public class HomeController : Controller
    {
        public List<Location> lstLocations;

        public string Get_JSON_GTFS_Response(string apiURL)
        { 
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

            return output;
        }

        public HomeController()
        { 
            lstLocations = new List<Location>();
              
            string positionJSON = Get_JSON_GTFS_Response("https://gtfsapi.metrarail.com/gtfs/positions");
            List<TripPosition> positionList = JsonConvert.DeserializeObject<List<TripPosition>>(positionJSON);
            
            string tripUpdateJSON = Get_JSON_GTFS_Response("https://gtfsapi.metrarail.com/gtfs/tripUpdates");
            List<TripUpdateCollection> tripUpdateList = JsonConvert.DeserializeObject<List<TripUpdateCollection>>(tripUpdateJSON);

            for (int i = 0; i < positionList.Count; i++)
            {
                string tripID = positionList[i].vehicle.trip.trip_id;

                //Routes that stop at Western Avenue station
                if (
                    (tripID.StartsWith("MD-N"))||
                    (tripID.StartsWith("MD-W")) ||
                    (tripID.StartsWith("NCS"))
                    )
                {
                    string stopTimesJSON = Get_JSON_GTFS_Response("https://gtfsapi.metrarail.com/gtfs/schedule/stop_times/" + tripID);

                    TripUpdateCollection tuc = tripUpdateList.Where(x => x.id.Equals(tripID)).FirstOrDefault();

                    var tripDelay = tuc.trip_update.delay;
                     
                    List<StopOnTrip> stopsList = JsonConvert.DeserializeObject<List<StopOnTrip>>(stopTimesJSON);

                    //if (!stopsList[0].stop_id.Equals("CUS")) //If DOES not start at Chicago Union Station = INBOUND
                    if (stopsList[0].stop_id.Equals("CUS")) //If starts at Chicago Union Station = OUTBOUND
                    {
                        StopOnTrip westernAve = stopsList.Where(x => x.stop_id.Equals("WESTERNAVE")).FirstOrDefault();

                        string arrivalTimeOnWestern = westernAve.arrival_time;

                        string dateTimeString = positionList[i].vehicle.trip.start_date + " " + positionList[i].vehicle.trip.start_time;

                        DateTime startDt = DateTime.ParseExact(dateTimeString,
                                                "yyyyMMdd hh:mm:ss",
                                                CultureInfo.InvariantCulture,
                                                DateTimeStyles.None);

                        string Delay = (tripDelay != null) ? Environment.NewLine + tripDelay.ToString() : null;

                        Location loc = new Location
                        {
                            LocationID = Convert.ToInt32(positionList[i].id),

                            TripID = tripID,

                            Lat = positionList[i].vehicle.position.latitude.ToString(),

                            Long = positionList[i].vehicle.position.longitude.ToString(),

                            Description = positionList[i].vehicle.trip.trip_id + Environment.NewLine
                                    + "Scheduled: " + arrivalTimeOnWestern + Delay,

                            ImagePath = "https://png.icons8.com/material/2x/train.png"
                        };

                        lstLocations.Add(loc);
                    }
                  
                }                
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllLocation()
        {
            
            var v = lstLocations.OrderBy(a => a.TripID).ToList();
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