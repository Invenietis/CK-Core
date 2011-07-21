using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Interop
{
    /// <summary>
    /// This attribute marks an interface to native methods: the static <see cref="PInvoker.GetInvoker{T}"/> obtains a concrete implementation (at runtime).
    /// Methods of the interface are decorated with <see cref="CK.Interop.DllImportAttribute">CK.Interop.DllImportAttribute</see>
    /// that may override the optional default <see cref="DefaultDllNameGeneric"/>, <see cref="DefaultDllName32"/> and <see cref="DefaultDllName64"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface, AllowMultiple = false )]
    public class NativeDllAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the native default library for any methods
        /// of the interface that do not specify it in their <see cref="CK.Interop.DllImportAttribute"/>.
        /// </summary>
        public string DefaultDllNameGeneric { get; set; }

        /// <summary>
        /// Gets or sets the name of the native default library to be used when running in 32 bits hosts for any methods
        /// of the interface that do not specify it in their <see cref="CK.Interop.DllImportAttribute"/>.
        /// </summary>
        public string DefaultDllName32 { get; set; }

        /// <summary>
        /// Gets or sets the name of the native default library to be used when running in 64 bits hosts for any methods
        /// of the interface that do not specify it in their <see cref="CK.Interop.DllImportAttribute"/>.
        /// </summary>
        public string DefaultDllName64 { get; set; }

        /// <summary>
        /// Initializes a new <see cref="NativeDllAttribute"/>.
        /// </summary>
        public NativeDllAttribute()
        {
        }

        internal string GetBestDefaultName()
        {
            if( IntPtr.Size == 4 ) return String.IsNullOrEmpty( DefaultDllName32 ) ? DefaultDllNameGeneric : DefaultDllName32;
            return String.IsNullOrEmpty( DefaultDllName64 ) ? DefaultDllNameGeneric : DefaultDllName64;
        }

    }

}
