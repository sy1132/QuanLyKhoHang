namespace QLKhoHang.Models
{
    public class ProductsModel
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string CategoryID { get; set; }
        public string Brand { get; set; }
        public string Price { get; set; }
        public float Cost { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int WarehouseId { get; set; }
        public IFormFile? Image { get; set; }
        public string location { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime finaldDate { get; set; }
<<<<<<< HEAD
        public String  Num { get; set; }

=======
>>>>>>> parent of 365e0b3 (FN)
    }
}

