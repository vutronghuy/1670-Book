using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _1670_Book.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string? NameBook { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? Author { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public virtual Category? Category { get; set; }
        public string? BookUrl { get; set; }

        [NotMapped]
        public IFormFile BookImage { get; set; }
    }
}
