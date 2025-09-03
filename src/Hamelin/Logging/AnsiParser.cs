// This is a copy of the .Net internal AnsiParser class used in Microsoft.Extensions.Logging.Console
// https://github.com/dotnet/runtime/blob/release/8.0/src/libraries/Microsoft.Extensions.Logging.Console/src/AnsiParser.cs
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Hamelin.Logging;

internal sealed class AnsiParser
{
    private readonly Action<string, int, int, ConsoleColor?, ConsoleColor?> _onParseWrite;

    public AnsiParser(Action<string, int, int, ConsoleColor?, ConsoleColor?> onParseWrite)
    {
        ArgumentNullException.ThrowIfNull(onParseWrite);

        _onParseWrite = onParseWrite;
    }

    /// <summary>
    /// Parses a subset of display attributes
    /// Set Display Attributes
    /// Set Attribute Mode [{attr1};...;{attrn}m
    /// Sets multiple display attribute settings. The following lists standard attributes that are getting parsed:
    /// 1 Bright
    /// Foreground Colours
    /// 30 Black
    /// 31 Red
    /// 32 Green
    /// 33 Yellow
    /// 34 Blue
    /// 35 Magenta
    /// 36 Cyan
    /// 37 White
    /// Background Colours
    /// 40 Black
    /// 41 Red
    /// 42 Green
    /// 43 Yellow
    /// 44 Blue
    /// 45 Magenta
    /// 46 Cyan
    /// 47 White
    /// </summary>
    public void Parse(string message)
    {
        int startIndex = -1;
        int length = 0;
        ConsoleColor? foreground = null;
        ConsoleColor? background = null;
        var span = message.AsSpan();
        const char escapeChar = '\u001b';
        bool isBright = false;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == escapeChar && span.Length >= i + 4 && span[i + 1] == '[')
            {
                int escapeCode;
                if (span[i + 3] == 'm')
                {
                    // Example: \u001b[1m
                    if (IsDigit(span[i + 2]))
                    {
                        escapeCode = span[i + 2] - '0';
                        if (startIndex != -1)
                        {
                            _onParseWrite(message, startIndex, length, background, foreground);
                            startIndex = -1;
                            length = 0;
                        }

                        if (escapeCode == 1)
                            isBright = true;
                        i += 3;
                        continue;
                    }
                }
                else if (span.Length >= i + 5 && span[i + 4] == 'm')
                {
                    // Example: \u001b[40m
                    if (IsDigit(span[i + 2]) && IsDigit(span[i + 3]))
                    {
                        escapeCode = (span[i + 2] - '0') * 10 + (span[i + 3] - '0');
                        if (startIndex != -1)
                        {
                            _onParseWrite(message, startIndex, length, background, foreground);
                            startIndex = -1;
                            length = 0;
                        }

                        if (TryGetForegroundColor(escapeCode, isBright, out ConsoleColor? color))
                        {
                            foreground = color;
                            isBright = false;
                        }
                        else if (TryGetBackgroundColor(escapeCode, out color))
                        {
                            background = color;
                        }

                        i += 4;
                        continue;
                    }
                }
            }

            if (startIndex == -1)
            {
                startIndex = i;
            }

            int nextEscapeIndex = -1;
            if (i < message.Length - 1)
            {
                nextEscapeIndex = message.IndexOf(escapeChar, i + 1);
            }

            if (nextEscapeIndex < 0)
            {
                length = message.Length - startIndex;
                break;
            }

            length = nextEscapeIndex - startIndex;
            i = nextEscapeIndex - 1;
        }

        if (startIndex != -1)
        {
            _onParseWrite(message, startIndex, length, background, foreground);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(char c) => (uint)(c - '0') <= ('9' - '0');

    internal const string DefaultForegroundColor = "\u001b[39m\u001b[22m"; // reset to default foreground color
    internal const string DefaultBackgroundColor = "\u001b[49m"; // reset to the background color

    internal static string GetForegroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[30m",
            ConsoleColor.DarkRed => "\u001b[31m",
            ConsoleColor.DarkGreen => "\u001b[32m",
            ConsoleColor.DarkYellow => "\u001b[33m",
            ConsoleColor.DarkBlue => "\u001b[34m",
            ConsoleColor.DarkMagenta => "\u001b[35m",
            ConsoleColor.DarkCyan => "\u001b[36m",
            ConsoleColor.Gray => "\u001b[37m",
            ConsoleColor.Red => "\u001b[1m\u001b[31m",
            ConsoleColor.Green => "\u001b[1m\u001b[32m",
            ConsoleColor.Yellow => "\u001b[1m\u001b[33m",
            ConsoleColor.Blue => "\u001b[1m\u001b[34m",
            ConsoleColor.Magenta => "\u001b[1m\u001b[35m",
            ConsoleColor.Cyan => "\u001b[1m\u001b[36m",
            ConsoleColor.White => "\u001b[1m\u001b[37m",
            _ => DefaultForegroundColor // default foreground color
        };
    }

    internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[40m",
            ConsoleColor.DarkRed => "\u001b[41m",
            ConsoleColor.DarkGreen => "\u001b[42m",
            ConsoleColor.DarkYellow => "\u001b[43m",
            ConsoleColor.DarkBlue => "\u001b[44m",
            ConsoleColor.DarkMagenta => "\u001b[45m",
            ConsoleColor.DarkCyan => "\u001b[46m",
            ConsoleColor.Gray => "\u001b[47m",
            _ => DefaultBackgroundColor // Use default background color
        };
    }

    private static bool TryGetForegroundColor(int number, bool isBright, out ConsoleColor? color)
    {
        color = number switch
        {
            30 => ConsoleColor.Black,
            31 => isBright ? ConsoleColor.Red : ConsoleColor.DarkRed,
            32 => isBright ? ConsoleColor.Green : ConsoleColor.DarkGreen,
            33 => isBright ? ConsoleColor.Yellow : ConsoleColor.DarkYellow,
            34 => isBright ? ConsoleColor.Blue : ConsoleColor.DarkBlue,
            35 => isBright ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta,
            36 => isBright ? ConsoleColor.Cyan : ConsoleColor.DarkCyan,
            37 => isBright ? ConsoleColor.White : ConsoleColor.Gray,
            _ => null
        };
        return color != null || number == 39;
    }

    private static bool TryGetBackgroundColor(int number, out ConsoleColor? color)
    {
        color = number switch
        {
            40 => ConsoleColor.Black,
            41 => ConsoleColor.DarkRed,
            42 => ConsoleColor.DarkGreen,
            43 => ConsoleColor.DarkYellow,
            44 => ConsoleColor.DarkBlue,
            45 => ConsoleColor.DarkMagenta,
            46 => ConsoleColor.DarkCyan,
            47 => ConsoleColor.Gray,
            _ => null
        };
        return color != null || number == 49;
    }
}
