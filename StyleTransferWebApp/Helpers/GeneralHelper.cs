using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace StyleTransferWebApp.Helpers
{
    public static class GeneralHelper
    {
        public static List<StyleTransferResult> GetResultsForUser(string userID)
        {
            var result = new List<StyleTransferResult>();

            // check if user folder exists in the output folder and get image paths if it does
            string outputPath = WebConfigurationManager.AppSettings["output_folder"];
            outputPath = HostingEnvironment.MapPath(outputPath);
            string userOutputFolder = Path.Combine(outputPath, userID);
            if (!Directory.Exists(userOutputFolder))
            {
                // user doesn't have his folder yet
                return null;
            }

            // get all job result folders for user
            var resultFolderPaths = Directory.GetDirectories(userOutputFolder);

            // return if there are no folders
            if (resultFolderPaths.Length <= 0)
            {
                // user has no result folders yet
                return null;
            }

            // get image paths for each result folder that user has
            foreach (var resultFolder in resultFolderPaths)
            {
                StyleTransferResult styleTransferResult = new StyleTransferResult();

                // get input folder from inside the result folder (this folder should have content and style images used during the style transfer)
                string resultInputFolder = Path.Combine(resultFolder, "input");                

                // get content and style image paths
                if (Directory.Exists(resultInputFolder))
                {
                    // set default images to 404
                    styleTransferResult.contentImage = "Content/Images/404.jpg";
                    styleTransferResult.styleImage = "Content/Images/404.jpg";

                    // get content image path
                    var contentImageArray = Directory.GetFiles(resultInputFolder, "*content*");
                    if (contentImageArray.Length > 0)
                    {
                        string contentImagePath = contentImageArray.First();
                        string contentImageRelativePath = contentImagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                        styleTransferResult.contentImage = contentImageRelativePath.Replace("\\", "/");
                    }

                    // get style image path
                    var styleImageArray = Directory.GetFiles(resultInputFolder, "*style*");
                    if (styleImageArray.Length > 0)
                    {
                        string styleImagePath = styleImageArray.First();
                        string styleImageRelativePath = styleImagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                        styleTransferResult.styleImage = styleImageRelativePath.Replace("\\", "/");
                    }
                }

                // get generated images
                var generatedImagesArray = Directory.GetFiles(resultFolder);
                if (generatedImagesArray.Length > 0)
                {
                    foreach (var generatedImagePath in generatedImagesArray)
                    {
                        string generatedImageRelativePath = generatedImagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                        styleTransferResult.generatedImages.Add(generatedImageRelativePath.Replace("\\", "/"));
                    }
                }
                else
                {
                    // set WIP image if there are no generated images yet
                    styleTransferResult.generatedImages.Add("Content/Images/WIP.jpg");
                }

                // add results of one job folder to the list that contains all resulting job folders for user
                result.Add(styleTransferResult);
            }

            // show newest on top
            result.Reverse();

            return result;
        }

        public static List<StyleTransferResult> GetUnprocessedImagesForUser(string userID)
        {
            var result = new List<StyleTransferResult>();

            // check if there are any job folders for this user in the input folder
            string inputPath = WebConfigurationManager.AppSettings["input_folder"];
            inputPath = HostingEnvironment.MapPath(inputPath);
            var jobFolderPaths = Directory.GetDirectories(inputPath, "*" + userID + "*");

            // return if there are no folders
            if (jobFolderPaths.Length <= 0)
            {
                // user has no unprocessed job folders
                return null;
            }

            // get image paths for each result folder that user has
            foreach (var jobFolder in jobFolderPaths)
            {
                StyleTransferResult styleTransferResult = new StyleTransferResult();

                // set default images (404 and InQueue)
                styleTransferResult.contentImage = "Content/Images/404.jpg";
                styleTransferResult.styleImage = "Content/Images/404.jpg";
                styleTransferResult.generatedImages.Add("Content/Images/InQueue.jpg");

                // get content image path
                var contentImageArray = Directory.GetFiles(jobFolder, "*content*");
                if (contentImageArray.Length > 0)
                {
                    string contentImagePath = contentImageArray.First();
                    string contentImageRelativePath = contentImagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                    styleTransferResult.contentImage = contentImageRelativePath.Replace("\\", "/");
                }

                // get style image path
                var styleImageArray = Directory.GetFiles(jobFolder, "*style*");
                if (styleImageArray.Length > 0)
                {
                    string styleImagePath = styleImageArray.First();
                    string styleImageRelativePath = styleImagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                    styleTransferResult.styleImage = styleImageRelativePath.Replace("\\", "/");
                }

                // add results of one job folder to the list
                result.Add(styleTransferResult);
            }

            // show newest on top
            result.Reverse();

            return result;
        }

        public static void SaveImage(Image image, string path)
        {
            var bitmap = new Bitmap(image);
            
            bitmap.Save(path);
        }



    }
}