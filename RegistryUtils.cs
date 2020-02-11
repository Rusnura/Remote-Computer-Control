using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    public class RegistryUtils
    {
        public static void addKeyValueToCurrentUserRegistry(string key, string value, string location)
        {
            try
            {
                using (RegistryKey rKey = Registry.CurrentUser.CreateSubKey(location))
                {
                    rKey.SetValue(key, value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[RegistryUtils::addKeyValueToCurrentUserRegistry(string, string, string)] Ошибка создания ключа: " + e.Message);
            }
        }

        public static void removeKeyValueFromCurrentUserRegistry(string key, string location)
        {
            try
            {
                using (RegistryKey rKey = Registry.CurrentUser.OpenSubKey(location, true))
                {
                    rKey.DeleteValue(key, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[RegistryUtils::removeKeyValueFromCurrentUserRegistry(string, string)] Ошибка удаления ключа: " + e.Message);
            }
        }

        public static bool checkKeyValueExists(string key, string location)
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey(location).GetValue(key, null) != null;
            }
            catch (Exception e)
            {
                Console.WriteLine("[RegistryUtils::checkKeyValueExists(string, string)] Ошибка проверки ключа: " + e.Message);
                return false;
            }
        }
    }
}
