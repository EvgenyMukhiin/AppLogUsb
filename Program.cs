using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LogUsb
{
    class Program
    {
        static void Main(string[] args)
        {
            AddInsetUSBHandler();
            AddRemoveUSBHandler();
            Console.WriteLine(app_start);
            Console.WriteLine($"Путь к файлу лога:{path}");
            Console.ReadKey();
        }
        // каталог для файла
        public static string writePath = @"C:\Log";
        public static string path = @"C:\Log\Log.txt";
        public static string date = DateTime.Now.ToString();

        static ManagementEventWatcher w = null;
        private static object sync = new object();
        public static string app_start = "Программа мониторинга запущена ";
        public static string usb_added = "USB устройство подключенно ";
        public static string usb_removed = "USB устройство отключенно ";

        public static void Write(_Exception ex)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(writePath);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3}\r\n", DateTime.Now, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message);
                lock (sync)
                {
                    File.WriteAllText(path, fullText);
                }
            }
            catch
            {

            }
        }

        public static async Task Logs(string str)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(writePath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = "Файл мониторинга создан: " + date + Environment.NewLine;
                File.WriteAllText(path, createText);
            }
            string appendText = str + Environment.NewLine;
            File.AppendAllText(path, appendText);
            string readText = File.ReadAllText(path);
            Console.WriteLine("Запись в лог выполнена");
        }

        public static void AddRemoveUSBHandler()
        {
            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBHub'";
                w = new ManagementEventWatcher(scope, q);
                w.EventArrived += new EventArrivedEventHandler(USBRemoved);
                w.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (w != null)
                    w.Stop();
            }
        }

        static void AddInsetUSBHandler()
        {
            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;
            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBHub'";
                w = new ManagementEventWatcher(scope, q);
                w.EventArrived += new EventArrivedEventHandler(USBAdded);
                w.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (w != null)
                    w.Stop();
            }
        }

        public static void USBAdded(object sender, EventArgs e)
        {
            try
            {
                string str = $"Событие:{usb_added} Дата:{DateTime.Now}";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(str);
                Logs(str);
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        public static void USBRemoved(object sender, EventArgs e)
        {
            try
            {
                string str = $"Событие:{usb_removed} Дата:{DateTime.Now}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(str);
                Logs(str);
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        static void Print(Sensor[] sensors)
        {
            if (sensors.Any())
                Console.WriteLine(string.Join(" ", sensors.Select(x => x.ToString())));
        }

        public static void t()
        {
            var cpu = Cpu.Discover();
            foreach (var item in cpu)
            {
                Print(item.CoreTemperatures);
                Print(item.CoreClocks);
                Print(item.CorePowers);
                Print(item.CoreVoltages);
                Print(item.CoreClocks);
            }
        }
    }

}
