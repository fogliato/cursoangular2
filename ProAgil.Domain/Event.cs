using System;
using System.Collections.Generic;

namespace ProAgil.Domain
{
    public class Event
    {
        public int Id { get; set; }
        public string Location { get; private set; } = null!;
        public DateTime? EventDate { get; private set; }
        public string Theme { get; set; } = null!;
        public int PeopleCount { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public List<Batch> Batches { get; set; } = new List<Batch>();
        public List<SocialNetwork> SocialNetworks { get; set; } = new List<SocialNetwork>();
        public List<SpeakerEvent> SpeakerEvents { get; private set; } = new List<SpeakerEvent>();
    }
}

