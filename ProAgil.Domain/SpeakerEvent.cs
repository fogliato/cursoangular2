namespace ProAgil.Domain
{
    public class SpeakerEvent
    {
        public int SpeakerId { get; set; }
        public Speaker Speaker { get; set; } = null!;
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
    }
}

