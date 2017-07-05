using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleAds.Data;

namespace SimpleAds.Web.Models
{
    public class HomePageViewModel
    {
        public IEnumerable<SimpleAd> Ads { get; set; }
    }
}