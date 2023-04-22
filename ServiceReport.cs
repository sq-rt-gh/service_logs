using System;
using System.Collections.Generic;
using System.IO;

namespace SafeBoard2023_service_logs
{
    interface IPrintable // нужен для хранения объектов в словаре
    {
        void Print();
    }

    class ServiceReport // для хранения информации об отчете о сервисе
    {
        public string Name;
        public DateTime FirstLog = DateTime.MaxValue;
        public DateTime LastLog = DateTime.MinValue;
        public Dictionary<string, int> Categories = new Dictionary<string, int>();
        public int Rotations = 0;

        public ServiceReport(string name) => Name = name;

        public void ReadFile(StreamReader sr) // дополнение объекта данными из файла
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

                if (Categories.ContainsKey(cat))
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

    class ServiceReportList : IPrintable // для хранения информации всех отчетов попавших в условие поиска
    {
        public List<ServiceReport> List;

        public ServiceReportList(List<ServiceReport> l) => List = l;

        public void Print()
        {
            if (List.Count == 0)
            {
                Console.WriteLine("No files were found");
            }
            foreach (var item in List)
            {
                Console.WriteLine("\n========================================\n");
                item.Print();
            }
            Console.WriteLine("\n========================================\n");
        }
    }

    class UnfinishedReport : IPrintable // для хранения информации о незавершеном отчете
    {
        public string Dir;
        public string FileMask;

        public UnfinishedReport(string dir, string mask)
        {
            Dir = dir;
            FileMask = mask;
        }

        public void Print()
        {
            Console.WriteLine("Status: In progress");
            Console.WriteLine($"Searching files \"{FileMask}\" in \"{Dir}\"");
        }
    }

    class ErrorReport : IPrintable
    {
        public Exception Exception;

        public ErrorReport(Exception e) => Exception = e;

        public void Print()
        {
            Console.WriteLine("Error:");
            Console.WriteLine(Exception.Message);
        }
    }
}
