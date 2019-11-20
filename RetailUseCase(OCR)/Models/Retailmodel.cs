using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RetailUseCase_OCR_.Models
{
    public class RetailModel
    {
        public string boundingBox { get; set; }
        public string words { get; set; }
        public string text { get; set; }
    }
}