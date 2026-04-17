namespace Stakes.Core;

using System.Text;


class StringFormatter
{
    public static string FormatChanged(string from, string to)
    {
        return $"{from} → {to}";
    }

    public static string PadSides(string text, int size)
    {
        int sidePad = size - text.Length;
        if (sidePad <= 0)
        {
            return text;
        }
        var builder = new StringBuilder(new string(' ', sidePad / 2), size);
        builder.Append(text);
        return builder.ToString().PadRight(size, ' ');
    }
}
