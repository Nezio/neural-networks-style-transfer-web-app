using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StyleTransferWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // get/save user id
            string userid;
            if (Request.Cookies["userid"] != null)
            {
                userid = Request.Cookies["userid"].Value.ToString();
            }
            else
            {
                Response.Cookies["userid"].Value = Guid.NewGuid().ToString();
            }


            return View();
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
            Session["content_image"] = file;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult UploadStyleImage(HttpPostedFileBase file)
        {
            // save image to session
            Session["style_image"] = file;

            return RedirectToAction("Index", "Home");
        }



        public ActionResult SaveUploadedFile(IEnumerable<HttpPostedFileBase> files)
        {
            var file = files.First();
            // the other one
            if (file != null)
            {
                string pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(
                                       Server.MapPath("~/images/profile"), pic);
                // file is uploaded
                //file.SaveAs(path);


            }

            // the first one
            bool SavedSuccessfully = true;
            string fName = "";
            try
            {
                //loop through all the files
                foreach (var xfile in files)
                {

                    //Save file content goes here
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0)
                    {
                        ;

                        /*var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\", Server.MapPath(@"\")));

                        string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "imagepath");

                        var fileName1 = Path.GetFileName(file.FileName);

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, file.FileName);
                        file.SaveAs(path);*/

                    }

                }

            }
            catch (Exception ex)
            {
                SavedSuccessfully = false;
            }


            /* if (SavedSuccessfully)
             {
                 return RedirectToAction("Index", new { Message = "All files saved successfully" });
             }
             else
             {
                 return RedirectToAction("Index", new { Message = "Error in saving file" });
             }*/
            return RedirectToAction("Index", "Home");
        }


    }
}