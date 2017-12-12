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

        public DateTime ScheduledTime { get; set; }

        public string TripURL { get; set; }

        public string ArrivesIn { get; set; }
    }
      
    public class HomeController : Controller
    {
        private const string METRA_API_URL = "https://gtfsapi.metrarail.com/gtfs/";

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

            string tripUpdateJSON = Get_JSON_GTFS_Response(METRA_API_URL + "tripUpdates");
            List<TripUpdateCollection> tripUpdateList = JsonConvert.DeserializeObject<List<TripUpdateCollection>>(tripUpdateJSON);
            
            string positionJSON = Get_JSON_GTFS_Response(METRA_API_URL + "positions");
            List<TripPosition> positionList = JsonConvert.DeserializeObject<List<TripPosition>>(positionJSON);

            List<string> lstRoutesFilter = new List<string>();
            lstRoutesFilter.Add("MD-N");
            lstRoutesFilter.Add("MD-W");
            lstRoutesFilter.Add("NCS");
             
            //Filter positions to contain only routes that stop at Western Avenue
            positionList = positionList
                .Where(a => lstRoutesFilter.Any(b => a.vehicle.trip.trip_id.StartsWith(b)))
                .ToList();

            for (int i = 0; i < positionList.Count; i++)
            {
                string tripID = positionList[i].vehicle.trip.trip_id;
                
                string routeID = positionList[i].vehicle.trip.route_id;

                string stopTimesJSON = Get_JSON_GTFS_Response(METRA_API_URL + "schedule/stop_times/" + tripID);

                string stationsJSON = Get_JSON_GTFS_Response(METRA_API_URL + "schedule/stops");  
                List<Station> stationsList = JsonConvert.DeserializeObject<List<Station>>(stationsJSON);
                Dictionary<string, string> dictStations = stationsList.ToDictionary(prop => prop.stop_id, prop => prop.stop_name);

                TripUpdateCollection tuc = tripUpdateList.Where(x => x.id.Equals(tripID)).FirstOrDefault();
                if (tuc == null) continue;

                string lastStationAbbr = tuc.trip_update.stop_time_update[0].stop_id;
                if ((lastStationAbbr.Equals("WESTERNAVE")) || (lastStationAbbr.Equals("CUS"))) continue;

                int delayInSeconds = tuc.trip_update.stop_time_update[0].arrival.delay;

                string Delay = string.Empty;
                TimeSpan ts = new TimeSpan(0, 0, 0);

                if (delayInSeconds > 0)
                {
                    ts = TimeSpan.FromSeconds(delayInSeconds);
                    Delay = Environment.NewLine + (int)ts.TotalMinutes + " min late";
                }
               

                DateTime dtAtLastStation = tuc.trip_update.stop_time_update[0].departure.time.low;
                DateTime adjDtAtLastStation = dtAtLastStation.Add(new TimeSpan(-6, 0, 0));
                string timeAtLastStation = adjDtAtLastStation.ToString("HH:mm");
  
                List<StopOnTrip> stopTimesList = JsonConvert.DeserializeObject<List<StopOnTrip>>(stopTimesJSON);
                if (stopTimesList == null) continue;

                if (!stopTimesList[0].stop_id.Equals("CUS"))    //INBOUND - DOES not start at Chicago Union Station
                {
                    StopOnTrip westernAve = stopTimesList.Where(x => x.stop_id.Equals("WESTERNAVE")).FirstOrDefault();
                    if (westernAve == null) continue;

                    string arrivalTimeOnWestern = westernAve.arrival_time;
                    DateTime dtArrivalTimeOnWestern = DateTime.ParseExact(arrivalTimeOnWestern,
                        "H:m:s",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None);

                    dtArrivalTimeOnWestern = dtArrivalTimeOnWestern.Add(ts);

                    string dateTimeString = positionList[i].vehicle.trip.start_date + " " + positionList[i].vehicle.trip.start_time;

                    DateTime startDt = DateTime.ParseExact(dateTimeString,
                                            "yyyyMMdd HH:mm:ss",
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.None);

                    DateTime dtUpdateTime = positionList[0].vehicle.timestamp.low;

                    TimeSpan tsArrivesIn = (DateTime)dtArrivalTimeOnWestern - dtUpdateTime.Add(new TimeSpan(-6, 0, 0)); 
                    int arrivesInMinutes = (int)tsArrivesIn.TotalMinutes;

                    Location loc = new Location
                    {
                        LocationID = Convert.ToInt32(positionList[i].id),

                        TripID = tripID,

                        Lat = positionList[i].vehicle.position.latitude.ToString(),

                        Long = positionList[i].vehicle.position.longitude.ToString(),

                        ArrivesIn = arrivesInMinutes + " min",

                        ScheduledTime = dtArrivalTimeOnWestern,

                        Description = "Arrives at Western Ave: " + dtArrivalTimeOnWestern.ToString("HH:mm") + Delay + Environment.NewLine +
                        "Last Stop: " + dictStations[lastStationAbbr] + ", " + timeAtLastStation, 

                        ImagePath = "https://png.icons8.com/material/2x/train.png",

                        TripURL = "https://metrarail.com/maps-schedules/train-lines/" + routeID + "/trip/" + tripID
                    };

                    lstLocations.Add(loc); 
                } 
            }


            //Sort by start time
            lstLocations = lstLocations.OrderBy(x => x.ScheduledTime).ToList(); 
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