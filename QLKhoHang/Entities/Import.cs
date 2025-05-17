namespace QLKhoHang.Entities
{
    public class Import
    {
        public int Id { get; set; }
        public string Status{ get; set; }
        public decimal Cost { get; set; }
        public DateTime DateInput { get; set; }
        public int WarehouseId { get; set; }

    }
}
