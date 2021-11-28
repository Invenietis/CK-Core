using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CK.Core.Tests
{

    public class CompletableTests
    {
        enum CommandAction
        {
            Success,
            Error,
            Canceled
        }

        static readonly Exception RunException = new Exception( "Run exception." );
        static readonly Exception OverriddenException = new Exception( "Overridden exception." );


        class Command : ICompletable
        {
            public Command()
            {
                CompletionSource = new CompletionSource( this );
            }

            public Command( Random r )
                : this()
            {
                RunAction = (CommandAction)r.Next( 3 );
                SuccessOnCancel = r.Next( 2 ) == 0;
                OnErrorHook = (CommandAction)r.Next( 3 );
                if( r.Next( 2 ) == 0 ) OverriddenExceptionOnError = OverriddenException;
                ExecutionTime = r.Next( 300 );
                UseTrySet = r.Next( 2 ) == 0;
            }

            public readonly CompletionSource CompletionSource;

            public CommandAction RunAction { get; set; }

            public bool SuccessOnCancel { get; set; }

            public CommandAction OnErrorHook { get; set; }

            public Exception? OverriddenExceptionOnError { get; set; }

            public int ExecutionTime { get; set; }

            public bool UseTrySet { get; set; }

            public ICompletion Completion => CompletionSource;

            public void OnCanceled( ref CompletionSource.OnCanceled result )
            {
                if( SuccessOnCancel ) result.SetResult();
                else result.SetCanceled();
            }

            public void OnError( Exception ex, ref CompletionSource.OnError result )
            {
                switch( OnErrorHook )
                {
                    case CommandAction.Error: result.SetException( OverriddenExceptionOnError ?? ex ); break;
                    case CommandAction.Success: result.SetResult(); break;
                    case CommandAction.Canceled: result.SetCanceled(); break;
                    default: throw new NotSupportedException();
                }
            }
        }

        static async Task CommandExecuteAsync( Command c )
        {
            await Task.Delay( c.ExecutionTime ).ConfigureAwait( false );
            switch( c.RunAction )
            {
                case CommandAction.Success: if( c.UseTrySet ) c.CompletionSource.TrySetResult(); else c.CompletionSource.SetResult(); break;
                case CommandAction.Canceled: if( c.UseTrySet ) c.CompletionSource.TrySetCanceled(); else c.CompletionSource.SetCanceled(); break;
                case CommandAction.Error: if( c.UseTrySet ) c.CompletionSource.TrySetException( RunException ); else c.CompletionSource.SetException( RunException ); break;
            }
        }

        [TestCase(100000, 12)]
        public async Task completable_can_hook_the_task_result( int nb, int seed )
        {
            var random = new Random( seed );

            var commands = Enumerable.Range( 0, nb ).Select( _ => new Command( random ) ).ToArray();
            foreach( var c in commands )
            {
                _ = CommandExecuteAsync( c );
            }
            foreach( var c in commands )
            {
                try
                {
                    await c.Completion;
                    c.Completion.IsCompleted.Should().BeTrue();
                }
                catch( OperationCanceledException )
                {
                    (c.RunAction == CommandAction.Canceled || (c.RunAction == CommandAction.Error && c.OnErrorHook == CommandAction.Canceled))
                        .Should().BeTrue();
                }
                catch( Exception ex ) when( ex == RunException )
                {
                    c.RunAction.Should().Be( CommandAction.Error );
                    c.OverriddenExceptionOnError.Should().BeNull();
                }
                catch( Exception ex ) when( ex == OverriddenException )
                {
                    c.RunAction.Should().Be( CommandAction.Error );
                    c.OverriddenExceptionOnError.Should().NotBeNull();
                }
                c.Completion.IsCompleted.Should().BeTrue();

                switch( c.RunAction )
                {
                    case CommandAction.Error:
                        Debug.Assert( c.Completion.OriginalException != null );
                        c.Completion.OriginalException.Message.Should().BeSameAs( RunException.Message );
                        c.Completion.HasFailed.Should().BeTrue();
                        switch( c.OnErrorHook )
                        {
                            case CommandAction.Canceled:
                                c.Completion.Task.Status.Should().Be( TaskStatus.Canceled );
                                break;
                            case CommandAction.Success:
                                c.Completion.Task.Status.Should().Be( TaskStatus.RanToCompletion );
                                break;
                            case CommandAction.Error:
                                Debug.Assert( c.Completion.Task.Exception != null );
                                c.Completion.Task.Status.Should().Be( TaskStatus.Faulted );
                                c.Completion.Task.Exception.Message.Should().Contain( (c.OverriddenExceptionOnError ?? RunException).Message );
                                break;
                        }
                        break;
                    case CommandAction.Success:
                        c.Completion.HasSucceed.Should().BeTrue();
                        break;
                    case CommandAction.Canceled:
                        c.Completion.HasBeenCanceled.Should().BeTrue();
                        c.Completion.Task.Status.Should().Be( c.SuccessOnCancel ? TaskStatus.RanToCompletion : TaskStatus.Canceled );
                        break;
                }
            }
        }

        class CommandWithResult : ICompletable<int>
        {
            public CommandWithResult()
            {
                CompletionSource = new CompletionSource<int>( this );
            }

            public CommandWithResult( Random r )
                : this()
            {
                RunAction = (CommandAction)r.Next( 3 );
                SuccessOnCancel = r.Next( 2 ) == 0;
                OnErrorHook = (CommandAction)r.Next( 3 );
                if( r.Next( 2 ) == 0 ) OverriddenExceptionOnError = OverriddenException;
                ExecutionTime = r.Next( 300 );
                UseTrySet = r.Next( 2 ) == 0;
            }

            public readonly CompletionSource<int> CompletionSource;

            public CommandAction RunAction { get; set; }

            public bool SuccessOnCancel { get; set; }

            public CommandAction OnErrorHook { get; set; }

            public Exception? OverriddenExceptionOnError { get; set; }

            public int ExecutionTime { get; set; }

            public bool UseTrySet { get; set; }

            public ICompletion<int> Completion => CompletionSource;

            public void OnCanceled( ref CompletionSource<int>.OnCanceled result )
            {
                if( SuccessOnCancel ) result.SetResult( 2 );
                else result.SetCanceled();
            }

            public void OnError( Exception ex, ref CompletionSource<int>.OnError result )
            {
                switch( OnErrorHook )
                {
                    case CommandAction.Error: result.SetException( OverriddenExceptionOnError ?? ex ); break;
                    case CommandAction.Success: result.SetResult( 1 ); break;
                    case CommandAction.Canceled: result.SetCanceled(); break;
                    default: throw new NotSupportedException();
                }
            }
        }

        static async Task CommandExecuteAsync( CommandWithResult c )
        {
            await Task.Delay( c.ExecutionTime ).ConfigureAwait( false );
            switch( c.RunAction )
            {
                case CommandAction.Success: if( c.UseTrySet ) c.CompletionSource.TrySetResult( 3712 ); else c.CompletionSource.SetResult( 3712 ); break;
                case CommandAction.Canceled: if( c.UseTrySet ) c.CompletionSource.TrySetCanceled(); else c.CompletionSource.SetCanceled(); break;
                case CommandAction.Error: if( c.UseTrySet ) c.CompletionSource.TrySetException( RunException ); else c.CompletionSource.SetException( RunException ); break;
            }
        }

        [TestCase( 100000, 877 )]
        public async Task completable_with_result_can_hook_the_task_result( int nb, int seed )
        {
            var random = new Random( seed );

            var commands = Enumerable.Range( 0, nb ).Select( _ => new CommandWithResult( random ) ).ToArray();
            foreach( var c in commands )
            {
                _ = CommandExecuteAsync( c );
            }
            foreach( var c in commands )
            {
                try
                {
                    await c.Completion;
                    c.Completion.IsCompleted.Should().BeTrue();
                }
                catch( OperationCanceledException )
                {
                    (c.RunAction == CommandAction.Canceled || (c.RunAction == CommandAction.Error && c.OnErrorHook == CommandAction.Canceled))
                        .Should().BeTrue();
                    c.Completion.Task.Status.Should().Be( TaskStatus.Canceled );
                }
                catch( Exception ex ) when( ex == RunException )
                {
                    c.RunAction.Should().Be( CommandAction.Error );
                    c.OverriddenExceptionOnError.Should().BeNull();
                    c.Completion.Task.Status.Should().Be( TaskStatus.Faulted );
                }
                catch( Exception ex ) when( ex == OverriddenException )
                {
                    c.RunAction.Should().Be( CommandAction.Error );
                    c.OverriddenExceptionOnError.Should().NotBeNull();
                    c.Completion.Task.Status.Should().Be( TaskStatus.Faulted );
                }
                c.Completion.IsCompleted.Should().BeTrue();

                switch( c.RunAction )
                {
                    case CommandAction.Error:
                        Debug.Assert( c.Completion.OriginalException != null );
                        c.Completion.OriginalException.Message.Should().BeSameAs( RunException.Message );
                        c.Completion.HasFailed.Should().BeTrue();
                        switch( c.OnErrorHook )
                        {
                            case CommandAction.Canceled:
                                c.Completion.Task.Status.Should().Be( TaskStatus.Canceled );
                                break;
                            case CommandAction.Success:
                                c.Completion.Task.Status.Should().Be( TaskStatus.RanToCompletion );
                                c.Completion.Result.Should().Be( 1 );
                                break;
                            case CommandAction.Error:
                                Debug.Assert( c.Completion.Task.Exception != null );
                                c.Completion.Task.Status.Should().Be( TaskStatus.Faulted );
                                c.Completion.Task.Exception.Message.Should().Contain( (c.OverriddenExceptionOnError ?? RunException).Message );
                                break;
                        }
                        break;
                    case CommandAction.Success:
                        c.Completion.HasSucceed.Should().BeTrue();
                        c.Completion.Result.Should().Be( 3712 );
                        break;
                    case CommandAction.Canceled:
                        c.Completion.HasBeenCanceled.Should().BeTrue();
                        c.Completion.Task.Status.Should().Be( c.SuccessOnCancel ? TaskStatus.RanToCompletion : TaskStatus.Canceled );
                        if( c.SuccessOnCancel ) c.Completion.Result.Should().Be( 2 );
                        break;
                }
            }
        }


        class SimpleCommand : ICompletable<int>
        {
            public SimpleCommand()
            {
                CompletionSource = new CompletionSource<int>( this );
            }

            public readonly CompletionSource<int> CompletionSource;

            public ICompletion<int> Completion => CompletionSource;

            void ICompletable<int>.OnCanceled( ref CompletionSource<int>.OnCanceled result ) => result.SetCanceled();

            public void OnError( Exception ex, ref CompletionSource<int>.OnError result )
            {
                // This is how OperationCanceledException can be transformed to "normal" cancellation.
                // if( ex is OperationCanceledException ) result.SetCanceled(); else
                result.SetException( ex );
            }
        }

        class SimpleCommandNoResult : ICompletable
        {
            public SimpleCommandNoResult()
            {
                CompletionSource = new CompletionSource( this );
            }

            public readonly CompletionSource CompletionSource;

            public ICompletion Completion => CompletionSource;

            void ICompletable.OnCanceled( ref CompletionSource.OnCanceled result ) => result.SetCanceled();

            public void OnError( Exception ex, ref CompletionSource.OnError result )
            {
                // This is how OperationCanceledException can be transformed to "normal" cancellation.
                // if( ex is OperationCanceledException ) result.SetCanceled(); else
                result.SetException( ex );
            }
        }

        [TestCase( "NoResult", "OperationCanceledException" )]
        [TestCase( "NoResult", "Exception" )]
        [TestCase( "NoResult", "Cancel" )]
        [TestCase( "WithResult", "OperationCanceledException" )]
        [TestCase( "WithResult", "Exception" )]
        [TestCase( "WithResult", "Cancel" )]
        public void unobserved_completion_dont_raise_UnobservedTaskException( string commandType, string error )
        {
            bool raised = false;
            TaskScheduler.UnobservedTaskException += ( sender, e ) => raised = true;

            CreateCompleteAndForgetCommand( commandType, error );

            GC.Collect();
            GC.WaitForPendingFinalizers();

            raised.Should().BeFalse();
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        private static void CreateCompleteAndForgetCommand( string commandType, string error )
        {
            if( commandType == "WithResult" )
            {
                var cmd = new SimpleCommand();
                if( error == "Cancel" ) cmd.CompletionSource.SetCanceled();
                else if( error == "OperationCanceledException" ) cmd.CompletionSource.SetException( new OperationCanceledException() );
                else cmd.CompletionSource.SetException( new Exception( "Pouf" ) );
            }
            else
            {
                var cmd = new SimpleCommandNoResult();
                if( error == "Cancel" ) cmd.CompletionSource.SetCanceled();
                else if( error == "OperationCanceledException" ) cmd.CompletionSource.SetException( new OperationCanceledException() );
                else cmd.CompletionSource.SetException( new Exception( "Pouf" ) );
            }
        }

    }
}
