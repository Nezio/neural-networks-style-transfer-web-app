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
            StyleTransferViewModel stViewModel = new StyleTransferViewModel();

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

            // set user results to return to view (can be null if there are no results yet)
            stViewModel.styleTransferUserResults = GeneralHelper.GetResultsForUser(userID);

            // append unprocessed job folders to the results list
            stViewModel.styleTransferUserResults.InsertRange(0, GeneralHelper.GetUnprocessedImagesForUser(userID));

            // set response message or just pass it as null/empty
            stViewModel.responseMessage = message;


            return View(stViewModel);
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

            string numberInQueue = "";
            string eta = "";

            string jobFolderPath = null;

            try
            {
                if (contentImage == null || styleImage == null)
                {
                    throw new Exception("Content and style images have to be set!");
                }

                string inputPath = WebConfigurationManager.AppSettings["input_folder"];
                inputPath = Server.MapPath(inputPath);
                string datetime = DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");
                string userID = Request.Cookies["userInfo"]["userID"];
                string jobID = contentImage.name + "-" + styleImage.name;

                // create the job folder if it doesn't exist
                string jobFolderName = datetime + "_" + userID + "_" + jobID;
                jobFolderPath = Path.Combine(inputPath, jobFolderName);
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

                // get queue number and eta
                numberInQueue = Directory.GetDirectories(inputPath).Length.ToString();
                eta = Math.Ceiling(Directory.GetDirectories(inputPath).Length * 1.5).ToString();
            }
            catch (Exception ex)
            {
                // delete job folder if created
                if (!string.IsNullOrEmpty(jobFolderPath) && Directory.Exists(jobFolderPath))
                {
                    Directory.Delete(jobFolderPath, true);
                }

                savedSuccessfully = false;
                responseMessage = ex.Message;
            }

            if (savedSuccessfully)
            {
                // clear session
                Session["content_image"] = null;
                Session["style_image"] = null;

                string msg = "Style trasnfer is in progress. You are number " + numberInQueue + " in the queue. Refresh the page in about " + eta + " minutes to see the results.";
                return RedirectToAction("Index", new { message = msg });
            }
            else
            {
                return RedirectToAction("Index", new { message = "There has been a problem starting style transfer. Message: " + responseMessage });
            }

        }



    }
}