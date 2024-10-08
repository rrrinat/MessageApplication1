namespace MessageHistoryClient.Models
{
    public class Message
    {
        public string Text { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
