using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;

namespace QLKhoHang.Controllers
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("import")]
        public async Task<IActionResult> GetImportReport(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.Imports.AsQueryable();

                // Lọc theo khoảng thời gian
                if (fromDate.HasValue)
                {
                    query = query.Where(i => i.DateInput >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    // Thêm 1 ngày để bao gồm cả ngày kết thúc
                    var nextDay = toDate.Value.AddDays(1);
                    query = query.Where(i => i.DateInput < nextDay);
                }

                var imports = await query
                    .OrderByDescending(i => i.DateInput)
                    .ToListAsync();

                // Lấy thống kê
                var statistics = new
                {
                    TotalImports = imports.Count,
                    ApprovedImports = imports.Count(i => i.Status == "Approved"),
                    PendingImports = imports.Count(i => i.Status == "Pending"),
                    RejectedImports = imports.Count(i => i.Status == "Rejected")
                };

                // Lấy chi tiết
                var details = new List<object>();
                foreach (var import in imports)
                {
                    var importDetails = await _context.ImportDetails
                        .Where(d => d.IdInport == import.Id)
                        .ToListAsync();

                    foreach (var detail in importDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.IdProduct);
                        var supplier = await _context.Suppliers.FindAsync(detail.IdSupplier);

                        details.Add(new
                        {
                            import.Id,
                            import.DateInput,
                            import.Status,
                            ProductName = product?.Name ?? "Unknown",
                            SupplierName = supplier?.Name ?? "Unknown",
                            detail.Quantity,
                            detail.Cost,
                            detail.Note
                        });
                    }
                }

                var result = new
                {
                    Statistics = statistics,
                    Details = details
                };

                return Ok(new ApiResult { Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult { Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("import/export")]
        public async Task<IActionResult> ExportImportReport(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.Imports.AsQueryable();

                // Lọc theo khoảng thời gian
                if (fromDate.HasValue)
                {
                    query = query.Where(i => i.DateInput >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var nextDay = toDate.Value.AddDays(1);
                    query = query.Where(i => i.DateInput < nextDay);
                }

                var imports = await query
                    .OrderByDescending(i => i.DateInput)
                    .ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Import Report");

                    // Tiêu đề
                    worksheet.Cell(1, 1).Value = "BÁO CÁO NHẬP HÀNG";
                    worksheet.Cell(2, 1).Value = $"Từ ngày: {fromDate?.ToString("dd/MM/yyyy") ?? "N/A"}";
                    worksheet.Cell(2, 3).Value = $"Đến ngày: {toDate?.ToString("dd/MM/yyyy") ?? "N/A"}";

                    // Header
                    worksheet.Cell(4, 1).Value = "ID Phiếu";
                    worksheet.Cell(4, 2).Value = "Ngày nhập";
                    worksheet.Cell(4, 3).Value = "Sản phẩm";
                    worksheet.Cell(4, 4).Value = "Nhà cung cấp";
                    worksheet.Cell(4, 5).Value = "Số lượng";
                    worksheet.Cell(4, 6).Value = "Đơn giá";
                    worksheet.Cell(4, 7).Value = "Thành tiền";
                    worksheet.Cell(4, 8).Value = "Trạng thái";

                    // Format header
                    var headerRange = worksheet.Range(4, 1, 4, 8);
                    headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Font.Bold = true;

                    int row = 5;
                    decimal totalValue = 0;
                    int totalQuantity = 0;

                    foreach (var import in imports)
                    {
                        var importDetails = await _context.ImportDetails
                            .Where(d => d.IdInport == import.Id)
                            .ToListAsync();

                        foreach (var detail in importDetails)
                        {
                            var product = await _context.Products.FindAsync(detail.IdProduct);
                            var supplier = await _context.Suppliers.FindAsync(detail.IdSupplier);

                            worksheet.Cell(row, 1).Value = import.Id;
                            worksheet.Cell(row, 2).Value = import.DateInput.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 3).Value = product?.Name ?? "Unknown";
                            worksheet.Cell(row, 4).Value = supplier?.Name ?? "Unknown";
                            worksheet.Cell(row, 5).Value = detail.Quantity;
                            worksheet.Cell(row, 6).Value = detail.Cost;
                            worksheet.Cell(row, 7).Value = detail.Quantity * detail.Cost;
                            worksheet.Cell(row, 8).Value = GetStatusText(import.Status);

                            totalQuantity += detail.Quantity;
                            totalValue += detail.Quantity * detail.Cost;
                            row++;
                        }
                    }

                    // Tổng cộng
                    worksheet.Cell(row, 3).Value = "TỔNG CỘNG:";
                    worksheet.Cell(row, 3).Style.Font.Bold = true;
                    worksheet.Cell(row, 5).Value = totalQuantity;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
                    worksheet.Cell(row, 7).Value = totalValue;
                    worksheet.Cell(row, 7).Style.Font.Bold = true;

                    // Format tiền tệ
                    worksheet.Range(5, 6, row, 7).Style.NumberFormat.Format = "#,##0 VNĐ";

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        string fileName = $"ImportReport_{DateTime.Now:yyyyMMdd}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult { Message = $"Error: {ex.Message}" });
            }
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ duyệt",
                "Approved" => "Đã duyệt",
                "Rejected" => "Đã từ chối",
                _ => status
            };
        }
    }
}