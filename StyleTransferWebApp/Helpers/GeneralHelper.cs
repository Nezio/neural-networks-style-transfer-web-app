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
            if (Directory.Exists(userOutputFolder))
            {
                // get all job result folders for user
                var resultFolderPaths = Directory.GetDirectories(userOutputFolder);

                // return if there are no folders
                if (resultFolderPaths.Length <= 0)
                {
                    // user has no result folders yet
                    return null;
                }

                foreach (var resultFolder in resultFolderPaths)
                {

                    string resultFolderRelativePath = resultFolder.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);

                    StyleTransferResult styleTransferResult = new StyleTransferResult();
                    //styleTransferResult.contentImage = resultFolderRelativePath + "\input\content.???"

                }


            }

            //String RelativePath = AbsolutePath.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);


            return result;
        }



    }
}