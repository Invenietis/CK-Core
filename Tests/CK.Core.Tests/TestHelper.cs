using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace Core
{
    static class TestHelper
    {
        static IDefaultActivityLogger _logger;
        static ActivityLoggerConsoleSync _console;
        static string _scriptFolder;

        static TestHelper()
        {
            _console = new ActivityLoggerConsoleSync();
            _logger = DefaultActivityLogger.Create().Register( _console );
        }

        public static IActivityLogger Logger
        {
            get { return _logger; }
        }

        public static bool LogsToConsole
        {
            get { return _logger.RegisteredSinks.Contains( _console ); }
            set
            {
                if( value ) _logger.Register( _console );
                else _logger.Unregister( _console );
            }
        }
    }
}
