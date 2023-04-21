using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SafeBoard2023_service_logs
{
     internal class Program
     {
        static void Main()
        {
            Console.Write("Directory: ");
            string dir = Console.ReadLine();
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("This directory does not exists.");
                return;
            }

            Console.Write("Search: ");
            string search = Console.ReadLine();

            foreach (var item in AnalyzeServices(dir, search))
            {
                Console.WriteLine("\n========================================\n");
                item.Print();
            }


            Console.ReadKey();
        }

        static IEnumerable<ServiceReport> AnalyzeServices(string dir, string search) 
        {
            // получаем имена сервисов из каталога dir, которые удовлетворяют условию поиска search
            List<string> services = new List<string>();
            try
            {
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
            }
            catch(Exception ex) 
            { 
                Console.WriteLine("Error: " + ex.Message); 
            }


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

                yield return serviceReport;
            } 
        }

     }
    
    class ServiceReport
    {
        public string Name;
        public DateTime FirstLog = DateTime.MaxValue;
        public DateTime LastLog = DateTime.MinValue;
        public Dictionary<string, int> Categories = new Dictionary<string, int>();
        public int Rotations = 0;

        public ServiceReport(string name) 
        {
            Name = name;
        }

        public void ReadFile(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;

                // Извлекаем дату и время
                int ind = line.IndexOf(']');
                DateTime dt = DateTime.Parse(line.Substring(1, ind - 2));

                if (dt < FirstLog) FirstLog = dt;
                if (dt > LastLog) LastLog = dt;

                // Извлекаем категорию
                ind += 2; //начальный индекс категории
                string cat = line.Substring(ind, line.IndexOf(']', ind) - ind);

                if(Categories.ContainsKey(cat))
                    Categories[cat] += 1;
                else
                    Categories.Add(cat, 1);

            }
        }

        public void Print()
        {
            Console.WriteLine("Service: " + Name);

            if (Categories.Count == 0) //если нет информации о логах, то нет смысла выводить остальную информацию
            {
                Console.WriteLine("\nNo logs were found for this service.");
                return;
            }

            Console.WriteLine($"\nFirst log: {FirstLog}.{FirstLog.Millisecond}");
            Console.WriteLine($"Last log:  {LastLog}.{LastLog.Millisecond}\n");

            Console.WriteLine("Log count in each category: ");
            foreach (var item in Categories)
            {
                Console.WriteLine($"\t{item.Key} - {item.Value}");
            }

            Console.WriteLine("\nRotations: " + Rotations);
        }
    }
}
