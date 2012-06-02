using System;

namespace CK.Storage
{
    /// <summary>
    /// Protects a <see cref="IStructuredWriter"/> write: changes are effective only 
    /// when <see cref="SaveChanges"/> is called.
    /// </summary>
    public interface IProtectedStructuredWriter : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="IStructuredWriter"/> to write to.
        /// </summary>
        IStructuredWriter StructuredWriter { get; }

        /// <summary>
        /// Do save the changes.
        /// </summary>
        void SaveChanges();
    }


}
