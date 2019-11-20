using RetailUseCase_OCR_.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Spire.Barcode;
using System.Dynamic;
using Newtonsoft.Json;

namespace RetailUseCase_OCR_.Controllers
{
    public class HomeController : Controller
    {
        const string subscriptionKey = "e05f1a77f0b541caba2f803611b1fa01";

        const string uriBase = "http://localhost:62566/api/OCRrequest";

        public string filePath;
        public string filePath1;
        public string imageFilePath;
        Retailprice rp = new Retailprice();
        List<string> extractedline = new List<string>();
        public ActionResult Index()
        {
            return View();
        }

            static string responsetext;

            static string Subscriptionkey()
            {
                return System.Configuration.ConfigurationManager
                   .AppSettings["Subscription-Key"];
            }
            static string RequestParameters()
            {
                return System.Configuration.ConfigurationManager
                   .AppSettings["RequestParameters"];
            }

            static string ReadURI()
            {
                return System.Configuration.ConfigurationManager
                   .AppSettings["APIuri"];
            }
            static string Contenttypes()
            {
                return System.Configuration.ConfigurationManager
                   .AppSettings["Contenttypes"];
            }


        static byte[] GetByteArray(string LocalimageFilePath)
        {
            FileStream ImagefileStream = new FileStream(LocalimageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader ImagebinaryReader = new BinaryReader(ImagefileStream);
            byte[] bytedata=ImagebinaryReader.ReadBytes((int) ImagefileStream.Length);
            ImagefileStream.Close();
            return bytedata;
        }

        [HttpPost]
        public async Task<JsonResult> Upload(string baseData)
        {
            if (HttpContext.Request.Files.AllKeys.Any())
            {
                for (int i = 0; i <= HttpContext.Request.Files.Count; i++)
                {
                    var file = HttpContext.Request.Files["files" + i];
                    if (file != null)
                    {
                        var fileSavePath = Path.Combine(Server.MapPath("/Images"), file.FileName);
                        filePath1 = Path.Combine(Server.MapPath("/Files"), file.FileName);
                        Session["filePath"] = fileSavePath;
                        filePath = fileSavePath;
                       
                        try
                        {
                            if (System.IO.File.Exists(filePath))
                            { System.IO.File.Delete(filePath);
                                
                                
                                //System.IO.File.Copy(Server.MapPath("/Images"), Server.MapPath("/Files"));
                            }
                            if (System.IO.File.Exists(filePath1))
                            {
                                System.IO.File.Delete(filePath1);
                                

                            }
                            file.SaveAs(fileSavePath);
                            file.SaveAs(filePath1);

                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }

            var APIclient = new HttpClient();
            
            try
            {
                //ComputerVisionAPIclient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Subscriptionkey());
                //string requestParameters = RequestParameters();
                string APIuri = ReadURI()+ filePath1;
                HttpResponseMessage myresponse;
                MultipartFormDataContent form = new MultipartFormDataContent();
                //byte[] byteData = GetByteArray(filePath);
                //var content = new ByteArrayContent(byteData);
                var fileStream = new FileStream(filePath, FileMode.Open);
                form.Add(new StreamContent(fileStream));
                //content.Headers.ContentType = new MediaTypeHeaderValue(Contenttypes());
                myresponse = await APIclient.PostAsync(APIuri, form);
                myresponse.EnsureSuccessStatusCode();
                responsetext = await myresponse.Content.ReadAsStringAsync();
                rp=JsonConvert.DeserializeObject<Retailprice>(responsetext);
                //rp=ExtractPrintedWords(responsetext);
                using (var reader = new StreamReader(@"C:\Users\devNXT\Desktop\RetailUseCase(OCR)\Content\RetailPriceBook.csv"))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                       
                        extractedline.Add(line);
                                               
                    }
                    
                        foreach (string str in extractedline)
                        {
                            if (str.Contains(rp.Productname))
                            {
                                var values = str.Split(',');
                                bool resp = rp.Price.Equals(values[1]);
                                rp.Validation = resp;
                            }
                        }
                }

            }

            catch (Exception e)
            {
                EventLog.WriteEntry("Text Recognition Error", e.Message + "Trace" + e.StackTrace, EventLogEntryType.Error, 121, short.MaxValue);
            }
         return   Json(rp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BarCodeUpload(string baseData)
        {
            string[] result = new string[100];
            if (HttpContext.Request.Files.AllKeys.Any())
            {
                for (int i = 0; i <= HttpContext.Request.Files.Count; i++)
                {
                    var file = HttpContext.Request.Files["files" + i];
                    if (file != null)
                    {
                        var fileSavePath = Path.Combine(Server.MapPath("/BarCodes"), file.FileName);

                        filePath = fileSavePath;
                       
                        try
                        {
                            if (System.IO.File.Exists(filePath))
                            { System.IO.File.Delete(filePath); }
                            file.SaveAs(fileSavePath);
                            FileStream ImagefileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            result = BarcodeScanner.Scan(ImagefileStream, BarCodeType.QRCode, true);
                          
                        }
                        catch(Exception e)
                        {
                            return Json(e.Message.ToString());
                        }
                    }
                }
            }
            return Json(result,JsonRequestBehavior.AllowGet);
        }


        //static Retailprice ExtractPrintedWords(string jsonResponse)
        //{
        //    List<string> listDistinctWords = new List<string>();
        //    Retailprice objrp = new Retailprice();
        //    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);

        //    foreach (var region in jsonObj.regions)
        //    {
        //        foreach (var line in region.lines)
        //        {
        //            foreach (var word in line.words)
        //            {

        //                listDistinctWords.Add(Convert.ToString(word.text));
        //            }
        //        }
        //    }

        //    if (listDistinctWords.Contains("Yogurt"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2] + " " + listDistinctWords[3];
        //        string productname2 = listDistinctWords[10] + " " + listDistinctWords[11] + " " + listDistinctWords[12] + " " + listDistinctWords[13];
        //        string RetailPrice1 = listDistinctWords[9];
        //        string RetailPrice2 = listDistinctWords[19];
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }
        //    else if (listDistinctWords.Contains("Beefsteak") && listDistinctWords.Contains("Tomatoes,"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2];
        //        productname1 = productname1.Replace(",", "");
        //        string price = listDistinctWords[7];
        //        string RetailPrice1 = listDistinctWords[6] + price;
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;


        //    }

        //    else if (listDistinctWords.Contains("Red") && listDistinctWords.Contains("Potatoes,"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1];

        //        string RetailPrice1 = listDistinctWords[6];
        //        productname1 = productname1.Replace(",", "");
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }

        //    else if (listDistinctWords.Contains("Vine,") && listDistinctWords.Contains("Tomatoes"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2] + " " + listDistinctWords[3];

        //        string RetailPrice1 = listDistinctWords[9];
        //        productname1 = productname1.Replace(",", "");
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }

        //    else if (listDistinctWords.Contains("Russet") && listDistinctWords.Contains("Potatoes,"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1] + " " + listDistinctWords[2];

        //        string RetailPrice1 = listDistinctWords[6];
        //        productname1 = productname1.Replace(",", "");
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }

        //    else if (listDistinctWords.Contains("Roma") && listDistinctWords.Contains("Tomatoes,"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1];

        //        string RetailPrice1 = listDistinctWords[5];
        //        productname1 = productname1.Replace(",", "");
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }

        //    else if (listDistinctWords.Contains("White") && listDistinctWords.Contains("Potatoes,"))
        //    {
        //        string productname1 = listDistinctWords[0] + " " + listDistinctWords[1];

        //        string RetailPrice1 = listDistinctWords[5];
        //        productname1 = productname1.Replace(",", "");
        //        objrp.Price = RetailPrice1;
        //        objrp.Productname = productname1;

        //    }


        //    return objrp;

        //}
        

    }
}