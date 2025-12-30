namespace ProAgil.Domain
{
    public class SocialNetwork
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string URL { get; set; } = null!;
        public int? EventId { get; set; }
        public Event? Event { get; }
        public int? SpeakerId { get; set; }
        public Speaker? Speaker { get; }
    }
}

