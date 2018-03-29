using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WesternAvenue.Models
{
    public class DateTimeModels
    { 
            public string status { get; set; }
            public string message { get; set; }
            public string countryCode { get; set; }
            public string countryName { get; set; }
            public string zoneName { get; set; }
            public string abbreviation { get; set; }
            public int gmtOffset { get; set; }
            public string dst { get; set; }
            public int dstStart { get; set; }
            public int dstEnd { get; set; }
            public string nextAbbreviation { get; set; }
            public int timestamp { get; set; }
            public string formatted { get; set; } 
    }
}