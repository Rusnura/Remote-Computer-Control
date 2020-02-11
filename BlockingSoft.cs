using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    [Serializable()]
    public class BlockingSoft
    {
        private string soft;
        private DateTime date;
        private State state = State.NEW;

        public BlockingSoft(string soft, DateTime date)
        {
            this.soft = soft;
            this.date = date;
        }

        public string getSoft()
        {
            return this.soft;
        }

        public void setSoft(string soft)
        {
            this.soft = soft;
        }

        public DateTime getDate()
        {
            return this.date;
        }

        public void setDate(DateTime date) 
        {
            this.date = date;
        }

        public State getState() 
        {
            return this.state;
        }

        public void setState(State state)
        {
            this.state = state;
        }
    }
}
