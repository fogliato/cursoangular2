using System;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class BatchDto
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        
        [Required]
        public decimal Price { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Range(2, 50000, ErrorMessage = "Quantity must be between 2 and 50,000")]
        public int Quantity { get; set; }       
    }
}

