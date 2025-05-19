namespace QLKhoHang.Entities
{
    public class Export
    {
        public int id { get; set; }
        public int idWarehouseExport { get; set; }
        public int idWarehouseImport { get; set; }
        public int productCount { get; set; }
        public string note { get; set; }
        public DateTime exportDate { get; set; }
        public string Name { get; set; }
        public string status { get; set; }

    }
}
