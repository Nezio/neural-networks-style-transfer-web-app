using StyleTransferWebApp.Helpers;
using StyleTransferWebApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
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

            // preppend unprocessed job folders to the results list
            var unprocessedImages = GeneralHelper.GetUnprocessedImagesForUser(userID);
            if (unprocessedImages != null)
            {
                stViewModel.styleTransferUserResults.AddRange(unprocessedImages);
            }

            // set user results to return to view (can be null if there are no results yet)
            var resultImages = GeneralHelper.GetResultsForUser(userID);
            if (resultImages != null)
            {
                stViewModel.styleTransferUserResults.AddRange(resultImages);
            }            

            // set response message or just pass it as null/empty
            stViewModel.responseMessage = message;


            return View(stViewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

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
                GeneralHelper.SaveImage(contentImage.image, contentImagePath);
                GeneralHelper.SaveImage(styleImage.image, styleImagePath);

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

            if (!savedSuccessfully)
            {
                return RedirectToAction("Index", new { message = "There has been a problem starting style transfer. Message: " + responseMessage });    
            }

            // clear session
            Session["content_image"] = null;
            Session["style_image"] = null;

            string msg = "Style trasnfer is in progress. You are number " + numberInQueue + " in the queue. The image will be complete in about " + eta + " minutes, but you will be able to see the early results before that.";

            return RedirectToAction("Index", new { message = msg });
        }
        public JsonResult DeleteOutputFolder(string folder)
        {
            bool deletedSuccessfully = false;
            string responseMessage = "";

            try
            {
                // delete the folder
                string outputPath = WebConfigurationManager.AppSettings["output_folder"];
                outputPath = HostingEnvironment.MapPath(outputPath);
                string userID = Request.Cookies["userInfo"]["userID"];
                string userOutputFolder = Path.Combine(outputPath, userID);
                string folderToDelete = Path.Combine(userOutputFolder, folder);

                Directory.Delete(folderToDelete, true);

                deletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                deletedSuccessfully = false;
                responseMessage = ex.Message;
            }

            if (!deletedSuccessfully)
            {
                string msg = "Delete failed! Reason: " + responseMessage;
                return this.Json(new { success = false, message = msg});
            }
            
            responseMessage = "Delete successful!";
            return this.Json(new { success = true, message = responseMessage });
        }

        [HttpPost]
        public JsonResult GetGeneratedImagesFromFolders(List<string> wipFolders, List<string> inqueueFolders)
        {
            // initialize result dictionaries
            IDictionary<string, string> newGeneratedImages = new Dictionary<string, string>();
            IDictionary<string, bool> folderStartedProcessing = new Dictionary<string, bool>();

            // initialize the lists in case they are empty
            if(wipFolders == null)
            {
                wipFolders = new List<string>();
            }
            if (inqueueFolders == null)
            {
                inqueueFolders = new List<string>();
            }

            try
            {
                string userID = Request.Cookies["userInfo"]["userID"];
                var resultImages = GeneralHelper.GetResultsForUser(userID);

                // get last generated image for wip folders (if no images it will be the WIP image)
                foreach (var wipFolder in wipFolders)
                {
                    var lastGeneratedImage = resultImages.Where(r => r.contentImage.Contains(wipFolder)).First().generatedImages.Last();
                    newGeneratedImages.Add(wipFolder, lastGeneratedImage);
                }

                // check if folder exists and return true/false (if true it will be updated to a WIP image
                foreach (var inqueueFolder in inqueueFolders)
                {
                    var newWIPFolders = resultImages.Where(r => r.contentImage.Contains(inqueueFolder));

                    if (newWIPFolders.Count() == 0)
                    {
                        // this folder has not started processing, yet
                        folderStartedProcessing.Add(inqueueFolder, false);
                    }
                    else
                    {
                        // this folder has started processing and is now a WIP folder
                        folderStartedProcessing.Add(inqueueFolder, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, exception = ex.Message });
            }
            
            return this.Json(new { success = true, newGeneratedImages, folderStartedProcessing});
        }



    }
}