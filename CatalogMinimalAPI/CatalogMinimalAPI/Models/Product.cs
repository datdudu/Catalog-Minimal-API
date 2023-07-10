namespace CatalogMinimalAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public DateTime BuyDate { get; set; }
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
