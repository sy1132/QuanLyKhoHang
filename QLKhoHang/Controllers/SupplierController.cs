using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Entities;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QLKhoHang.Data;

namespace QLKhoHang.Controllers
{
    [Route("api/supplier")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;

        }

        // GET: Lấy danh sách nhà cung cấp với tìm kiếm
        // GET: Lấy toàn bộ danh sách nhà cung cấp (hiện ra luôn khi vào trang)
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return Ok(suppliers);
        }

        // GET: Tìm kiếm nhà cung cấp theo tên
        [HttpGet("search")]
        public async Task<IActionResult> SearchSuppliers(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return BadRequest(new { Message = "Vui lòng nhập từ khóa tìm kiếm." });

            var suppliers = await _context.Suppliers
                .Where(s => s.Name.Contains(searchString))
                .ToListAsync();

            return Ok(suppliers);
        }

        // POST: Thêm nhà cung cấp và lưu vào database
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Đặt thời gian tạo và cập nhật
            supplier.CreatedAt = DateTime.Now;
            supplier.UpdatedAt = DateTime.Now;

            try
            {
                // Thêm nhà cung cấp vào DbSet
                _context.Suppliers.Add(supplier);
                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Nhà cung cấp được tạo và lưu vào cơ sở dữ liệu thành công", SupplierId = supplier.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi lưu nhà cung cấp vào cơ sở dữ liệu: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        // POST: Import file Excel và lưu vào database
        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Vui lòng chọn file Excel." });
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Đảm bảo stream ở đầu

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rowCount = worksheet.LastRowUsed().RowNumber();

                    if (rowCount < 2)
                    {
                        return BadRequest(new { Message = "File Excel không có dữ liệu." });
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var supplier = new Supplier
                        {
                            Name = worksheet.Cell(row, 1).GetString() ?? string.Empty,
                            Address = worksheet.Cell(row, 2).GetString() ?? string.Empty,
                            Phone = worksheet.Cell(row, 3).GetString() ?? string.Empty,
                            Email = worksheet.Cell(row, 4).GetString() ?? string.Empty,
                            WarehouseId = int.TryParse(worksheet.Cell(row, 5).GetString(), out int warehouseId) ? warehouseId : 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        if (!string.IsNullOrEmpty(supplier.Name))
                        {
                            _context.Suppliers.Add(supplier);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { Message = "Dữ liệu từ file Excel đã được nhập và lưu vào cơ sở dữ liệu thành công." });
        }

        // GET: Xuất file Excel
        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var suppliers = await _context.Suppliers.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Suppliers");

                // Tiêu đề cột
                worksheet.Cell(1, 1).Value = "Tên";
                worksheet.Cell(1, 2).Value = "Địa chỉ";
                worksheet.Cell(1, 3).Value = "Số điện thoại";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Mã kho";

                // Dữ liệu
                for (int i = 0; i < suppliers.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = suppliers[i].Name ?? string.Empty;
                    worksheet.Cell(i + 2, 2).Value = suppliers[i].Address ?? string.Empty;
                    worksheet.Cell(i + 2, 3).Value = suppliers[i].Phone ?? string.Empty;
                    worksheet.Cell(i + 2, 4).Value = suppliers[i].Email ?? string.Empty;
                    worksheet.Cell(i + 2, 5).Value = suppliers[i].WarehouseId;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string excelName = $"Suppliers-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
        }
    }
}
