using System.Collections.Generic;

namespace ProAgil.Domain
{
    public class Speaker
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ShortBio { get; set; }
        public string? ImageUrl { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public List<SocialNetwork> SocialNetworks { get; set; } = new List<SocialNetwork>();
        public List<SpeakerEvent> SpeakerEvents { get; set; } = new List<SpeakerEvent>();
    }
}

