using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    public class UserUnauthrizedException : Exception
    {
        public UserUnauthrizedException()
        {
        }

        public UserUnauthrizedException(string message) : base(message)
        {
        }

        public UserUnauthrizedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
