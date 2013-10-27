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
        /// Sets a function that will be called on group closing to generate a conclusion.
        /// </summary>
        /// <param name="getConclusionText">Function that generates a group conclusion.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        IDisposable ConcludeWith( Func<string> getConclusionText );
    }
}
