using System;
using System.Collections.Generic;

namespace CK.Monitoring
{
    /// <summary>
    /// Parented log entry binds an entry to its parent group and can be a missing entry (a line or group opening or closing that we know it exists
    /// but have no data for it or only their <see cref="ILogEntry.LogTime"/>).
    /// </summary>
    public class ParentedLogEntry
    {
        /// <summary>
        /// Parent entry. Null when there is no group above.
        /// </summary>
        public readonly ParentedLogEntry Parent;

        /// <summary>
        /// The entry itself.
        /// </summary>
        public readonly ILogEntry Entry;

        internal ParentedLogEntry( ParentedLogEntry parent, ILogEntry entry )
        {
            Parent = parent;
            Entry = entry;
        }

        /// <summary>
        /// Gets whether this is actually a missing entry (it can be a group opening, closing or a mere line): we do not have data for it, except, may be its <see cref="ILogEntry.LogTime"/>
        /// (if the log time is not known, the <see cref="Entry"/>'s <see cref="ILogEntry.LogTime">LogTime</see> is <see cref="DateTimeStamp.Unknown"/>).
        /// </summary>
        public bool IsMissing
        {
            get { return LogEntry.IsMissingLogEntry( Entry ); }
        }

        /// <summary>
        /// Collects path of <see cref="ILogEntry"/> in a reusable list (the buffer is <see cref="List{T}.Clear">cleared</see> first).
        /// </summary>
        /// <param name="reusableBuffer">List that will be cleared and filled with parents.</param>
        /// <param name="addThis">Set it to true to append to also add this entry.</param>
        public void GetPath( List<ILogEntry> reusableBuffer, bool addThis = false )
        {
            if( reusableBuffer == null ) throw new ArgumentNullException( "reusableBuffer" );
            reusableBuffer.Clear();
            CollectPath( p => reusableBuffer.Add( p.Entry ), addThis );
        }

        /// <summary>
        /// Collects the path of this <see cref="ParentedLogEntry"/>, optionally terminated with this entry.
        /// </summary>
        /// <param name="collector">Action for each item.</param>
        /// <param name="addThis">Set it to true to append to also call the collector with this entry.</param>
        public void CollectPath( Action<ParentedLogEntry> collector, bool addThis = false )
        {
            if( collector == null ) throw new ArgumentNullException( "collector" );

            if( Parent != null ) Parent.DoGetPath( collector );
            if( addThis ) collector( this );
        }

        void DoGetPath( Action<ParentedLogEntry> collector )
        {
            if( Parent != null ) Parent.DoGetPath( collector );
            collector( this );
        }
    }
}
