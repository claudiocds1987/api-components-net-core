namespace ApiComponents.Models
{
    public class ProductReview
    {
        public int id { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
        public DateTime date { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public int productId { get; set; }
    }
}
