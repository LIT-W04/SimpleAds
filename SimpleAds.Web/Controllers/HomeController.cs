using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimpleAds.Data;
using System.IO;
using SimpleAds.Web.Models;

namespace SimpleAds.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            SimpleAdDb db = new SimpleAdDb(Properties.Settings.Default.ConStr);
            return View(new HomePageViewModel
            {
                Ads = db.GetAds()
            });
        }

        public ActionResult NewAd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewAd(SimpleAd ad, IEnumerable<HttpPostedFileBase> adImages)
        {
            SimpleAdDb db = new SimpleAdDb(Properties.Settings.Default.ConStr);
            ad.Images = adImages?
                .Where(i => i != null)
                .Select(i => new Image { FileName = SaveFile(i) });
            db.AddSimpleAd(ad);
            string ids = "";
            HttpCookie cookie = Request.Cookies["AdIds"];
            if (cookie != null)
            {
                ids = $"{cookie.Value},";
            }
            ids += ad.Id;
            Response.Cookies.Add(new HttpCookie("AdIds", ids));
            return Redirect("/");
        }

        public ActionResult AdDetails(int adId)
        {
            SimpleAdDb db = new SimpleAdDb(Properties.Settings.Default.ConStr);
            var viewModel = new AdDetailsViewModel
            {
                Ad = db.GetById(adId)
            };
            var cookie = Request.Cookies["AdIds"];
            if (cookie != null)
            {
                var ids = cookie.Value.Split(',').Select(i => int.Parse(i));
                viewModel.ShowDeleteButton = ids.Any(i => i == adId);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult DeleteAd(int id)
        {
            SimpleAdDb db = new SimpleAdDb(Properties.Settings.Default.ConStr);
            db.Delete(id);
            return Redirect("/");
        }

        private string SaveFile(HttpPostedFileBase file)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            file.SaveAs($"{Server.MapPath("~/Images")}\\{fileName}");
            return fileName;
        }

    }
}