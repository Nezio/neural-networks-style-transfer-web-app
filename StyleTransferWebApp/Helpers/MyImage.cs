using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace StyleTransferWebApp.Helpers
{
    public class MyImage
    {
        public Image image;
        public string name;
        public string extension;

        public MyImage(Image image, string fullName)
        {
            this.image = image;
            name = Path.GetFileNameWithoutExtension(fullName);
            extension = Path.GetExtension(fullName);
        }
    }
}