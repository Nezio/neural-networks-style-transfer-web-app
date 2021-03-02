using StyleTransferWebApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StyleTransferWebApp.Models
{
    public class StyleTransferViewModel
    {
        public string responseMessage { get; set; }
        public List<StyleTransferResult> styleTransferUserResults;

    }
}