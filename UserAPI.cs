using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RemoteComputerControl
{
    public class UserAPI
    {
        public static readonly int CLIENT_API_VERSION = 1;
        public static string BASE_URL = Properties.Settings.Default.BASE_URL;
        private int UID = -1;
        
        public bool isUserRegistered()
        {
            if (Properties.Settings.Default.UUID > 0)
            {
                this.UID = Properties.Settings.Default.UUID;
                return true;
            }
            return false;
        }

        public List<BlockingSoft> getBlockedSoftware()
        {
            try
            {
                string json = WebUtils.makeGETRequest(BASE_URL + "/deceiver/getBlockedSoftware", "userId=" + UID + "&apiVersion=" + CLIENT_API_VERSION);
                List<BlockingSoft> expiredSoftware = new List<BlockingSoft>();
                foreach (Dictionary<string, string> soft in JSONUtils.convertJsonArrayToListWithKeyValueDictionary(json))
                {
                    DateTime dt = DateTime.Parse(soft["date"]);
                    DateTime now = DateTime.Now;
                    double timespan = (dt - now).TotalDays;
                    if (timespan < 0.0d)
                    {
                        expiredSoftware.Add(new BlockingSoft(soft["program"], dt));
                    }
                }
                this.saveBlockedSoftToStorage(expiredSoftware);
                return expiredSoftware;
            }
            catch (Exception e)
            {
                if (e.GetType().Name == "WebException")
                {
                    WebException we = (WebException)e;
                    HttpWebResponse response = (System.Net.HttpWebResponse)we.Response;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UserUnauthrizedException("User is unauthorized!", e);
                    }

                    if (response.StatusCode == HttpStatusCode.HttpVersionNotSupported)
                    {
                        throw new APIUpgradeRequired("Client API update required!", e);
                    }
                }
                throw e;
            }
        }

        public string updateClient()
        {
            try
            {
                int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(2019, 1, 25))).TotalMinutes;
                string filename = AppDomain.CurrentDomain.BaseDirectory + "RCC_" + timestamp + ".exe";
                WebUtils.downloadFile(BASE_URL + "/deceiver/doUpdate?userId=" + this.getUID(), filename);
                return filename;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool registerServer()
        {
            try
            {
                Console.WriteLine("Для начала введи адрес сервера для учёта пользователей, либо нажми Enter, чтобы оставить текущий (" + BASE_URL + ")");
                string serverUrl = Console.ReadLine();
                if (String.IsNullOrEmpty(serverUrl))
                {
                    serverUrl = BASE_URL;
                }

                string data = WebUtils.makeGETRequest(serverUrl + "/deceiver/first-ping", "");
                if (data.Equals("SUCCESSFULLY-FIRST_PING"))
                {
                    BASE_URL = serverUrl;
                    Console.WriteLine("Отлично, URL адрес сервера теперь: " + BASE_URL);
                    Console.WriteLine("====================================================");
                    Console.WriteLine("= НЕ ЗАБУДЬ ДОБАВИТЬ АДРЕС В ИСКЛЮЧЕНИЕ АНТИВИРУСА =");
                    Console.WriteLine("====================================================");
                    return true;
                }
                else
                {
                    Console.WriteLine("Недействительный адрес сервера. Ожидалось SPING, получено ''");
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public void register()
        {
            try
            {
                Console.WriteLine("UID не найден. Начинаю регистрацию нового пользователя!");
                while (!registerServer()) { /* NOP */ }
                Console.WriteLine("Введи Ф.И.О. пользователя: ");
                string username = Console.ReadLine();
                Console.WriteLine("Отлично. Теперь введи комментарий, который будет отображён в веб-панели: ");
                string description = Console.ReadLine();
                Console.WriteLine("Отлично, посылаю информацию на сервер!");

                string response = WebUtils.makePOSTRequest(BASE_URL + "/deceiver/add", "name=" + username + "&description=" + description);
                Console.WriteLine("Ответ: " + response);

                Dictionary<string, string> userData = JSONUtils.convertJsonToStringDictionary(response);
                Console.WriteLine("Регистрация прошла успешно. ID нового пользователя: " + userData["id"]);
                Properties.Settings.Default["BASE_URL"] = BASE_URL;
                Properties.Settings.Default["UUID"] = Int32.Parse(userData["id"]);
                Properties.Settings.Default.Save();
                this.UID = Int32.Parse(userData["id"]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void saveBlockedSoftToStorage(List<BlockingSoft> blockedSoft)
        {
            try
            {
                if (blockedSoft.Count > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, blockedSoft);
                        ms.Position = 0;
                        byte[] buffer = new byte[(int)ms.Length];
                        ms.Read(buffer, 0, buffer.Length);
                        Properties.Settings.Default.LOCKED_SOFT = Convert.ToBase64String(buffer);
                        Properties.Settings.Default.Save();
                    }
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine("[UserAPI::saveBlockedSoftToStorage(List<T>)] Ошибка: Не удалось сохранить данные в память '" + e.Message + "'");
                throw e;
            }
        }

        public List<BlockingSoft> loadBlockedSoftFromStorageOrString(string _string)
        {
            try
            {
                string data = null;
                if (!String.IsNullOrEmpty(_string))
                {
                    data = _string;
                }
                else
                {
                    if (!String.IsNullOrEmpty(Properties.Settings.Default.LOCKED_SOFT))
                    {
                        data = Properties.Settings.Default.LOCKED_SOFT;
                    }
                }

                if (!String.IsNullOrEmpty(data))
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        return (List<BlockingSoft>)bf.Deserialize(ms);
                    }
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine("[UserAPI::loadBlockedSoftFromStorageOrString(void)] Ошибка: Не удалось получить данные из памяти '" + e.Message + "'"); 
            }
            return new List<BlockingSoft>(0);
        }

        public void clean()
        {
            Properties.Settings.Default["BASE_URL"] = BASE_URL;
            Properties.Settings.Default["UUID"] = -1;
            Properties.Settings.Default["LOCKED_SOFT"] = "";
            Properties.Settings.Default.Save();
        }

        public void setUID(int UID)
        {
            this.UID = UID;
        }

        public int getUID()
        {
            return this.UID;
        }
    }
}