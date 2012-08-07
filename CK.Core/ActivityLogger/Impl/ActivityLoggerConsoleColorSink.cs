using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    public class ActivityLoggerConsoleColorSink : ActivityLoggerConsoleSink
    {
        ConsoleColor _defaultColor;
        Stack<ConsoleColor> _colors;


        public ActivityLoggerConsoleColorSink()
        {
            _colors = new Stack<ConsoleColor>();
            _defaultColor = System.Console.ForegroundColor;
            _colors.Push(_defaultColor);
        }

        protected override void OnEnterLevel(LogLevel level, string text)
        {
            ConsoleColor c = System.Console.ForegroundColor = GetColor(level);
            _colors.Push(c);
            base.OnEnterLevel(level, text);
        }

        protected override void OnLeaveLevel(LogLevel level)
        {
            base.OnLeaveLevel(level);
            _colors.Pop();
            System.Console.ForegroundColor = _colors.Peek();
        }


        ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Fatal:
                    return ConsoleColor.DarkRed;
                case LogLevel.Info:
                    return ConsoleColor.Cyan;
                case LogLevel.None:
                    return _defaultColor;
                case LogLevel.Trace:
                    return ConsoleColor.Gray;
                case LogLevel.Warn:
                    return ConsoleColor.Yellow;
                default:
                    return _defaultColor;
            }
        }

    }
}
