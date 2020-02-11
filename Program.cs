using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace RemoteComputerControl
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private static IntPtr currentConsolwWindowPointer = GetConsoleWindow();
        private static bool normalBehaviourWithWindow = true;

        private Timer queryTimer = new Timer(10 * 1000);
        private static UserAPI userAPI = new UserAPI();
        private static BlockingQueue<BlockingSoft> blockingQ = new BlockingQueue<BlockingSoft>();
        private BlockProcessor blocker = null;
        private bool timerIsBusy = false;

        private static string autoLaunchLocation = "Software\\Microsoft\\Windows\\CurrentVersion\\Run\\";
        private static string autoLaunchKey = "rcc";
        private static string currentAppPath = Assembly.GetEntryAssembly().Location;

        public void Start(string[] args)
        {
            blocker = BlockProcessor.getInstanse(blockingQ, userAPI);
            Console.WriteLine("Проверка регистрации.");
            if (!userAPI.isUserRegistered())
            {
                if (normalBehaviourWithWindow) ShowWindow(currentConsolwWindowPointer, SW_SHOW);
                try
                {
                    userAPI.register();
                    if (normalBehaviourWithWindow) ShowWindow(currentConsolwWindowPointer, SW_HIDE);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Регистрация не удалась :( Ошибка: [UserAPI::register(void)]: " + e.Message);
                    Console.WriteLine("Для продолжения нажмите на любую клавишу...");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine("Сервер: " + UserAPI.BASE_URL + ".");
                Console.WriteLine("Пользователь зарегистрирован. ID: " + userAPI.getUID() + ". [Минор: " + UserAPI.CLIENT_API_VERSION + "]");
            }
            queryTimer.Elapsed += queryTimer_Elapsed;
            queryTimer.Start();
            while (true) 
            { 
                Console.ReadKey(); 
            }
        }

        private void queryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!timerIsBusy)
            {
                timerIsBusy = true;
                try
                {
                    List<BlockingSoft> blockedSoftware = userAPI.getBlockedSoftware();
                    blocker.addBlockingSoft(blockedSoftware);
                    if (!RegistryUtils.checkKeyValueExists(autoLaunchKey, autoLaunchLocation))
                    {
                        RegistryUtils.addKeyValueToCurrentUserRegistry(autoLaunchKey, currentAppPath, autoLaunchLocation);
                    }
                }
                catch (UserUnauthrizedException)
                {
                    Console.WriteLine("Пользователь не авторизирован. Это знак того, что он был удалён из базы. Производим очистку!");
                    this.clean();
                }
                catch (APIUpgradeRequired)
                {
                    Console.WriteLine("Требуется обновление API клиента! Начинаю процесс обновления!");
                    this.update();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Program::queryTimer_Elapsed(object, ElapsedEventArgs)] Ошибка: " + ex.Message);
                }
                finally
                {
                    timerIsBusy = false;
                }
            }
        }

        private void update()
        {
            try
            {
                Console.WriteLine("Для начала сохраняем всю конфигурацию...");
                FileUtils.writeToFile(AppDomain.CurrentDomain.BaseDirectory + "cfg.tmp", new string[] {
                                Properties.Settings.Default.BASE_URL,
                                Properties.Settings.Default.UUID.ToString(),
                                Properties.Settings.Default.LOCKED_SOFT
                });
                Console.WriteLine("Конфигурация записана успешно! Начинаем скачивание данных.");
                string downloadedFileName = userAPI.updateClient();
                Console.WriteLine("Файл скачан и готов: " + downloadedFileName);
                RegistryUtils.removeKeyValueFromCurrentUserRegistry(autoLaunchKey, autoLaunchLocation);
                string args = "-upd " + Process.GetCurrentProcess().Id;
                if (!normalBehaviourWithWindow) args += " -spw";
                Process updatedVersion = new Process();
                updatedVersion.StartInfo.FileName = downloadedFileName;
                updatedVersion.StartInfo.Arguments = args;
                updatedVersion.Start();
                updatedVersion.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Program::update(void)] Обновление клиента не удалось: " + ex.Message);
            }
        }

        private void clean()
        {
            RegistryUtils.removeKeyValueFromCurrentUserRegistry(autoLaunchKey, autoLaunchLocation);
            queryTimer.Stop();
            userAPI.clean();
            blocker.clean();
            Console.WriteLine("Идентификатор API уничтожен! Производим выход!");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("-spw"))
                    {
                        normalBehaviourWithWindow = false;
                        Console.WriteLine("- Поведение \"Скрытие окна\" отключено -");
                    }

                    if (args[i].Equals("-upd"))
                    {
                        try
                        {
                            int processId = Int32.Parse(args[i + 1]);
                            Console.WriteLine("Получена команда обновления и параметр: " + processId);
                            try
                            {
                                Console.WriteLine("Читаем конфигурацию...");
                                string[] data = FileUtils.readAllLines(AppDomain.CurrentDomain.BaseDirectory + "cfg.tmp");
                                string BASE_URL = data[0];
                                string UID = data[1];
                                string BLOCKED_SOFT = data[2];

                                Console.WriteLine("Считано:");
                                Console.WriteLine(BASE_URL);
                                Console.WriteLine(UID);
                                Console.WriteLine(BLOCKED_SOFT);
                                FileUtils.removeFile(AppDomain.CurrentDomain.BaseDirectory + "cfg.tmp");
                                Console.WriteLine("Конфигурация считана успешно! Записываем...");
                                Properties.Settings.Default["BASE_URL"] = BASE_URL;
                                Properties.Settings.Default["UUID"] = Int32.Parse(UID);
                                List<BlockingSoft> storedBlockingSoft = userAPI.loadBlockedSoftFromStorageOrString(BLOCKED_SOFT);
                                Properties.Settings.Default.Save();
                                Console.WriteLine("Конфигурация записана успешно!");
                                Process parentProcess = Process.GetProcessById(processId);
                                Console.WriteLine("Закрываем родительский процесс: " + parentProcess.ProcessName);
                                parentProcess.Kill();
                                Console.WriteLine("Родительский процесс завершён!");
                                blockingQ.clearAllAndPutCollection(storedBlockingSoft);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Не удалось довести обновление до конца :( [" + e.Message + "]");
                                throw e;
                            }
                            
                        }
                        catch
                        {
                            Console.WriteLine("Не удалось получить параметр аргумента: -update");
                            Environment.Exit(-1);
                        }
                    }
                }
            }
            if (normalBehaviourWithWindow) ShowWindow(currentConsolwWindowPointer, SW_HIDE);
            new Program().Start(args);
        }
    }
}