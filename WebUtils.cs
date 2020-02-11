using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace RemoteComputerControl
{
    public class WebUtils
    {
        public static string makePOSTRequest(string URL, string rawData)
        {
            try
            {
                string[] nameValues = rawData.Split('&');
                NameValueCollection nameValueCollection = new NameValueCollection();
                foreach (string rawNameValue in nameValues)
                {
                    string[] nameValue = rawNameValue.Split(new char [] {'='}, 2);
                    nameValueCollection.Add(nameValue[0], nameValue[1]);
                }

                using (WebClient webClient = new WebClient())
                {
                    byte[] bytesOfResponse = webClient.UploadValues(URL, nameValueCollection);
                    return Encoding.UTF8.GetString(bytesOfResponse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[WebUtils::makePOSTRequest(string, string)] Ошибка: " + e.Message);
                throw e;
            }
        }

        public static string makeGETRequest(string URL, string data)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    return webClient.DownloadString(URL + "?" + data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[WebUtils::makeGETRequest(string, string)] Ошибка: " + e.Message);
                throw e;
            }
        }

        public static void downloadFile(string URL, string filename)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(URL, filename);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[WebUtils::downloadFile(string, string)] Ошибка: " + e.Message);
                throw e;
            }
        }
    }
}
