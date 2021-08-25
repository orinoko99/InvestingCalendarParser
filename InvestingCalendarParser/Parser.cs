using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows.Forms;

//https://anglesharp.github.io/docs/01-articles

namespace InvestingCalendarParser
{
    public class Parser
    {
        /// <summary>
        /// список собраннх событий
        /// </summary>
        List<CollectedData> events;

        /// <summary>
        /// сбор статистики
        /// "имя-аннотация" события, кол-во раз, которое встречается
        /// </summary>
        public Dictionary<string, int> Statistic;

        /// <summary>
        /// адрес календаря для запроса post
        /// </summary>
        string urlPost;

        /// <summary>
        /// адрес календаря для запроса get
        /// </summary>
        string urlGet;

        /// <summary>
        /// конфигурация
        /// </summary>
        IConfiguration config;

        /// <summary>
        /// HttpClient
        /// </summary>
        HttpClient client;

        public Parser()
        {            
            urlPost = "https://ru.investing.com/economic-calendar/Service/getCalendarFilteredData";
            urlGet = "https://ru.investing.com/economic-calendar/";
            config = Configuration.Default.WithDefaultLoader().WithDefaultCookies();
            client = new HttpClient();

            // headers
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
            client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
            client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU, ru;q=0.9, en-US;q=0.8, en;q=0.7, fr;q=0.6");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

            Statistic = new Dictionary<string, int>();
        }

        public List<CollectedData> Parse(DateTime date, string receivedPage, int searchHour, int addHourBeforeEvent, int addHourAfterEvent)
        {
            // список-хранилище событий / Event storage-List
            List<CollectedData> collectedData = new List<CollectedData>();

            string data ="<table>" + Json.Decode(receivedPage).data + "</table>";

            // парсим html / Parse HTML
            var htmlParser = new HtmlParser();
            //var htmlCode = htmlParser.ParseDocument(data);
            var htmlCode = htmlParser.ParseDocument(data);

            // события / events
            var events = htmlCode.QuerySelectorAll("tr");

            // разбор списка событий со страницы календаря / foreach of the list of events from the calendar page
            foreach (var e in events)
            {
                // у событий строки только с непустым ID / event string only with non-empty ID

                var eventDate = Convert.ToDateTime(e.GetAttribute("data-event-datetime"));

                // единичное событие / Single event
                CollectedData evnt = new CollectedData();

                // дата и время события / date and time events
                evnt.Date = eventDate;

                // получение всех колонок данной строки / Getting all columns of this line
                var rows = e.QuerySelectorAll("td");

                if (rows.Length == 1)
                {
                    continue;
                    evnt.Annotation = rows[0].InnerHtml;
                }
                else if (rows.Length > 1 && rows.Length <= 5)
                {   // выходной, праздник и т.п.
                    continue;

                    for (int i = 1; i < rows.Length; i++)
                    {
                        if (i == 1)
                        {   // country / currency
                            evnt.Currency = rows[i].QuerySelector("span").GetAttribute("data-img_key");
                        }
                        if (i == 2)
                        {   // volatility
                            evnt.Volatility = -1;
                        }
                        if (i == 3)
                        {   // annotation
                            evnt.Annotation = rows[i].TextContent;
                        }
                    }
                }
                else
                {
                    int startHour = searchHour - addHourBeforeEvent;
                    int endHour = searchHour + addHourAfterEvent;

                    // если у события есть время (не выходной или праздник, не служебная строка)
                    // и время +- час от искомого searchHour - парсим его
                    if (eventDate.Year != 1 && eventDate.Hour >= startHour && eventDate.Hour <= endHour)
                    {
                        //Console.WriteLine($"Время события {eventDate} укладывается в диапазон часов {startHour} - {endHour} (искомый час: {searchHour})");

                        for (int i = 1; i < rows.Length; i++)
                        {
                            if (i == 1)
                            {   // country / currency
                                rows[i].QuerySelector("span").Remove();
                                evnt.Currency = rows[i].TextContent;
                            }
                            if (i == 2)
                            {   // volatility
                                var field = rows[i].GetAttribute("data-img_key");
                                int volatility = 0;

                                if (field != null && Int32.TryParse(rows[i].GetAttribute("data-img_key").Replace("bull", ""), out volatility))
                                {
                                    evnt.Volatility = volatility;
                                }
                                else
                                {
                                    evnt.Volatility = -1;
                                }
                                //evnt.Volatility = Convert.ToInt32(rows[i].GetAttribute("data-img_key").Replace("bull", ""));
                            }
                            if (i == 3)
                            {   // annotation
                                try
                                {
                                    evnt.AnnotationHref = rows[i].QuerySelector("a").GetAttribute("href");
                                    evnt.Annotation = rows[i].QuerySelector("a").TextContent.Trim();

                                    AddStatistic(evnt.Annotation);
                                }
                                catch
                                {
                                    evnt.AnnotationHref = "err";
                                    evnt.Annotation = "err";
                                }
                            }
                            if (i == 4)
                            {   // actual
                                try
                                {
                                    evnt.Actual = rows[i].TextContent;
                                }
                                catch
                                {
                                    evnt.Actual = "err";
                                }
                            }
                            if (i == 5)
                            {   // forecast
                                try
                                {
                                    evnt.Forecast = rows[i].TextContent;
                                }
                                catch
                                {
                                    evnt.Forecast = "err";
                                }
                            }
                            if (i == 6)
                            {   // previous
                                try
                                {
                                    evnt.Privious = rows[i].TextContent;
                                }
                                catch
                                {
                                    evnt.Privious = "err";
                                }
                            }
                        }

                    }
                    else
                    {
                        //Console.WriteLine($"Время события {eventDate} НЕ укладывается в диапазон часов {startHour} - {endHour} (искомый час: {searchHour})");
                        continue;
                    }
                }
                collectedData.Add(evnt);
            }

            return collectedData;
        }

        private void AddStatistic(string annotation)
        {
            //var elem = Statistic.First(e => e.Key.Equals(annotation));

            if (Statistic.ContainsKey(annotation))
            {
                Statistic[annotation]++; 
            }
            else
            {
                Statistic[annotation] = 1;
            }

        }

        public async Task<string> GetPage(DateTime _date)
        {
            // верный формат даты для того чтобы его принял сервер
            string date = _date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string page = null;

            // параметры для post запроса
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("country:", ""));
            keyValues.Add(new KeyValuePair<string, string>("timeZone", "18"));
            keyValues.Add(new KeyValuePair<string, string>("dateFrom", date));
            keyValues.Add(new KeyValuePair<string, string>("dateTo", date));
            keyValues.Add(new KeyValuePair<string, string>("timeFilter", "timeRemain"));
            keyValues.Add(new KeyValuePair<string, string>("currentTab", "custom"));
            keyValues.Add(new KeyValuePair<string, string>("limit_from", "0"));

            // отправка post-запроса
            var responsePost = await client.PostAsync(urlPost, new FormUrlEncodedContent(keyValues)).ConfigureAwait(false);
            page = responsePost.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return page;
        }
    }
}
