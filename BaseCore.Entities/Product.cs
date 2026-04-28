

namespace BaseCore.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public decimal DiscountPercent { get; set; } = 0;

        public int? ManufacturerId { get; set; }

        public Category Category { get; set; }

        public Manufacturer? Manufacturer { get; set; }
    }
}
