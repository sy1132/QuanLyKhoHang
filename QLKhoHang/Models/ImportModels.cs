using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLKhoHang.Models
{
    /// <summary>
    /// Model dùng cho API tạo phiếu nhập mới
    /// </summary>
    public class ImportCreateModel
    {
        /// <summary>
        /// Mã kho hàng nhập vào
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn kho hàng")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Danh sách chi tiết các sản phẩm nhập
        /// </summary>
        [Required(ErrorMessage = "Phiếu nhập phải có ít nhất một sản phẩm")]
        public List<ImportDetailModel> Details { get; set; } = new List<ImportDetailModel>();
    }

    /// <summary>
    /// Model dùng cho API cập nhật phiếu nhập
    /// </summary>
    public class ImportUpdateModel
    {
        /// <summary>
        /// Mã phiếu nhập cần cập nhật
        /// </summary>
        [Required(ErrorMessage = "Mã phiếu nhập không được để trống")]
        public int Id { get; set; }

        /// <summary>
        /// Mã kho hàng nhập vào
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn kho hàng")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Danh sách chi tiết các sản phẩm nhập
        /// </summary>
        [Required(ErrorMessage = "Phiếu nhập phải có ít nhất một sản phẩm")]
        public List<ImportDetailModel> Details { get; set; } = new List<ImportDetailModel>();
    }

    /// <summary>
    /// Model dùng cho chi tiết sản phẩm trong phiếu nhập
    /// </summary>
    public class ImportDetailModel
    {
        /// <summary>
        /// Mã sản phẩm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int IdProduct { get; set; }

        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int IdSupplier { get; set; }

        /// <summary>
        /// Số lượng nhập
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        /// <summary>
        /// Giá nhập
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
    }

    /// <summary>
    /// Model dùng cho việc trả về thông tin phiếu nhập
    /// </summary>
    public class ImportViewModel
    {
        /// <summary>
        /// Mã phiếu nhập
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trạng thái phiếu nhập (Pending, Approved, Rejected)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Tổng chi phí
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Ngày nhập hàng
        /// </summary>
        public DateTime DateInput { get; set; }

        /// <summary>
        /// Mã kho hàng
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Tên kho hàng
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// Chi tiết phiếu nhập
        /// </summary>
        public List<ImportDetailViewModel> Details { get; set; } = new List<ImportDetailViewModel>();
    }

    /// <summary>
    /// Model dùng cho việc trả về thông tin chi tiết sản phẩm trong phiếu nhập
    /// </summary>
    public class ImportDetailViewModel
    {
        /// <summary>
        /// Mã chi tiết
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã sản phẩm
        /// </summary>
        public int IdProduct { get; set; }

        /// <summary>
        /// Tên sản phẩm
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int IdSupplier { get; set; }

        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Giá nhập
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
    }
}
