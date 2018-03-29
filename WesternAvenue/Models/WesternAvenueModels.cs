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
                string routeID = positionList[i].vehicle.trip.route_id;                

                //Create dictionary to resolve station abbrevs to names
                string stationsJSON = j.Get_GTFS_Response(j.METRA_API_URL + "schedule/stops");
                List<Station> stationsList = JsonConvert.DeserializeObject<List<Station>>(stationsJSON);
                Dictionary<string, string> dictStations = stationsList.ToDictionary(prop => prop.stop_id, prop => prop.stop_name);

                TripUpdateCollection tuc = tripUpdateList.Where(x => x.id.Equals(tripID)).FirstOrDefault();
                if (tuc == null) continue; 

                string lastStationAbbr = tuc.trip_update.stop_time_update[0].stop_id;
                if (lastStationAbbr.Equals("CUS")) continue;

                int delayInSeconds = tuc.trip_update.stop_time_update[0].arrival.delay;

                string Delay = string.Empty;
                TimeSpan ts = new TimeSpan(0, 0, 0);

                if (delayInSeconds > 0)
                {
                    ts = TimeSpan.FromSeconds(delayInSeconds);
                    Delay = (int)ts.TotalMinutes + " min late" + Environment.NewLine;
                }
                 
                DateTime dtAtLastStation = tuc.trip_update.stop_time_update[0].departure.time.low;
                  
                string timeAtNextStation = dtAtLastStation.ToString("HH:mm");

                string stopTimesJSON = j.Get_GTFS_Response(j.METRA_API_URL + "schedule/stop_times/" + tripID);
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
                     
                    string currentDateTimeJSON = j.Get_API_Response(j.CURRENT_TIME_API_URL);
                    DateTimeModels currentDateTime = JsonConvert.DeserializeObject<DateTimeModels>(currentDateTimeJSON);
                    
                    DateTime dtUpdateTime = DateTime.Parse(currentDateTime.formatted);

                    TimeSpan tsArrivesIn = dtArrivalTimeOnWestern.Subtract(dtUpdateTime);
                    int arrivesInMinutes = (int)tsArrivesIn.TotalMinutes;
                     
                    arrivalTimeOnWestern = dtArrivalTimeOnWestern.ToString("HH:mm");
                    string currentNextStop = dictStations[lastStationAbbr];

                    string description = string.Empty;
                    if ((currentNextStop.Equals("Western Ave")) && (arrivalTimeOnWestern.Equals(timeAtNextStation)))
                    {
                        description = Delay;
                    }
                    else
                    {
                        description = Delay + 
                            "Next Stop: " + dictStations[lastStationAbbr] + ", " + timeAtNextStation;
                    }

                    if (positionList[i].vehicle.position.latitude < 41.8888) continue;  //Exclude if already past the Western Avenue Station, but had not updated next station yet

                    Location loc = new Location
                    {
                        LocationID = Convert.ToInt32(positionList[i].id),

                        TripID = tripID,

                        Lat = positionList[i].vehicle.position.latitude.ToString(),

                        Long = positionList[i].vehicle.position.longitude.ToString(),

                        ArrivesIn = arrivesInMinutes + " min",

                        ArrivalTime = dtArrivalTimeOnWestern.ToString("HH:mm"),

                        Description = description,

                        ImagePath = "https://png.icons8.com/material/2x/train.png",

                        TripURL = "https://metrarail.com/maps-schedules/train-lines/" + routeID + "/trip/" + tripID
                    };

                    lstLocations.Add(loc);
                }
            }
              
            return lstLocations;
        }
    }


}