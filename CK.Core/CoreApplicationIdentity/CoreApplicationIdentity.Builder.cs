using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
                _domainName = DefaultDomainName;
                _environmentName = DefaultEnvironmentName;
            }

            internal CoreApplicationIdentity Build()
            {
                Debug.Assert( IsValidDomainName( DomainName ) );
                Debug.Assert( IsValidEnvironmentName( EnvironmentName ) );
                PartyName ??= PartyNameFromProcessPath( Environment.ProcessPath );
                Debug.Assert( IsValidPartyName( PartyName ) );
                return new CoreApplicationIdentity( this );
            }

            /// <summary>
            /// Tries to compute a <see cref="IsValidPartyName(ReadOnlySpan{char})"/> from any string.
            /// Ultimately returns <see cref="DefaultPartyName"/>.
            /// </summary>
            /// <param name="processPath">Called with <see cref="Environment.ProcessPath"/>.</param>
            /// <returns>A party name to use.</returns>
            public static string PartyNameFromProcessPath( string? processPath )
            {
                if( !String.IsNullOrEmpty( processPath ) )
                {
                    var replace = new Regex( "[^0-9A-Za-z_-]+", RegexOptions.CultureInvariant );
                    var p = replace.Replace( processPath, "_" );
                    replace = new Regex( "__+", RegexOptions.CultureInvariant );
                    p = replace.Replace( p, "_" );
                    if( p.Length > PartyNameMaxLength )
                    {
                        // Crappy help to have a "better" PartyName...
                        p = p.Replace( "_testhost_exe", "" );
                        p = p.Replace( "_bin_", "_" );
                        p = p.Substring( p.Length - PartyNameMaxLength );
                    }
                    int skipHead = 0;
                    while( skipHead < p.Length && (char.IsDigit( p[skipHead] ) || p[skipHead] == '_' || p[skipHead] == '-') ) ++skipHead;
                    int skipEnd = p.Length;
                    while( --skipEnd >= skipHead && (p[skipEnd] == '_' || p[skipEnd] == '-') ) ;
                    int len = skipEnd - skipHead + 1;
                    if( len > 0 ) return p.Substring( skipHead, len );
                }
                return DefaultPartyName;
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.DomainName"/>.
            /// <see cref="IsValidDomainName(ReadOnlySpan{char})"/> must be true otherwise an <see cref="ArgumentException"/> is thrown.
            /// </summary>
            public string DomainName
            {
                get => _domainName;
                set
                {
                    Throw.CheckNotNullArgument( value );
                    Throw.CheckArgument( IsValidDomainName( value ) );
                    _domainName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.EnvironmentName"/>.
            /// <see cref="IsValidEnvironmentName(ReadOnlySpan{char})"/> must be true otherwise an <see cref="ArgumentException"/> is thrown.
            /// </summary>
            public string EnvironmentName
            {
                get => _environmentName;
                set
                {
                    Throw.CheckNotNullArgument( value );
                    Throw.CheckArgument( IsValidEnvironmentName( value ) );
                    _environmentName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.PartyName"/>.
            /// <see cref="IsValidPartyName(ReadOnlySpan{char})"/> must be true and when not null,
            /// must not be longer than <see cref="PartyNameMaxLength"/> otherwise an <see cref="ArgumentException"/> is thrown.
            /// <para>
            /// Setting a "$Name" is allowed: the '$' prefix is automaticaaly removed.
            /// </para>
            /// </summary>
            public string? PartyName
            {
                get => _partyName;
                set
                {
                    Throw.CheckNotNullArgument( value );
                    Throw.CheckArgument( IsValidPartyName( value ) );
                    if( value[0] == '$' ) value = value.Substring( 1 ); 
                    _partyName = value;
                }
            }

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.ContextDescriptor"/>.
            /// When not null must not be longer than <see cref="ContextDescriptorMaxLength"/> otherwise an <see cref="ArgumentException"/> is thrown.
            /// <para>
            /// There is no constraint on this string (but shorter is better). Note that the
            /// characters 0 to 8 (NUl, SOH, STX, ETX, EOT, ENQ, ACK, BEL, BSP) are
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
                        Throw.CheckArgument( value.Length <= ContextDescriptorMaxLength );
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
