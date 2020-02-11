using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RemoteComputerControl
{
    public class FileUtils
    {
        public static string[] readAllLines(string file) 
        {
            try
            {
                return File.ReadAllLines(file);
            }
            catch (Exception e)
            {
                Console.WriteLine("[FileUtils::readAllLines(string)] Не удалось прочитать файл: " + e.Message);
                throw e;
            }
        }

        public static bool isFileExists(string file) 
        {
            try
            {
                return File.Exists(file);
            }
            catch (Exception e)
            {
                Console.WriteLine("[FileUtils::isFileExists(string)] Не удалось проверить файл на существование: " + e.Message);
                throw e;
            }
        }

        public static void writeToFile(string filepath, string[] data)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filepath, false))
                {
                    foreach (string line in data)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[FileUtils::writeToFile(string, string[])] Не удалось записать в файл: " + e.Message);
            }
        }

        public static bool removeFile(string file)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[FileUtils::isFileExists(string)] Не удалось удалить файл: " + e.Message);
                return false;
            }
        }
    }
}
