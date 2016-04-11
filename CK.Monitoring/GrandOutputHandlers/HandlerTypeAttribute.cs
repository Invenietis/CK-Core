using System;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Simple attribute that binds a <see cref="HandlerConfiguration"/> to the actual <see cref="HandlerBase"/> that will actually do the job.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class HandlerTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="HandlerTypeAttribute"/> with a type that must be a <see cref="HandlerBase"/> specialization.
        /// </summary>
        /// <param name="handlerType"></param>
        public HandlerTypeAttribute( Type handlerType )
        {
            HandlerType = handlerType;
        }

        /// <summary>
        /// Gets the type of the associated <see cref="HandlerBase"/>.
        /// </summary>
        public readonly Type HandlerType;
    }

}
