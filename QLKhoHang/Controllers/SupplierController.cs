using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QLKhoHang.Data;

namespace QLKhoHang.Controllers
{
    public class SupplierController : Controller
    {

        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // Hiển thị danh sách nhà cung cấp với tìm kiếm
        public async Task<IActionResult> Index(string searchString)
        {
            var suppliers = from s in _context.Suppliers
                            select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                suppliers = suppliers.Where(s => s.Name.Contains(searchString));
            }

            return View(await suppliers.ToListAsync());
        }

        // GET: Hiển thị form thêm nhà cung cấp
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thêm nhà cung cấp từ form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            supplier.CreatedAt = DateTime.Now;
            supplier.UpdatedAt = DateTime.Now;
            _context.Add(supplier);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Nhà cung cấp được tạo thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Import file Excel từ form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel.";
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                    {
                        TempData["Error"] = "File Excel không có dữ liệu.";
                        return RedirectToAction(nameof(Index));
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var supplier = new Supplier
                        {
                            Name = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty,
                            Address = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                            Phone = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty,
                            Email = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty,
                            WarehouseId = int.TryParse(worksheet.Cells[row, 5].Value?.ToString() ?? "0", out int warehouseId) ? warehouseId : 0,
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

            TempData["Success"] = "Nhập dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Xuất file Excel khi nhấn nút
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export()
        {
            var suppliers = await _context.Suppliers.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Suppliers");

                // Tiêu đề cột
                worksheet.Cells[1, 1].Value = "Tên";
                worksheet.Cells[1, 2].Value = "Địa chỉ";
                worksheet.Cells[1, 3].Value = "Số điện thoại";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Mã kho";

                // Dữ liệu
                for (int i = 0; i < suppliers.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = suppliers[i].Name ?? string.Empty;
                    worksheet.Cells[i + 2, 2].Value = suppliers[i].Address ?? string.Empty;
                    worksheet.Cells[i + 2, 3].Value = suppliers[i].Phone ?? string.Empty;
                    worksheet.Cells[i + 2, 4].Value = suppliers[i].Email ?? string.Empty;
                    worksheet.Cells[i + 2, 5].Value = suppliers[i].WarehouseId;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Suppliers-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
    }
}