using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Pardis.Utility
{
    public class Utility
    {
        public static string GetPersianDate(DateTime date, Boolean Reverse = false)
        {
            if (date > DateTime.MinValue)
            {
                PersianCalendar PC = new PersianCalendar();
                string Year = PC.GetYear(date).ToString();
                string Month = (PC.GetMonth(date) < 10) ? "0" + PC.GetMonth(date).ToString() : PC.GetMonth(date).ToString();
                string Day = (PC.GetDayOfMonth(date) < 10) ? "0" + PC.GetDayOfMonth(date).ToString() : PC.GetDayOfMonth(date).ToString();

                if (!Reverse)
                    return string.Format("{0}/{1}/{2}", Day, Month, Year);
                else
                    return string.Format("{0}/{1}/{2}", Year, Month, Day);
            }
            else
                return "";
        }

        public static DateTime ShamsiDateToGregorianDate(string date)
        {
            var pc = new PersianCalendar();
            string[] parts = date.Split('/');
            if (parts.Length != 3)
                throw new Exception("Incorrect format in shamsi date.");

            return pc.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0, 0, 0, 0);
        }

        public static DateTime ShamsiDateToGregorianDate(string date, string time)
        {
            var pc = new PersianCalendar();
            string[] parts = date.Split('/');
            if (parts.Length != 3)
                throw new Exception("Incorrect format in shamsi date.");

            string[] arrTime = time.Split(':');
            if (arrTime.Length != 3)
                throw new Exception("Incorrect format in shamsi date.");

            return pc.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(arrTime[0]),
                                 int.Parse(arrTime[1]), int.Parse(arrTime[2]), 0);
        }

        public static string ReversePersianDateStr(string PersianDateStr)
        {
            if (PersianDateStr.Length == 10)
            {
                string Year = PersianDateStr.Substring(6, 4);
                string Month = PersianDateStr.Substring(3, 2);
                string Day = PersianDateStr.Substring(0, 2);
                return Year + "/" + Month + "/" + Day;
            }
            else
                return "";
        }

        public static bool IsDateValid(string PersianDateStr, bool isReverse = false)
        {
            if (PersianDateStr.Length == 10)
            {
                string Year = PersianDateStr.Substring(6, 4);
                string Month = PersianDateStr.Substring(3, 2);
                string Day = PersianDateStr.Substring(0, 2);
                bool flag = false;
                try
                {
                    int intYear = Convert.ToInt32(Year);
                    int intMonth = Convert.ToInt32(Month);
                    int intDay = Convert.ToInt32(Day);
                }
                catch
                {
                    flag = true;
                }
                if (!flag)
                    return IsDateValid(Year, Month, Day);
                else
                {
                    Year = PersianDateStr.Substring(0, 4);
                    Month = PersianDateStr.Substring(5, 2);
                    Day = PersianDateStr.Substring(8, 2);
                    return IsDateValid(Year, Month, Day);
                }

            }
            else
                return false;
        }

        public static bool IsDateValid(string YearStr, string MonthStr, string DayStr)
        {
            try
            {
                int Year, Month, Day;
                if (YearStr != "")
                    Year = Convert.ToInt32(YearStr);
                else
                    Year = 0;
                Month = Convert.ToInt32(MonthStr);
                Day = Convert.ToInt32(DayStr);
                bool Result = true;
                if (Year != 0 || Month != 0 || Day != 0)
                {
                    if (Day == 0)
                        Result = false;
                    if (Month == 0)
                        Result = false;
                    int FYear = 0;
                    bool IsFDigit = int.TryParse(YearStr, out FYear);
                    if (YearStr == "" || YearStr.Length < 4 || FYear < 1250 || FYear > 1450)
                        Result = false;
                }
                else
                    Result = false;
                return Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تبدیل دو نوع داده که خصوصیت های هم نام به هم دارند
        /// </summary>
        /// <typeparam name="T">نوع داده تبدیل شده</typeparam>
        /// <param name="SourceObject">شیئ که قصد تبدیل آن را به شیء دیگر داریم</param>
        /// <returns>شیء جدید از نوع داده T</returns>
        public static T Cast<T>(object SourceObject)
        {
            Type objectType = SourceObject.GetType();
            Type DestinationObject = typeof(T);

            var x = Activator.CreateInstance(DestinationObject, false);

            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in DestinationObject.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();

            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                if (SourceObject.GetType().GetProperty(memberInfo.Name) != null)
                {
                    propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                    value = SourceObject.GetType().GetProperty(memberInfo.Name).GetValue(SourceObject, null);
                    if (value != null && propertyInfo.PropertyType.FullName.Contains("DateTime") && (DateTime)value == DateTime.MinValue)
                        value = null;
                    if (value != null)
                        if (!propertyInfo.PropertyType.Name.Contains("Null"))
                            propertyInfo.SetValue(x, Convert.ChangeType(value, propertyInfo.PropertyType), null);
                        else
                            propertyInfo.SetValue(x, value, null);
                }
            }

            return (T)x;
        }

        /// <summary>
        /// تبدیل دو لیست به یکدیگر
        /// </summary>
        /// <typeparam name="T">نوع داده عناصر لیست مقصد</typeparam>
        /// <typeparam name="G">نوع داده عناصر لیست مبداء</typeparam>
        /// <param name="SourceList">شی لیست مبداء</param>
        /// <returns>شیء لیست مقصد</returns>
        public static List<T> Cast<T, G>(List<G> SourceList)
        {
            List<T> Result = new List<T>();

            if (SourceList != null)
            {
                foreach (G Item in SourceList)
                {
                    T tempItem = Cast<T>(Item);
                    if (tempItem != null)
                        Result.Add(tempItem);
                }
            }
            return Result;
        }

    }
}