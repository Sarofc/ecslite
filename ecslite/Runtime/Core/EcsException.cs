using System;

namespace Saro.Entities
{
    public class EcsException : Exception
    {
        public EcsException(string message) : base(message)
        {
        }
    }
}
