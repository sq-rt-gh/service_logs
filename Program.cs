using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SafeBoard2023_service_logs
{
    internal class Program
    {
        static int Id = 0;
        static bool Notify = true;
        static Dictionary<int, IPrintable> Dict = new Dictionary<int, IPrintable>();

        static void Main()
        {
            string cmd;
            Console.Write("Type 'help' to show all commands \n>");

            while ((cmd = Console.ReadLine()) != "exit")
            {
                if (string.IsNullOrWhiteSpace(cmd))
                {
                    Console.Write(">");
                    continue;
                }

                switch (cmd.Trim().ToLower())
                {
                    case "help":
                        Console.WriteLine("Available commands: ");
                        Console.WriteLine("new - create new process");
                        Console.WriteLine("get - get info about process");
                        Console.WriteLine("status - show status of all processes");
                        Console.WriteLine("notify - Disable/enable notifications about finished processes");
                        Console.WriteLine("exit - exit program");
                        break;

                    case "new":
                        Console.Write("Directory: ");
                        string dir = Console.ReadLine();
                        if (!Directory.Exists(dir))
                        {
                            Console.WriteLine("Directory does not exists.");
                            break;
                        }

                        Console.Write("Search for: ");
                        string search = Console.ReadLine();

                        _ = AnalyzeServicesAsync(dir, search, Id++);
                        break;

                    case "get":
                        Console.Write("Process id: ");
                        int id;
                        try 
                        { 
                            id = Convert.ToInt32(Console.ReadLine().Trim());
                        }
                        catch
                        {
                            Console.WriteLine("Incorrect argument");
                            break;
                        }
                        if (Dict.ContainsKey(id))
                            Dict[id].Print();
                        else
                            Console.WriteLine($"Id {id} does not exists.");
                        break;

                    case "notify":
                        Notify = !Notify;
                        Console.WriteLine("Notifications " + (Notify ? "enabled" : "disabled"));
                        break;

                    case "status":
                        if (Dict.Count == 0)
                        {
                            Console.WriteLine("No active processes");
                            break;
                        }
                        foreach (var item in Dict)
                        {
                            if (item.Value is UnfinishedReport)
                                Console.WriteLine($"#{item.Key} - running");
                            else
                                Console.WriteLine($"#{item.Key} - finished");
                        }
                        break;

                    default:
                        Console.WriteLine("Incorrect command");
                        break;

                }
                Console.Write(">");
            }

        }

        async static Task AnalyzeServicesAsync(string dir, string search, int id)
        {
            Dict.Add(id, new UnfinishedReport(dir, search));

            Console.WriteLine("Process started with id: " + id);

            await Task.Delay(15000); //иммитация долгой работы
            IPrintable report;
            try
            {
                report = await AnalyzeServices(dir, search);
            }
            catch (Exception e)
            {
                report = new ErrorReport(e);
            }

            Dict[id] = report;

            if (Notify)
            {
                int l = Console.CursorLeft;
                int t = Console.CursorTop;
                Console.WriteLine($"\t\tproсess #{id} finished.");
                Console.SetCursorPosition(l, t);
            }
        }

        static Task<ServiceReportList> AnalyzeServices(string dir, string search) // получаем имена сервисов из каталога dir, которые удовлетворяют условию поиска search
        {
            List<string> services = new List<string>();

            foreach (string file in Directory.EnumerateFiles(dir))
            {
                string serviceName = Regex.Match(file, @"\\\w+\.log$").Value; // ищем файлы без ротации и извлекаем имя сервиса
                if (serviceName == "")
                    continue;

                serviceName = serviceName.Substring(1, serviceName.Length - 5); //отсечение слеша и расширения файла

                if (Regex.IsMatch(serviceName, search))
                {
                    services.Add(serviceName);
                }
            }
            
            List<ServiceReport> list = new List<ServiceReport>(services.Count);
            foreach (string service in services)
            {
                ServiceReport serviceReport = new ServiceReport(service);

                using (StreamReader sr = new StreamReader($@"{dir}\{service}.log")) //считывание данных из файла без ротации
                {
                    serviceReport.ReadFile(sr);
                }

                for (int i = 1; ; i++) //считывание данных из файлов с ротацией
                {
                    string file = $@"{dir}\{service}.{i}.log";
                    if (File.Exists(file))
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            serviceReport.ReadFile(sr);
                        }
                    }
                    else
                    {
                        serviceReport.Rotations = i - 1;
                        break;
                    }

                }

                list.Add(serviceReport);
            }

            return Task.FromResult(new ServiceReportList(list));
        }

    }
}
