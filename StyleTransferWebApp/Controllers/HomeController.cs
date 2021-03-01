using StyleTransferWebApp.Helpers;
using StyleTransferWebApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace StyleTransferWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message = null)
        {
            // get/save user id
            string userID;
            if (Request.Cookies["userInfo"] != null)
            {
                userID = Request.Cookies["userInfo"]["userID"];
            }
            else
            {
                HttpCookie userInfo = new HttpCookie("userInfo");
                userID = userInfo["userID"] = Guid.NewGuid().ToString();
                userInfo.Expires = DateTime.Now.AddDays(180);
                Response.Cookies.Add(userInfo);
            }


            GeneralHelper.GetResultsForUser(userID);


            // set response message or just pass it as null/empty
            ViewData["responseMessage"] = message;

            return View(new Home { responseMessage = message });
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

        public ActionResult UploadContentImage(HttpPostedFileBase file)
        {
            // save image to session
            MyImage image = new MyImage(Image.FromStream(file.InputStream, true, true), file.FileName);
            Session["content_image"] = image;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult UploadStyleImage(HttpPostedFileBase file)
        {
            // save image to session
            MyImage image = new MyImage(Image.FromStream(file.InputStream, true, true), file.FileName);
            Session["style_image"] = image;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult StartStyleTransfer()
        {
            MyImage contentImage = (MyImage)Session["content_image"];
            MyImage styleImage = (MyImage)Session["style_image"];

            bool savedSuccessfully = true;
            string responseMessage = "";
            
            try
            {
                if (contentImage == null || styleImage == null)
                {
                    throw new Exception("Content and style images have to be set!");
                }

                string inputPath = WebConfigurationManager.AppSettings["input_folder"];
                string datetime = DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");
                string userID = Request.Cookies["userInfo"]["userID"];
                string jobID = contentImage.name + "-" + styleImage.name;

                // create the job folder if it doesn't exist
                string jobFolderName = datetime + "_" + userID + "_" + jobID;
                string jobFolderPath = Path.Combine(inputPath, jobFolderName);
                if (!Directory.Exists(jobFolderPath))
                {
                    Directory.CreateDirectory(jobFolderPath);
                }
                else
                {
                    // no need to do anything, already created
                    return RedirectToAction("Index", "Home");
                }

                // create paths for images
                string contentImagePath = Path.Combine(jobFolderPath, "content" + contentImage.extension);
                string styleImagePath = Path.Combine(jobFolderPath, "style" + styleImage.extension);

                // save images to the job folder
                contentImage.image.Save(contentImagePath);
                styleImage.image.Save(styleImagePath);
            }
            catch (Exception ex)
            {
                savedSuccessfully = false;
                responseMessage = ex.Message;
            }

            if (savedSuccessfully)
            {
                return RedirectToAction("Index", new { message = "Style trasnfer is in progress. Refresh the page in about two minutes to see the results." });
            }
            else
            {
                return RedirectToAction("Index", new { message = "There has been a problem starting style transfer. Message: " + responseMessage });
            }

        }



    }
}