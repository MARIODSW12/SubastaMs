using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subasta.Infrastructure.Utils
{
    public class Utils
    {
        public static string ConvertToCronExpression(DateTime dateTime, bool includeSeconds = false)
        {
            // Format: [Seconds] Minutes Hours Day Month DayOfWeek (Year - optional)
            return includeSeconds
                ? $"{dateTime.Second} {dateTime.Minute} {dateTime.Hour} {dateTime.Day} {dateTime.Month} ? {dateTime.Year}"
                : $"{dateTime.Minute} {dateTime.Hour} {dateTime.Day} {dateTime.Month} ? {dateTime.Year}";
        }
    }
}
