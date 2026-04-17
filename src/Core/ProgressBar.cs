// TO-DO: Stop coloring emptyChar

namespace Stakes.Core;

using Pastel;


class ProgressBar
{
    private static char _emptyChar;
    public static char EmptyChar
    {
        get => _emptyChar;
        set => _emptyChar = value;
    }
    private static char _mainChar;
    public static char MainChar { get => _mainChar; set => _mainChar = value; }

    private static char _altChar;
    public static char AltChar { get => _altChar; set => _altChar = value; }

    private static int _size;
    public static int Size { get => _size; set => _size = value; }

    private static string[] _colors = [];
    public static string[] Colors { get => _colors; set => _colors = value; }

    public static void Initialize(
        char? emptyChar = null,
        char? mainChar = null,
        char? altChar = null,
        int? size = null,
        string[]? colors = null
    )
    {
        if (size != null)
        {
            if (size < 5)
            {
                throw new Exceptions.ProgressBarSizeException();
            }
            _size = (int)size;
        }
        if (emptyChar != null)
        {
            _emptyChar = (char)emptyChar;
        }
        if (mainChar != null)
        {
            _mainChar = (char)mainChar;
        }
        if (altChar != null)
        {
            _altChar = (char)altChar;
        }
        if (colors != null)
        {
            _colors = colors;
        }
    }

    // value: how much we have
    // full: how much we have when progressBar is at 100%
    public static string GetProgressBar(double value, double full)
    {
        int maxSize = _size - 4;

        if (value <= 0 || full <= 0)
        {
            return "[ " + new string(_emptyChar, maxSize) + " ]";
        }
        int charCount = Convert.ToInt32(maxSize * value / full);

        // How much is left after compressing charCount into maxSize
        int excedentCount = charCount % maxSize;

        // How many times our charCount surpassed fully maxSize
        int excededCount = charCount / maxSize;

        // Define alt char
        char altChar;
        if (charCount > maxSize)
        {
            altChar = _altChar;
        }
        else
        {
            altChar = _emptyChar;
        }

        // Get part sizes
        if (excedentCount == 0)
        {
            charCount = maxSize;
        }
        else
        {
            charCount = excedentCount;
        }

        int altCharCount = maxSize - charCount;

        // Get colors
        int colorsCount = _colors.Length;

        int i = excededCount % colorsCount;
        int j = (excededCount + 1) % colorsCount;

        var mainFiller = new string(_mainChar, charCount).Pastel(_colors[i]);
        var altFiller = new string(altChar, altCharCount).Pastel(_colors[j]);

        var progressBar = $"[ {mainFiller}{altFiller} ]";
        return progressBar;
    }
}
