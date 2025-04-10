using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace CAAP2.Business.Helpers
{
    public static class OrderValidationHelper
    {
        public static bool IsWithinAllowedTime()
        {
            var now = DateTime.Now;
            var day = now.DayOfWeek;
            var time = now.TimeOfDay;

            //domingo a jueves 10am - 9pm
            if (day >= DayOfWeek.Sunday && day <= DayOfWeek.Thursday)
            {
                return time >= new TimeSpan(10, 0, 0) && time <= new TimeSpan(21, 0, 0);
            }

            //viernes y sabado 11am - 11pm
            if (day == DayOfWeek.Friday || day == DayOfWeek.Saturday)
            {
                return time >= new TimeSpan(11, 0, 0) && time <= new TimeSpan(23, 0, 0);
            }

            return false;
        }
    }
}

