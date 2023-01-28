using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CK.Core
{
    public class DualPriorityChannel<T>
    {
        readonly Channel<T> _lowPriorityChannel;
        readonly Channel<Shell> _highPriorityChannel;

        readonly struct Shell
        {
            [MemberNotNullWhen( false, nameof( Item ) )]
            public bool IsMessage { get; }
            public T? Item { get; }

            public Shell( bool isMessage, T? item )
            {
                IsMessage = isMessage;
                Item = item;
            }
        }

        DualPriorityChannel( Channel<T> lowPriorityChannel, Channel<Shell> highPriorityChannel )
        {
            _lowPriorityChannel = lowPriorityChannel;
            _highPriorityChannel = highPriorityChannel;
            HighPriorityWriter = new HighPriorityWriterImpl( this );
            LowPriorityWriter = new LowPriorityWriterImpl( this );
            Reader = new PriorityChannelReader( this );
        }

        public static DualPriorityChannel<T> CreateUnbounded( UnboundedChannelOptions options )
            => new DualPriorityChannel<T>(
                Channel.CreateUnbounded<T>( options ),
                Channel.CreateUnbounded<Shell>( options )
            );

        public static DualPriorityChannel<T> CreateBounded( BoundedChannelOptions lowPrioOptions, BoundedChannelOptions highPrioOptions )
           => new DualPriorityChannel<T>(
               Channel.CreateBounded<T>( lowPrioOptions ),
               Channel.CreateBounded<Shell>( highPrioOptions )
           );

        public ChannelWriter<T> HighPriorityWriter { get; }
        public ChannelWriter<T> LowPriorityWriter { get; }
        public ChannelReader<T> Reader { get; }

        class HighPriorityWriterImpl : ChannelWriter<T>
        {
            readonly DualPriorityChannel<T> _parent;

            public HighPriorityWriterImpl( DualPriorityChannel<T> parent )
            {
                _parent = parent;
            }
            public override bool TryWrite( T item )
                => _parent._highPriorityChannel.Writer.TryWrite( new Shell( false, item ) );

            public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
                => _parent._highPriorityChannel.Writer.WaitToWriteAsync( cancellationToken );
        }

        class LowPriorityWriterImpl : ChannelWriter<T>
        {
            readonly DualPriorityChannel<T> _parent;

            public LowPriorityWriterImpl( DualPriorityChannel<T> parent )
            {
                _parent = parent;
            }

            public override bool TryWrite( T item )
            {
                var res = _parent._lowPriorityChannel.Writer.TryWrite( item );
                if( res ) _parent._highPriorityChannel.Writer.TryWrite( new Shell( true, default ) );
                return res;
            }

            public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
            {
                throw new NotImplementedException();
            }
        }

        class PriorityChannelReader : ChannelReader<T>
        {
            readonly DualPriorityChannel<T> _parent;

            public PriorityChannelReader( DualPriorityChannel<T> parent )
            {
                _parent = parent;
            }

            public override bool TryRead( [MaybeNullWhen( false )] out T item )
            {
                while( true )
                {
                    var res = _parent._highPriorityChannel.Reader.TryRead( out DualPriorityChannel<T>.Shell shellOrItem );
                    if( !res ) return _parent._lowPriorityChannel.Reader.TryRead( out item );
                    if( shellOrItem.IsMessage ) continue;
                    item = shellOrItem.Item;
                    return true;
                }
            }

            public override ValueTask<bool> WaitToReadAsync( CancellationToken cancellationToken = default ) => _parent._highPriorityChannel.Reader.WaitToReadAsync( cancellationToken );
        }

    }
}
