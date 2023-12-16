namespace _1670_Book.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderTime { get; set; }
        public double Total { get; set; }
        public virtual Book? Book { get; set; }
    }
}
