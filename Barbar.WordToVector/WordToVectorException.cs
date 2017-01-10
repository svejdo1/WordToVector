using System;

namespace Barbar.WordToVector
{
    public class WordToVectorException : Exception
    {
        public WordToVectorException(string message) : base(message)
        {
        }
        public WordToVectorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
