using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvestingCalendarParser
{
    public partial class Form1 : Form
    {
        Parser parser = new Parser();
        DateTime dateFrom;
        DateTime dateTo;

        private List<string> inData = new List<string>();
        private List<string> outData = new List<string>();

        public Form1()
        {
            InitializeComponent();

            //// цстанавливаем текущую дату
            //dateTimePickerFrom.Value = DateTime.Now;
            //dateTimePickerTo.Value = DateTime.Now;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            //if (inData.Count < 2)
            //{
            //    textBoxOut.Text = "Нет загруженных дат";
            //    return;
            //}

            //DateTime dateValue;

            //// parsing data
            //for (int i = 0; i < inData.Count; i++)
            //{
            //    List<string> stringValue = inData[i].Split(';').ToList();

            //    // час, для которого (+- час) ищем события
            //    int searchHour = 0;

            //    // очищаем текстовое поле
            //    textBoxOut.Clear();

            //    if (i == 0)
            //    {   // первая строка, м.б. заголовок
            //        if (!DateTime.TryParse(stringValue[0], out dateValue))
            //        {   // заголовок
            //            stringValue.Add("Events");
            //            outData.Add(String.Join(";",stringValue));

            //            //textBoxOut.Text += $"{outData[i]}\r\n";
            //        }
            //    }
            //    else
            //    {   // все остальные (кроме первой) строки
            //        if (DateTime.TryParse(stringValue[0], out dateValue))
            //        {   // ищем события на дату dateValue
                        
            //            string receivedPage = null;
            //            while (receivedPage == null)
            //            {
            //                // получение данных календаря
            //                try
            //                {
            //                    receivedPage = parser.GetPage(dateValue).Result;
            //                }
            //                catch (Exception ex) { }
            //            }

            //            var calendarEvents = parser.Parse(dateValue, receivedPage);

            //            if (calendarEvents.Count == 0)
            //            {   // если событий нет
            //                stringValue[stringValue.Count - 1] = "Error loading events on this date!";
            //                continue;
            //            }

            //            stringValue[stringValue.Count - 1] += CollectedData.GetHeaders() + "\r\n";

            //            foreach (var evnt in calendarEvents)
            //            {
            //                stringValue[stringValue.Count - 1] += "\""+ evnt.ToString() + "\"\r\n";
            //            }
            //        }
            //        else
            //        {
            //            stringValue[stringValue.Count - 1] = "Error loading events on this date!";
            //        }

            //        outData.Add(String.Join(";", stringValue));
            //        Thread.Sleep(300);


            //        // TODO: выбрать только нужное время +- час
            //        //    проверить выборку времени на Program.cs

            //        // TODO: 2) обработка webException если пропала связь - в while(Exception.message != null)


            //    }
            //}

            

            //for (int i = 0; i < outData.Count; i++)
            //{
            //    if (i < 3 || i > outData.Count - 3)
            //    {
            //        textBoxOut.Text += $"{outData[i]}\r\n";
            //    }
            //    else
            //    {
            //        textBoxOut.Text += "\r\n...\r\n";
            //    }
            //}


            /*

            // получаем дату из полей
            //dateFrom = dateTimePickerFrom.Value;
            //dateTo = dateTimePickerTo.Value;

            // получаем распарсенный список событий
            var calendarEvents = parser.StartDatesFromTo(dateFrom, dateTo);

            // очищаем текстовое поле
            textBoxOut.Clear();

            if (calendarEvents.Count == 0)
            {   // если событий нет
                textBoxOut.Text = "Нет событий для отображения";
                return;
            }

            // заголовки
            //textBoxOut.Text += calendarEvents[0].GetHeaders() + "\r\n";
            textBoxOut.Text += CollectedData.GetHeaders() + "\r\n";

            foreach (var item in calendarEvents)
            {
                textBoxOut.Text += item.ToString() + "\r\n";
            }
            */
        }

        private void btnOpenDatesFile_Click(object sender, EventArgs e)
        {
            OpenDatesFile();
        }

        private void OpenDatesFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            // получаем выбранный файл
            string fileName = openFileDialog1.FileName;

            inData = System.IO.File.ReadAllLines(fileName).ToList();

            textBoxOut.Text = "Количество загруженных сторок: " + inData.Count;
            textBoxOut.AppendText("\r\nПервые 3 строки: ");

            for (int i = 0; i <= 3; i++)
            {
                textBoxOut.AppendText("\r\n" + inData[i]);
            }
        }
    }
}
