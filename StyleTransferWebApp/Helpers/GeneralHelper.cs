using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace StyleTransferWebApp.Helpers
{
    public static class GeneralHelper
    {
        public static void GetResultsForUser(string userID)
        {
            // check if user folder exists in the output folder and get image paths if it does
            string outputPath = WebConfigurationManager.AppSettings["output_folder"];
            string userOutputFolder = Path.Combine(outputPath, userID);
            if (Directory.Exists(userOutputFolder))
            {
                // get all job folders
                var resultFolderPaths = Directory.GetDirectories(userOutputFolder);
                if (resultFolderPaths.Length > 0)
                {

                }
            }


            
        }



    }
}