using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    public class JSONUtils
    {
        public static Dictionary<string, string> convertJsonToStringDictionary(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (Exception e) 
            {
                Console.WriteLine("[JSONUtils::convertJsonToStringDictionary(string)] Ошибка: " + e.Message);
                throw e;
            }
        }

        public static List<Dictionary<string, string>> convertJsonArrayToListWithKeyValueDictionary(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("[JSONUtils::convertJsonArrayToListWithKeyValueDictionary(string)] Ошибка: " + e.Message);
                throw e;
            }
        }
    }
}
