using System;
using System.Collections.Generic;


using System.Text;

namespace iSchool.Application.ReponseModels
{

    public class QqMapReverseAddress
    {
        public int status { get; set; }
        public string message { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public string title { get; set; }
        public Location location { get; set; }
        public Ad_Info ad_info { get; set; }
        public Address_Components address_components { get; set; }
        public float similarity { get; set; }
        public int deviation { get; set; }
        public int reliability { get; set; }
        public int level { get; set; }
    }

    public class Location
    {
        public float lng { get; set; }
        public float lat { get; set; }
    }

    public class Ad_Info
    {
        public string adcode { get; set; }
    }

    public class Address_Components
    {
        public string province { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string street { get; set; }
        public string street_number { get; set; }
    }
}
