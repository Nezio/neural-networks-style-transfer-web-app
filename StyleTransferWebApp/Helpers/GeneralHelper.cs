using System;
using System.Collections.Generic;
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
                string resultFolderRelativePath = resultFolder.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);

                StyleTransferResult styleTransferResult = new StyleTransferResult();

                // get input folder from inside the result folder (this folder should have content and style images used during the style transfer)
                string resultInputFolder = Path.Combine(resultFolder, "input");

                // get content and style image paths
                if (Directory.Exists(resultInputFolder))
                {
                    // TODO: set 404 images as default

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

                // add results of one job folder to the list that contains all resulting job folders for user
                result.Add(styleTransferResult);
            }

            return result;
        }



    }
}