using BookStore.API.Models;
using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class OrderStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
