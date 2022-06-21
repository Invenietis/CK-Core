using System;
using System.Diagnostics;
using System.Text;

namespace CK.Core
{
    public ref partial struct ROSpanCharMatcher
    {
        ref struct Dumper
        {
            readonly Span<(int Pos, int Line, int Col, int Depth, bool IsError, string Expectation, string CallerName)> _errors;
            readonly StringBuilder _builder;
            readonly bool _withMethodName;

            public Dumper( ref ROSpanCharMatcher m, int maxDepth, bool withMethodName )
            {
                _errors = m.GetErrors( maxDepth ).Span;
                _builder = new StringBuilder();
                _withMethodName = withMethodName;
                Debug.Assert( _errors.Length > 0 );
            }

            public void Dump()
            {
                int _index = 0;
                var (pos, line, col, depth, isError, e, callerName) = _errors[_index];
                do
                {
                    var (subLC, sub) = DoNew( depth, isError, line, col, e, callerName );
                    int topDepth = depth;
                    int lastDepth = depth;
                    bool curIsError = isError;
                    int curPos = pos;
                    bool isOr = false;
                    while( ++_index < _errors.Length )
                    {
                        (pos, line, col, depth, isError, e, callerName) = _errors[_index];
                        if( curPos == pos )
                        {
                            if( curIsError == isError )
                            {
                                int deltaD = depth - topDepth;
                                if( deltaD < 0 ) deltaD = 0;
                                isOr |= !isError && depth <= lastDepth;
                                DoContinueOnSamePosAndKind( sub, isOr, deltaD, e, callerName );
                                lastDepth = depth;
                            }
                            else
                            {
                                sub = DoContinueOnSamePos( subLC, isError, e, callerName );
                            }
                        }
                        else break;
                    }
                }
                while( _index < _errors.Length );
            }

            int DoContinueOnSamePos( int subLC, bool isError, string e, string callerName )
            {
                _builder.AppendLine();
                int sub = _builder.Length;
                _builder.Append( ' ', subLC );
                if( isError )
                {
                    _builder.Append( " - Error: " );
                    sub = _builder.Length - sub;
                }
                else
                {
                    _builder.Append( " - Expected: " );
                    sub = _builder.Length - sub;
                }
                AppendMessage( e, callerName );
                return sub;
            }

            void DoContinueOnSamePosAndKind( int sub, bool isOr, int deltaDepth, string e, string callerName )
            {
                if( isOr ) 
                {
                    _builder.AppendLine().Append( ' ', sub - 4 ).Append( "Or: " ).Append( ' ', deltaDepth*2 );
                }
                else
                {
                    _builder.AppendLine().Append( ' ', deltaDepth*2 + sub );
                }
                AppendMessage(e, callerName);
            }

            (int,int) DoNew( int depth, bool isError, int line, int col, string e, string callerName )
            {
                int sub = AppenNewLine();
                _builder.Append( ' ', depth*2 ).Append( '@' ).Append( line ).Append( ',' ).Append( col );
                int subLC = _builder.Length - sub;
                if( isError )
                {
                    _builder.Append( " - Error: " );
                    sub = _builder.Length - sub;
                }
                else
                {
                    _builder.Append( " - Expected: " );
                    sub = _builder.Length - sub;
                }
                AppendMessage( e, callerName );
                return (subLC, sub);
            }

            readonly int AppenNewLine()
            {
                int sub = _builder.Length;
                if( sub != 0 )
                {
                    _builder.AppendLine();
                    sub = _builder.Length;
                }

                return sub;
            }

            void AppendMessage( string e, string callerName )
            {
                _builder.Append( e );
                if( _withMethodName && !ReferenceEquals( e, callerName ) )
                {
                    _builder.Append( " (" ).Append( callerName ).Append( ')' );
                }
            }

            public override string ToString() => _builder.ToString();
        }

        /// <summary>
        /// Gets the errors as a multi line string with line and columns.
        /// </summary>
        /// <param name="withMethodName">False to not display the name of the caller method.</param>
        /// <param name="maxDepth">Optional depth restriction.</param>
        /// <returns>The error message. The empty string if <see cref="HasError"/> is false.</returns>
        public string GetErrorMessage( bool withMethodName = true, int maxDepth = 0 )
        {
            if( !HasError ) return String.Empty;
            var d = new Dumper( ref this, maxDepth, withMethodName );
            d.Dump();
            return d.ToString();
        }


    }
}
