using System.Globalization;

class Parse
{
    const string TimeFormatException = "ERROR: bad format time";

    public static TimeSpan ParseTime(string time)
    {
        // "([HH..Hh][MM..Mmin][SS...Ss])"
        // accepting ambigous formatting eg. '1h3m'

        int lastIdx = 0;

        int hIdx = time.IndexOf('h');
        uint hours = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] hIdx: {hIdx}");
        # endif
        if (hIdx != -1)
        {
            if (! uint.TryParse(time[0..hIdx], out hours))
            {
                throw new FormatException(TimeFormatException);
            }
            lastIdx = hIdx + 1;
        }

        int minIdx = time.IndexOf("min");
        uint minutes = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] hours: {hours}");
            Console.WriteLine($"[DEBUG] minIdx: {minIdx}");
        # endif
        if (minIdx != -1)
        {
            if (! uint.TryParse(time[(hIdx + 1)..minIdx], out minutes))
            {
                throw new FormatException(TimeFormatException);
            }
            lastIdx = minIdx + 3;
        }

        int sIdx = time.IndexOf('s');
        uint seconds = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] minutes: {minutes}");
            Console.WriteLine($"[DEBUG] sIdx: {sIdx}");
        # endif
        if (sIdx != -1)
        {
            if (! uint.TryParse(time[lastIdx..sIdx], out seconds))
            {
                throw new FormatException(TimeFormatException);
            }
        }

        # if DEBUG
            Console.WriteLine($"[DEBUG] seconds: {seconds}");
        # endif

        if (hours == minutes && minutes == seconds && seconds == 0)
        {
            throw new FormatException(TimeFormatException);
        }

        return new TimeSpan(
            Convert.ToInt32(hours),
            Convert.ToInt32(minutes),
            Convert.ToInt32(seconds)
        );
    }

    // it needs iso to go and iso to come
    public static string ToIso(string dateTime)
    {
        if (dateTime.Equals(string.Empty))
        {
            // default assigning this to commitCommitDate lead to
            // strange behaviour
            dateTime = DateTime.UtcNow.ToString();
        }
        try
        {
            var dt = DateTime.Parse(dateTime);
            var ut = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            string isoDt = ut.ToString("s", CultureInfo.InvariantCulture); // + "Z";
            return isoDt;
        }
        catch (FormatException)
        {
        }
        throw new FormatException(TimeFormatException);
    }
}
