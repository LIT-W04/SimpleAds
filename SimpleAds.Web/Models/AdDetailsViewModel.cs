using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleAds.Data;

namespace SimpleAds.Web.Models
{
    public class AdDetailsViewModel
    {
        public SimpleAd Ad { get; set; }
        public bool ShowDeleteButton { get; set; }
    }
}