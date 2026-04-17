namespace Stakes.Core;


class TimeParser
{
    public static TimeSpan ParseTime(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new Exceptions.TimeFormatException();
        }

        input = input.Replace(" ", "").ToLower();

        int hours = 0, minutes = 0, seconds = 0;
        string number = "";

        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                number += c;
                continue;
            }

            if (string.IsNullOrEmpty(number))
            {
                throw new Exceptions.TimeFormatException();
            }

            int value = int.Parse(number);

            switch (c)
            {
                case 'h':
                    hours += value;
                    break;
                case 'm':
                    minutes += value;
                    break;
                case 's':
                    seconds+= value;
                    break;
                default:
                    throw new Exceptions.TimeFormatException();
            }

            number = "";
        }

        if (!string.IsNullOrEmpty(number))
        {
            throw new Exceptions.TimeFormatException();
        }

        if (hours == 0 && minutes == 0 && seconds == 0)
        {
            throw new Exceptions.TimeFormatException();
        }

        return new TimeSpan(hours, minutes, seconds);
    }
}
