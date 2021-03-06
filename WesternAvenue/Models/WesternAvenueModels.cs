﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using static WesternAvenue.Models.AuxModels;
using static WesternAvenue.Models.TripPositionModels;
using static WesternAvenue.Models.TripUpdateModels;
using j = WesternAvenue.Models.JSON_Models;

namespace WesternAvenue.Models
{
    public class Location
    {
        public int LocationID { get; set; }

        public string TripID { get; set; }

        public string Lat { get; set; }

        public string Long { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }

        public string ArrivalTime { get; set; }

        public string TripURL { get; set; }

        public string ArrivesIn { get; set; }
        public string RouteNumber { get; set; }
    }


    public class WesternAvenueModels
    {
        public List<Location> GetRoutesOnTheWayToWesternAvenue()
        {
            List<Location> lstLocations = new List<Location>();

            string tripUpdateJSON = j.Get_GTFS_Response(j.METRA_API_URL + "tripUpdates");
            List<TripUpdateCollection> tripUpdateList = JsonConvert.DeserializeObject<List<TripUpdateCollection>>(tripUpdateJSON);

            string positionJSON = j.Get_GTFS_Response(j.METRA_API_URL + "positions");
            List<TripPosition> positionList = JsonConvert.DeserializeObject<List<TripPosition>>(positionJSON);

            string currentDateTimeJSON = j.Get_API_Response(j.CURRENT_TIME_API_URL);
            DateTime dtUpdateTime = DateTime.Now;

            if (!string.IsNullOrEmpty(currentDateTimeJSON))
            {
                DateTimeModels currentDateTime = JsonConvert.DeserializeObject<DateTimeModels>(currentDateTimeJSON);
                dtUpdateTime = DateTime.Parse(currentDateTime.formatted);
            }
             
            //Filter positions to contain only routes that stop at Western Avenue
            List<string> lstRoutesFilter = new List<string>();
            lstRoutesFilter.Add("MD-N");
            lstRoutesFilter.Add("MD-W");
            lstRoutesFilter.Add("NCS");
             
            positionList = positionList
                .Where(a => lstRoutesFilter.Any(b => a.vehicle.trip.trip_id.StartsWith(b)))
                .ToList();
 
            for (int i = 0; i < positionList.Count; i++)
            {
                string tripID = positionList[i].vehicle.trip.trip_id;
                string lineID = positionList[i].vehicle.trip.route_id;
                string routeID = positionList[i].vehicle.vehicle.label;

                //Exclude if already past the Western Avenue Station, but had not updated next station yet
                if (positionList[i].vehicle.position.latitude < 41.8888) continue;  

                //Create dictionary to resolve station abbrevs to names
                string stationsJSON = j.Get_GTFS_Response(j.METRA_API_URL + "schedule/stops");
                List<Station> stationsList = JsonConvert.DeserializeObject<List<Station>>(stationsJSON);
                Dictionary<string, string> dictStations = stationsList.ToDictionary(prop => prop.stop_id, prop => prop.stop_name);

                TripUpdateCollection tuc = tripUpdateList.Where(x => x.id.Equals(tripID)).FirstOrDefault();
                if (tuc == null) continue; 
                                
                string nexsStationAbbr = tuc.trip_update.stop_time_update[0].stop_id;

                string priorStationAbbr = (tuc.trip_update.stop_time_update.Count < 2) ? string.Empty : tuc.trip_update.stop_time_update[1].stop_id;

                if (nexsStationAbbr.Equals("CUS")) continue;

                string stopTimesJSON = j.Get_GTFS_Response(j.METRA_API_URL + "schedule/stop_times/" + tripID);
                List<StopOnTrip> stopTimesList = JsonConvert.DeserializeObject<List<StopOnTrip>>(stopTimesJSON);
                if (stopTimesList == null) continue;

                if (stopTimesList[0].stop_id.Equals("CUS"))    //INBOUND - DOES not start at Chicago Union Station
                {
                    continue;
                }

                StopOnTrip westernAve = stopTimesList.Where(x => x.stop_id.Equals("WESTERNAVE")).FirstOrDefault();
                if (westernAve == null) continue;

                int delayInSeconds = tuc.trip_update.stop_time_update[0].arrival.delay;

                string Delay = string.Empty;
                TimeSpan ts = new TimeSpan(0, 0, 0);

                if (delayInSeconds > 0)
                {
                    ts = TimeSpan.FromSeconds(delayInSeconds);
                    Delay = (int)ts.TotalMinutes + " min late" + Environment.NewLine;
                }
                  
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
                      
                TimeSpan tsArrivesIn = dtArrivalTimeOnWestern.Subtract(dtUpdateTime);
                tsArrivesIn = RoundToNearest(tsArrivesIn, TimeSpan.FromMinutes(1));

                int arrivesInMinutes = (int)tsArrivesIn.Minutes;

                //Bug on server - after 6pm, 24 hrs gets added to the time.
                if (arrivesInMinutes >= 1440)
                {
                    arrivesInMinutes = arrivesInMinutes - 1440;
                }

                if (arrivesInMinutes < 0)
                {
                    arrivesInMinutes = 0;
                }

                arrivalTimeOnWestern = dtArrivalTimeOnWestern.ToString("HH:mm");
                string currentNextStop = dictStations[nexsStationAbbr];

                string description = Delay +
                        "Next Stop: " + currentNextStop;
                

                Location loc = new Location
                {
                    LocationID = Convert.ToInt32(positionList[i].id),

                    TripID = tripID,

                    RouteNumber = lineID + ", Route " + routeID,

                    Lat = positionList[i].vehicle.position.latitude.ToString(),

                    Long = positionList[i].vehicle.position.longitude.ToString(),

                    ArrivesIn = arrivesInMinutes + " min",

                    ArrivalTime = dtArrivalTimeOnWestern.ToString("HH:mm"),

                    Description = description,

                    ImagePath = "https://png.icons8.com/material/2x/train.png",

                    TripURL = "https://metrarail.com/maps-schedules/train-lines/" + lineID + "/trip/" + tripID
                };

                lstLocations.Add(loc); 
            }

            lstLocations = lstLocations.OrderBy(x => x.ArrivalTime).ToList();
            return lstLocations;
        }

        public static TimeSpan RoundToNearest(TimeSpan a, TimeSpan roundTo)
        {
            long ticks = (long)(Math.Round(a.Ticks / (double)roundTo.Ticks) * roundTo.Ticks);
            return new TimeSpan(ticks);
        }
    }


}