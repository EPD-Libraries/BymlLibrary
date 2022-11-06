using System;

namespace Nintendo.Byml
{
    /// <summary>
    /// Represents errors that occur when trying to process invalid BYAML data.
    /// </summary>
    internal class BymlException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BymlException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal BymlException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BymlException"/> class with a specified error message and a
        /// reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner"> The exception that is the cause of the current exception, or a null reference if no
        /// inner exception is specified.</param>
        internal BymlException(string message, Exception inner) : base(message, inner) { }
    }
}
