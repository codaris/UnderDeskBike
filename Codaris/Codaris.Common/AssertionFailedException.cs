using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codaris.Common
{
    /// <summary>
    /// Exception raised when an assertion fails
    /// </summary>
    [Serializable]
    public class AssertionFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssertionFailedException" /> class.
        /// </summary>
        public AssertionFailedException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertionFailedException" /> class with message.
        /// </summary>
        /// <param name="message">Exception message</param>
        public AssertionFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertionFailedException" /> class with message and inner exception.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="e">Inner exception</param>
        public AssertionFailedException(string message, Exception e) : base(message, e) { }
    }
}
