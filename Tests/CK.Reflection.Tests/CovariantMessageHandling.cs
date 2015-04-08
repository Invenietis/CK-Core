using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Reflection.Tests
{

    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class CovariantMessageHandling
    {
        interface IMessageHandler
        {
        }

        interface IMessageHandler<in T> : IMessageHandler where T : class
        {
            void Handle( T m );
        }

        class MessageHandlerMap
        {
            readonly ConcurrentDictionary<Type,Delegate[]> _cache;
            readonly ConcurrentDictionary<Type,byte> _availableHandlers;

            public MessageHandlerMap()
            {
                _cache = new ConcurrentDictionary<Type, Delegate[]>();
                _availableHandlers = new ConcurrentDictionary<Type, byte>();
            }

            public void AddHandlerType( Type messageHandlerType )
            {
                if( _availableHandlers.TryAdd( messageHandlerType, 0 ) )
                {
                    _cache.Clear();
                }
            }
            
            public void AddHandlerType( IEnumerable<Type> messageHandlerTypes )
            {
                bool atLeastOne = false;
                foreach( var h in messageHandlerTypes )
                {
                    atLeastOne |= _availableHandlers.TryAdd( h, 0 );
                }
                if( atLeastOne )
                {
                    _cache.Clear();
                }
            }

            public void AddHandlerType( params Type[] messageHandlerType )
            {
                AddHandlerType( (IEnumerable<Type>)messageHandlerType );
            }

            public void RemoveHandlerType( Type messageHandlerType )
            {
                byte dummy;
                if( _availableHandlers.TryRemove( messageHandlerType, out dummy ) )
                {
                    _cache.Clear();
                }
            }

            public void ClearCachedHandlers()
            {
                _cache.Clear();
            }

            public void ClearHandlerTypes()
            {
                _availableHandlers.Clear();
                _cache.Clear();
            }

            public void HandleMessage( object message )
            {
                var messageType = message.GetType();
                var handlers = _cache.GetOrAdd( messageType, t => GetCovariantHandlers( t ) );
                foreach( var d in handlers )
                {
                    d.DynamicInvoke( message );
                }
            }

            Delegate[] GetCovariantHandlers( Type t )
            {
                return _availableHandlers.Keys.Select( h => new
                {
                    Delegates = h.GetType().GetInterfaces()
                                            .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IMessageHandler<> ) )
                                            .Select( i => new { HInterface = i, HType = i.GetGenericArguments()[0] } )
                                            .Where( i => Reflection.ReflectionHelper.CovariantMatch( i.HType, t ) )
                                            //.Select( i =>
                                            //{
                                            //    Console.WriteLine( "T = {0}, Message = {1}", i.HType, t );
                                            //    return i;
                                            //} )
                                            .Select( i => h.GetType().GetInterfaceMap( i.HInterface ).TargetMethods[0] )
                                            .Select( m => Delegate.CreateDelegate( typeof( Action<> ).MakeGenericType( t ), h, m ) )
                } )
                .SelectMany( o => o.Delegates )
                .ToArray();
            }

        }

        class StringHandler : IMessageHandler<string>
        {
            public void Handle( string m )
            {
                Console.WriteLine( "A string has been handled: {0}", m );
            }
        }

        class ObjectHandler : IMessageHandler<object>
        {
            public void Handle( object m )
            {
                Console.WriteLine( "An object has been handled: {0}", m );
            }
        }

        class TagHandler : IMessageHandler<IMessageTag>
        {
            public void Handle( IMessageTag m )
            {
                Console.WriteLine( "A MessageTag has been handled: {0}", m.Tag );
            }
        }

        class SimpleMessageHandler : IMessageHandler<SimpleMessage>
        {
            public void Handle( SimpleMessage m )
            {
                Console.WriteLine( "A SimpleMessage has been handled: {0}", m.Name );
            }
        }

        class GenericMultiHandler : IMessageHandler<IEnumerable<object>>
        {
            public void Handle( IEnumerable<object> multi )
            {
                Console.WriteLine( "A generic multiple message has been handled: {0}", multi.Count() );
            }
        }
        
        class MultiTagHandler : IMessageHandler<IEnumerable<IMessageTag>>
        {
            public void Handle( IEnumerable<IMessageTag> multi )
            {
                Console.WriteLine( "An enumerable of MessageTag has been handled: {0}", multi.Count() );
            }
        }


        class LaTotaleHandler : IMessageHandler<IEnumerable<object>>, 
                                IMessageHandler<SimpleMessage>, 
                                IMessageHandler<IMessageTag>
        {
            public void Handle( IEnumerable<object> multi )
            {
                Console.WriteLine( "LaTotaleHandler: A generic multiple message has been handled: {0}", multi.Count() );
            }

            public void Handle( SimpleMessage m )
            {
                Console.WriteLine( "LaTotaleHandler: A SimpleMessage has been handled: {0}", m.Name );
            }

            // Explicit implementation must work:
            void IMessageHandler<IMessageTag>.Handle( IMessageTag m )
            {
                Console.WriteLine( "LaTotaleHandler: A MessageTag has been handled: {0}", m.Tag );
            }
        }

        interface IMessageTag
        {
            int Tag { get; set; }
        }
        
        class SimpleMessage : IMessageTag
        {
            public string Name { get; set; }

            public int Tag { get; set; }
        }

        class ExtendedMessage : SimpleMessage
        {
            public int Power { get; set; }
        }

        class OtherMessage : IMessageTag
        {
            public string Name { get; set; }
            
            public int Tag { get; set; }
        }

        IMessageHandler[] _handlers = new IMessageHandler[] 
        { 
            new StringHandler(), 
            new ObjectHandler(), 
            new TagHandler(), 
            new SimpleMessageHandler(),
            new GenericMultiHandler(),
            new MultiTagHandler(),
            new LaTotaleHandler()
        };

        void GenericHandle( object message )
        {
            var handlers = GetCovariantHandlers( message.GetType(), _handlers );
            foreach( var d in handlers )
            {
                d.DynamicInvoke( message );
            }
        }

        [Test]
        public void handling_string_message_IS_NOT_POSSIBLE()
        {
            Assert.Throws<ArgumentException>( () => GenericHandle( "Hello" ) );
        }

        [Test]
        public void handling_SimpleMessage_message()
        {
            GenericHandle( new SimpleMessage() { Name = "SimpleMessage" } );
        }

        [Test]
        public void handling_OtherMessage_message()
        {
            GenericHandle( new OtherMessage() { Name = "OtherMessage" } );
        }

        [Test]
        public void handling_MultiTag_message()
        {
            var m = new IMessageTag[] { new SimpleMessage(), new OtherMessage() };
            GenericHandle( m );
        }

        [Test]
        public void handling_enumerable_of_string_message()
        {
            var m = new string[] { "Hello", "World" };
            GenericHandle( m );
        }

        [Test]
        public void handling_dynamically_types_message()
        {
            IMessageHandler[] handlers = new IMessageHandler[] { new StringHandler(), new ObjectHandler() };
            var handlersForString = GetCovariantHandlers( typeof( string ), handlers ).ToArray();
            foreach( var d in handlersForString )
            {
                d.DynamicInvoke( "Hello" );
            }
            var handlersForObject = GetCovariantHandlers( typeof( object ), handlers ).ToArray();
            foreach( var d in handlersForObject )
            {
                d.DynamicInvoke( this );
            }
        }

        IEnumerable<Delegate> GetCovariantHandlers( Type t, IEnumerable<IMessageHandler> handlers )
        {
            return handlers.Select( h => new
            {
                Delegates = h.GetType().GetInterfaces()
                                        .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IMessageHandler<> ) )
                                        .Select( i => new { HInterface = i, HType = i.GetGenericArguments()[0] } )
                                        .Where( i => Reflection.ReflectionHelper.CovariantMatch( i.HType, t ) )
                                        //.Select( i =>
                                        //{
                                        //    Console.WriteLine( "T = {0}, Message = {1}", i.HType, t );
                                        //    return i;
                                        //} )
                                        .Select( i => h.GetType().GetInterfaceMap( i.HInterface ).TargetMethods[0] )
                                        .Select( m => Delegate.CreateDelegate( typeof( Action<> ).MakeGenericType( t ), h, m ) )
            } )
            .SelectMany( o => o.Delegates );
        }
    }
}
