using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    public class APIUpgradeRequired : Exception
    {
        public APIUpgradeRequired()
        {
        }

        public APIUpgradeRequired(string message) : base(message)
        {
        }

        public APIUpgradeRequired(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
