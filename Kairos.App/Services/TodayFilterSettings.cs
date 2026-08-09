using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.Services
{
    public static class TodayFilterSettings
    {
        public static bool UseStarred
        {
            get => Preferences.Get("filter_starred", true);
            set => Preferences.Set("filter_starred", value);
        }

        public static bool UseDueDate
        {
            get => Preferences.Get("filter_due_date", false);
            set => Preferences.Set("filter_due_date", value);
        }

        public static DateTime DueBefore
        {
            get
            {
                var s = Preferences.Get("filter_dueBefore", "");
                if (DateTime.TryParse(s, out var dt))
                    return dt;

                return DateTime.Today;
            }
            set => Preferences.Set("filter_dueBefore", value.ToString("yyyy-MM-dd"));
        }
    }
}
