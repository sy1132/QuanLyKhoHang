namespace QLKhoHang.Entities
{
    public class ImportDetail
    {
        public int Id { get; set; }
        public int IdInport { get; set; }
        public int IdProduct { get; set; }
        public int IdSupplier { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public string Note { get; set; }
    }
}
