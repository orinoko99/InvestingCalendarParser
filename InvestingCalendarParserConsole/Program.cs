using InvestingCalendarParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace InvestingCalendarParserConsole
{
    class Program
    {
        static Parser parser = new Parser();
        static string searchDatesFile = @"D:\Orinoko\Documents\tmp.csv";
        static string saveDatesFile = @"D:\Orinoko\Documents\events.csv";
        static List<string> inData = new List<string>();
        static List<string> outData = new List<string>();

        // берем события по времени +- nn часов:
        static int addHourBeforeEvent = 0; // кол-во часов до события
        static int addHourAfterEvent = 0; // кол-во часов после события

        // задержка между обращениями к серверу, миллисекунд
        static int connDelay = 100; 

        static void Main(string[] args)
        {
            // загрузить даты для поиска событий
            OpenDatesFile();

            // найти и загрузить события для каждой даты из inData в outData
            ParsingDatesCycle();

            // сохранить события в файл
            SaveEventsData();

            //Console.ReadKey();
        }

        /// <summary>
        /// Запись событий в файл
        /// </summary>
        private static void SaveEventsData()
        {
            Encoding encoding = Encoding.UTF8;
            try
            {
                //using (StreamWriter sw = new StreamWriter(saveDatesFile, false, System.Text.Encoding.Default))
                using (StreamWriter sw = new StreamWriter(saveDatesFile, false, encoding))
                {
                    foreach (var item in outData)
                    {
                        sw.WriteLine(item);
                    }                    
                }

                Console.WriteLine($"Файл сохранен успешно: {saveDatesFile}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// найти и загрузить события для каждой даты из inData в outData
        /// </summary>
        private static void ParsingDatesCycle()
        {
            if (inData.Count < 2)
            {
                Console.WriteLine("Нет загруженных дат");
                return;
            }

            DateTime dateValue;

            // проверяем первую строку - если заголовки, то добавляем заголовки в новый список            
            int beginFromString = 0;

            List<string> stringValue = inData[0].Split(';').ToList();

            if (!DateTime.TryParse(stringValue[0], out dateValue))
            {   // заголовок
                stringValue.Add("Events");
                outData.Add(String.Join(";", stringValue));
                Console.WriteLine("добавляем в заголовки колонку Events");
                
                //т.к. 0 строка - заголовок, начинаем со второй
                beginFromString = 1;
            }

            // parsing data
            for (int i = beginFromString; i < inData.Count; i++)
            {
                stringValue = inData[i].Split(';').ToList();
                
                // час, для которого (+- час) ищем события
                int searchHour = 0;

                if (DateTime.TryParse(stringValue[0], out dateValue))
                {   // ищем события на дату dateValue

                    // будем брать события searchHourHour +- час
                    searchHour = dateValue.Hour;

                    string receivedPage = null;
                    while (receivedPage == null)
                    {
                        Console.WriteLine("Try ro get data...");
                        // получение данных календаря
                        try
                        {
                            receivedPage = parser.GetPage(dateValue).Result;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    var calendarEvents = parser.Parse(dateValue, receivedPage, searchHour, addHourBeforeEvent, addHourAfterEvent);

                    if (calendarEvents.Count == 0)
                    {   // если событий нет
                        stringValue.Add("Error loading events on this date!");
                        Console.WriteLine("Error loading events on this date!");
                        continue;
                    }

                    var eventsString = "\"" + CollectedData.GetHeaders() + "\r\n";
                    //stringValue[stringValue.Count - 1] += 
                    foreach (var evnt in calendarEvents)
                    {
                        eventsString += evnt.ToString() + "\r\n";
                    }
                    eventsString += "\"";
                    stringValue.Add(eventsString);
                }
                else
                {
                    stringValue.Add("Error loading events on this date!");
                    Console.WriteLine("Error loading events on this date!");
                }

                outData.Add(String.Join(";", stringValue));

                Console.WriteLine("Added column: " + outData[outData.Count-1].ToString());
                Thread.Sleep(connDelay);
            }
        }

        /// <summary>
        /// открытие файла с датами и другими параметрами инструмента и добавление их в список inData
        /// </summary>
        private static void OpenDatesFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(searchDatesFile, System.Text.Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        inData.Add(line);
                    }
                    Console.WriteLine("\r\nДанные загружены успешно!\r\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
