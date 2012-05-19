using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Describes a conclusion emitted by a <see cref="IActivityLoggerClientBase"/>.
    /// </summary>
    public struct ActivityLogGroupConclusion
    {
        /// <summary>
        /// The log client that emitted the conclusion. Never null.
        /// </summary>
        public readonly IActivityLoggerClientBase Emitter;

        /// <summary>
        /// The conclusion (never null). Its <see cref="Object.ToString"/>
        /// method SHOULD provide a correct display of the conclusion (it
        /// can be a string).
        /// </summary>
        public readonly object Conclusion;

        /// <summary>
        /// Initializes a new conclusion.
        /// </summary>
        /// <param name="emitter">Must not be null.</param>
        /// <param name="conclusion">Must not be null and its ToString should not be null, empty nor white space.</param>
        public ActivityLogGroupConclusion( IActivityLoggerClientBase emitter, object conclusion )
            : this( conclusion, emitter )
        {
            if( emitter == null ) throw new ArgumentNullException( "emitter" );
            if( conclusion == null ) throw new ArgumentException( "conclusion" );
        }

        internal ActivityLogGroupConclusion( object conclusion, IActivityLoggerClientBase emitter )
        {
            Debug.Assert( conclusion != null && emitter != null );
            Emitter = emitter;
            Conclusion = conclusion;
        }

        /// <summary>
        /// Overriden to return <see cref="Conclusion"/>.ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Conclusion.ToString();
        }
    }

}
