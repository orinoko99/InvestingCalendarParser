using System;

namespace InvestingCalendarParser
{
    /// <summary>
    /// собранные данные
    /// </summary>
    public class CollectedData
    {
        /// <summary>
        /// дата и время события
        /// </summary>
        public DateTime Date;

        /// <summary>
        /// валюта события
        /// </summary>
        public string Currency;

        /// <summary>
        /// волатильность - количество звездочек-быков у события
        /// </summary>
        public int Volatility;

        /// <summary>
        /// аннотация события
        /// </summary>
        public string Annotation;

        /// <summary>
        /// ссылка на полное описание макроэкономического события
        /// </summary>
        public string AnnotationHref;

        //Факт. Прогноз Пред. строками - т.к. там встречаются %
        public string Actual; // Факт.
        public string Forecast; // Прогноз
        public string Privious; // Пред

        public static string GetHeaders() =>
            "Date\t" +
            "Currency\t" +
            "Volatility\t" +
            "Annotation\t" +
            "AnnotationHref\t" +
            "Actual\t" +
            "Forecast\t" +
            "Privious";

        public override string ToString() =>
            $"{Date}\t{Currency}\t{Volatility}\t{Annotation}\t{AnnotationHref}\t{Actual}\t{Forecast}\t{Privious}";
    }
}
