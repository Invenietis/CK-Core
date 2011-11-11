using System;
using System.Linq;

namespace CK.Core
{

    /// <summary>
    /// Provides extension methods for <see cref="IActivityLogger"/>.
    /// </summary>
    public static class ActivityLoggerExtension
    {

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="l">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">The log level of the group.</param>
        /// <param name="text">The text associated to the opening of the log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        static public IDisposable OpenGroup( this IActivityLogger l, LogLevel level, string text )
        {
            return l.OpenGroup( level, null, text );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="l">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        static public IDisposable OpenGroup( this IActivityLogger l, LogLevel level, Func<string> getConclusionText, string format, params object[] arguments )
        {
            return l.OpenGroup( level, getConclusionText, String.Format( format, arguments ) );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="l">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        static public IDisposable OpenGroup( this IActivityLogger l, LogLevel level, string format, params object[] arguments )
        {
            return l.OpenGroup( level, null, String.Format( format, arguments ) );
        }

        #region Trace

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a trace.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, string text )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, text );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, string format, object arg0 )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, string format, object arg0, object arg1 )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1, arg2 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, string format, params object[] args )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, String.Format( format, args ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger a, Func<string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, text() );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T>( this IActivityLogger a, T param, Func<T, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, text( param ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T1, T2>( this IActivityLogger a, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Trace ) a.UnfilteredLog( LogLevel.Trace, text( param1, param2 ) );
            return a;
        }
        #endregion

        #region Info

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an info.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, string text )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, text );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, string format, object arg0 )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, String.Format( format, arg0 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, string format, object arg0, object arg1 )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1, arg2 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, string format, params object[] args )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, String.Format( format, args ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger a, Func<string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, text() );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T>( this IActivityLogger a, T param, Func<T, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, text( param ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T1, T2>( this IActivityLogger a, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Info ) a.UnfilteredLog( LogLevel.Info, text( param1, param2 ) );
            return a;
        }
        #endregion

        #region Warn

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a warning.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, string text )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, text );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, string format, object arg0 )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, string format, object arg0, object arg1 )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1, arg2 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, string format, params object[] args )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, String.Format( format, args ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger a, Func<string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, text() );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T>( this IActivityLogger a, T param, Func<T, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, text( param ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T1, T2>( this IActivityLogger a, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Warn ) a.UnfilteredLog( LogLevel.Warn, text( param1, param2 ) );
            return a;
        }
        #endregion

        #region Error

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, string text )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, text );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, string format, object arg0 )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, String.Format( format, arg0 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, string format, object arg0, object arg1 )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1, arg2 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, string format, params object[] args )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, String.Format( format, args ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger a, Func<string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, text() );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T>( this IActivityLogger a, T param, Func<T, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, text( param ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T1, T2>( this IActivityLogger a, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Error ) a.UnfilteredLog( LogLevel.Error, text( param1, param2 ) );
            return a;
        }
        #endregion

        #region Fatal

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a fatal error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, string text )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, text );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, string format, object arg0 )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, string format, object arg0, object arg1 )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1, arg2 ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, string format, params object[] args )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, String.Format( format, args ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger a, Func<string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, text() );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T>( this IActivityLogger a, T param, Func<T, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, text( param ) );
            return a;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="a">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T1, T2>( this IActivityLogger a, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)a.Filter <= (int)LogLevel.Fatal ) a.UnfilteredLog( LogLevel.Fatal, text( param1, param2 ) );
            return a;
        }
        #endregion

    }
}
