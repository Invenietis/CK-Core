using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CK.Core
{
    public sealed partial class CoreApplicationIdentity
    {
        /// <summary>
        /// Builder for <see cref="CoreApplicationIdentity.Instance"/> accessible
        /// from <see cref="Configure(Action{Builder}, bool)"/> or <see cref="TryConfigure(Action{Builder})"/>.
        /// </summary>
        public sealed class Builder
        {
            string _domainName;
            private string _environmentName;
            private string? _partyName;
            private string? _contextDescriptor;

            internal Builder()
            {
                _domainName = "Undefined";
                _environmentName = "";
            }

            internal CoreApplicationIdentity Build()
            {
                Debug.Assert( IsValidIdentifier( DomainName ) );
                Debug.Assert( EnvironmentName == "" || IsValidIdentifier( EnvironmentName ) );
                PartyName ??= PartyNameFromProcessPath();
                return new CoreApplicationIdentity( this );
            }

            static string? PartyNameFromProcessPath()
            {
                if( !String.IsNullOrEmpty( Environment.ProcessPath ) )
                {
                    var replace = new Regex( "[^0-9A-Za-z_]+", RegexOptions.CultureInvariant );
                    var p = replace.Replace( Environment.ProcessPath, "_" );
                    if( p.Length > 0 )
                    {
                        if( p[0] == '_' ) p = p.Substring( 1 );
                        if( p.Length > 0 )
                        {
                            return p;
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// Checks whether the string is a valid <see cref="CoreApplicationIdentity.DomainName"/> or <see cref="CoreApplicationIdentity.PartyName"/>.
            /// Note that <see cref="CoreApplicationIdentity.EnvironmentName"/> can be empty.
            /// <para>
            /// This uses the "^[A-Za-z][0-9A-Za-z_]*$" regular expression.
            /// </para>
            /// </summary>
            /// <remarks>
            /// Since this should be used during the start of the application, the regular expression is not kept in memory.
            /// We use the static <see cref="Regex.Match(string, string, RegexOptions)"/>. 
            /// </remarks>
            /// <param name="candidateId">The identifier.</param>
            /// <returns>True for a valid identifier.</returns>
            public static bool IsValidIdentifier( string candidateId ) => candidateId != null && Regex.Match( candidateId, "^[A-Za-z][0-9A-Za-z_]*$", RegexOptions.CultureInvariant ).Success;

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.DomainName"/>.
            /// <see cref="IsValidIdentifier(string)"/> must be true otherwise an <see cref="ArgumentException"/> is thrown.
            /// </summary>
            public string DomainName
            {
                get => _domainName;
                set
                {
                    Throw.CheckArgument( IsValidIdentifier( value ) );
                    _domainName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.EnvironmentName"/>.
            /// This must be empty or <see cref="IsValidIdentifier(string)"/> must be true otherwise an <see cref="ArgumentException"/> is thrown.
            /// </summary>
            public string EnvironmentName
            {
                get => _environmentName;
                set
                {
                    Throw.CheckArgument( value == "" || IsValidIdentifier( value ) );
                    _environmentName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.PartyName"/>.
            /// <see cref="IsValidIdentifier(string)"/> must be true otherwise an <see cref="ArgumentException"/> is thrown.
            /// </summary>
            public string? PartyName
            {
                get => _partyName;
                set
                {
                    Throw.CheckArgument( value == null || IsValidIdentifier( value ) );
                    _partyName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.ContextDescriptor"/>.
            /// <para>
            /// There is no constraint on this string (but shorter is better) except that
            /// the characters 0 to 8 (NUl, SOH, STX, ETX, EOT, ENQ, ACK, BEL, BSP) are
            /// mapped to their respective angle bracket enclosed string representation
            /// (0x0 is mapped to &lt;NUL&gt;, 0x1 to &lt;SOH&gt;, etc.).
            /// </para>
            /// </summary>
            public string? ContextDescriptor
            {
                get => _contextDescriptor;
                set
                {
                    if( value != null )
                    {
                        // This may not be optimal but does the job.
                        value = value.Replace( "\u0000", "<NUL>"  )
                                     .Replace( "\u0001", "<SOH>" )
                                     .Replace( "\u0002", "<STX>" )
                                     .Replace( "\u0003", "<ETX>" )
                                     .Replace( "\u0004", "<EOT>" )
                                     .Replace( "\u0005", "<ENQ>" )
                                     .Replace( "\u0006", "<ACK>" )
                                     .Replace( "\u0007", "<BEL>" )
                                     .Replace( "\u0008", "<BSP>" );
                    }
                    _contextDescriptor = value;
                }
            }

        }

    }
}
