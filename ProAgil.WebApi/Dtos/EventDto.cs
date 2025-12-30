using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class EventDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Location must have between 2 and 100 characters")]
        public string Location { get; set; } = null!;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Theme { get; set; } = null!;

        [Required]
        [Range(2, 50000, ErrorMessage = "People count must be between 2 and 50,000")]
        public int PeopleCount { get; set; }

        public string? ImageUrl { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid e-mail address")]
        public string? Email { get; set; }

        public List<BatchDto> Batches { get; set; } = new List<BatchDto>();

        public List<SocialNetworkDto> SocialNetworks { get; set; } = new List<SocialNetworkDto>();

        public List<SpeakerDto> Speakers { get; set; } = new List<SpeakerDto>();
    }
}

