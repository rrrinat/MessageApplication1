using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageReceiverClient
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public int SequenceNumber { get; set; }
    }
}
