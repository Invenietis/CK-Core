using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Interface obtained once a Group has been opened.
    /// </summary>
    public interface IDisposableGroup : IDisposable
    {
        /// <summary>
        /// Sets a temporary topic associated to this group.
        /// The current monitor's topic will be automatically restored when group will be closed.
        /// </summary>
        /// <param name="topicOtherThanGroupText">Explicit topic it it must differ from the group's text.</param>
        /// <returns>This object in order to call <see cref="ConcludeWith"/> or to dispose it to close the group.</returns>
        IDisposableGroup SetTopic( string topicOtherThanGroupText = null );

        /// <summary>
        /// Sets a function that will be called on group closing to generate a conclusion.
        /// </summary>
        /// <param name="getConclusionText">Function that generates a group conclusion.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        IDisposable ConcludeWith( Func<string> getConclusionText );

    }
}
