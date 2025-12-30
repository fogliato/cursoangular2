using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class SpeakerDto
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        
        public string? ShortBio { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        public List<SocialNetworkDto> SocialNetworks { get; set; } = new List<SocialNetworkDto>();
        
        public List<EventDto> Events { get; set; } = new List<EventDto>();
    }
}

