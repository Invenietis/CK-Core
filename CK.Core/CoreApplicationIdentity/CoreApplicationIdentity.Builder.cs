using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CK.Core
{
    public sealed partial class CoreApplicationIdentity
    {
        /// <summary>
        /// Builder for <see cref="CoreApplicationIdentity.Instance"/> accessible
        /// from <see cref="Configure(Action{Builder})"/>.
        /// </summary>
        public sealed class Builder
        {
            // Do not cache nor compile the regular expression here.
            readonly Regex _rId;
            string _domainName;
            private string _environmentName;
            private string? _partyName;

            internal Builder()
            {
                _domainName = "Undefined";
                _environmentName = "";
                // 8 bytes of entropy should be enough. (Same as the monitor identifiers.)
                InstanceId = Util.GetRandomBase64UrlString( 11 );
                _rId = new Regex( "[A-Za-z][0-9A-Za-z_]*", RegexOptions.CultureInvariant );
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
                    var replace = new Regex( "^[0-9A-Za-z_]+", RegexOptions.CultureInvariant );
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

            bool IsValidIdentifier( string id ) => id != null && _rId.Match( id ).Success;

            /// <summary>
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.DomainName"/>.
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
            /// Gets or sets the eventual <see cref="CoreApplicationIdentity.ContextIdentifier"/>.
            /// </summary>
            public string? ContextIdentifier { get; set; }

            /// <summary>
            /// Gets the <see cref="CoreApplicationIdentity.InstanceId"/>.
            /// </summary>
            public string InstanceId { get; }

        }

    }
}
