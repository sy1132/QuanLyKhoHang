using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Entities;
using QLKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhoHang.Controllers
{
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/import - Lấy tất cả phiếu nhập
        [HttpGet]
        public async Task<IActionResult> GetAllImports()
        {
            var imports = await _context.Imports
                .OrderByDescending(i => i.DateInput)
                .ToListAsync();

            return Ok(new ApiResult { Data = imports });
        }

        // GET: api/import/list - Tìm kiếm phiếu nhập theo điều kiện
        [HttpGet("list")]
        public async Task<IActionResult> GetImportList(string status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Imports.AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            // Lọc theo khoảng thời gian
            if (fromDate.HasValue)
            {
                query = query.Where(i => i.DateInput >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.DateInput <= toDate.Value);
            }

            var imports = await query
                .OrderByDescending(i => i.DateInput)
                .ToListAsync();

            return Ok(new ApiResult { Data = imports });
        }

        // GET: api/import/{id} - Lấy thông tin phiếu nhập theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetImport(int id)
        {
            var import = await _context.Imports.FindAsync(id);

            if (import == null)
            {
                return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
            }

            return Ok(new ApiResult { Data = import });
        }

        // GET: api/import/{id}/details - Lấy chi tiết phiếu nhập
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetImportDetails(int id)
        {
            var import = await _context.Imports.FindAsync(id);

            if (import == null)
            {
                return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
            }

            var details = await _context.ImportDetails
                .Where(d => d.IdInport == id)
                .ToListAsync();

            var result = new
            {
                import,
                details
            };

            return Ok(new ApiResult { Data = result });
        }

        // POST: api/import/create - Tạo phiếu nhập mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateImport([FromBody] ImportCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResult { Message = "Dữ liệu không hợp lệ", Data = ModelState });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo phiếu nhập
                var import = new Import
                {
                    Status = "Pending",
                    Cost = model.Details.Sum(d => d.Cost * d.Quantity),
                    DateInput = DateTime.Now,
                    WarehouseId = model.WarehouseId
                };

                _context.Imports.Add(import);
                await _context.SaveChangesAsync();

                // Tạo chi tiết phiếu nhập
                foreach (var detail in model.Details)
                {
                    var importDetail = new ImportDetail
                    {
                        IdInport = import.Id,
                        IdProduct = detail.IdProduct,
                        IdSupplier = detail.IdSupplier,
                        Quantity = detail.Quantity,
                        Cost = detail.Cost,
                        Note = detail.Note
                    };

                    _context.ImportDetails.Add(importDetail);

                    // Cập nhật số lượng sản phẩm trong kho
                    var product = await _context.Products.FindAsync(detail.IdProduct);
                    if (product != null)
                    {
                        int currentQuantity = int.TryParse(product.Num, out int num) ? num : 0;
                        product.Num = (currentQuantity + detail.Quantity).ToString();
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResult
                {
                    Data = new { ImportId = import.Id },
                    Message = "Tạo phiếu nhập thành công"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResult { Message = $"Lỗi khi tạo phiếu nhập: {ex.Message}" });
            }
        }

        // PUT: api/import/{id} - Cập nhật phiếu nhập
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImport(int id, [FromBody] ImportUpdateModel model)
        {
            if (id != model.Id)
            {
                return BadRequest(new ApiResult { Message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResult { Message = "Dữ liệu không hợp lệ", Data = ModelState });
            }

            var import = await _context.Imports.FindAsync(id);
            if (import == null)
            {
                return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
            }

            if (import.Status != "Pending")
            {
                return BadRequest(new ApiResult { Message = "Chỉ có thể chỉnh sửa phiếu nhập ở trạng thái chờ duyệt" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy chi tiết hiện tại để hoàn tác số lượng sản phẩm
                var existingDetails = await _context.ImportDetails
                    .Where(d => d.IdInport == id)
                    .ToListAsync();

                // Hoàn tác số lượng sản phẩm
                foreach (var detail in existingDetails)
                {
                    var product = await _context.Products.FindAsync(detail.IdProduct);
                    if (product != null)
                    {
                        int currentQuantity = int.TryParse(product.Num, out int num) ? num : 0;
                        product.Num = Math.Max(0, currentQuantity - detail.Quantity).ToString();
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                // Cập nhật thông tin phiếu nhập
                import.WarehouseId = model.WarehouseId;
                import.Cost = model.Details.Sum(d => d.Cost * d.Quantity);
                _context.Entry(import).State = EntityState.Modified;

                // Xóa chi tiết cũ
                _context.ImportDetails.RemoveRange(existingDetails);

                // Tạo chi tiết mới
                foreach (var detail in model.Details)
                {
                    var importDetail = new ImportDetail
                    {
                        IdInport = import.Id,
                        IdProduct = detail.IdProduct,
                        IdSupplier = detail.IdSupplier,
                        Quantity = detail.Quantity,
                        Cost = detail.Cost,
                        Note = detail.Note
                    };

                    _context.ImportDetails.Add(importDetail);

                    // Cập nhật số lượng sản phẩm mới
                    var product = await _context.Products.FindAsync(detail.IdProduct);
                    if (product != null)
                    {
                        int currentQuantity = int.TryParse(product.Num, out int num) ? num : 0;
                        product.Num = (currentQuantity + detail.Quantity).ToString();
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResult { Message = "Cập nhật phiếu nhập thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResult { Message = $"Lỗi khi cập nhật phiếu nhập: {ex.Message}" });
            }
        }

        // PATCH: api/import/{id}/approve - Duyệt phiếu nhập
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApproveImport(int id)
        {
            var import = await _context.Imports.FindAsync(id);
            if (import == null)
            {
                return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
            }

            if (import.Status != "Pending")
            {
                return BadRequest(new ApiResult { Message = "Chỉ có thể duyệt phiếu nhập ở trạng thái chờ duyệt" });
            }

            try
            {
                import.Status = "Approved";
                _context.Entry(import).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ApiResult { Message = "Phiếu nhập đã được duyệt thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResult { Message = $"Lỗi khi duyệt phiếu nhập: {ex.Message}" });
            }
        }

        // PATCH: api/import/{id}/reject - Từ chối phiếu nhập
        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> RejectImport(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var import = await _context.Imports.FindAsync(id);
                if (import == null)
                {
                    return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
                }

                if (import.Status != "Pending")
                {
                    return BadRequest(new ApiResult { Message = "Chỉ có thể từ chối phiếu nhập ở trạng thái chờ duyệt" });
                }

                // Cập nhật trạng thái phiếu nhập
                import.Status = "Rejected";
                _context.Entry(import).State = EntityState.Modified;

                // Hoàn tác số lượng sản phẩm
                var details = await _context.ImportDetails
                    .Where(d => d.IdInport == id)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    var product = await _context.Products.FindAsync(detail.IdProduct);
                    if (product != null)
                    {
                        int currentQuantity = int.TryParse(product.Num, out int num) ? num : 0;
                        product.Num = Math.Max(0, currentQuantity - detail.Quantity).ToString();
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResult { Message = "Phiếu nhập đã bị từ chối thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResult { Message = $"Lỗi khi từ chối phiếu nhập: {ex.Message}" });
            }
        }

        // DELETE: api/import/{id} - Xóa phiếu nhập
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImport(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var import = await _context.Imports.FindAsync(id);
                if (import == null)
                {
                    return NotFound(new ApiResult { Message = "Không tìm thấy phiếu nhập với ID này" });
                }

                // Nếu phiếu nhập đã được duyệt, cần hoàn tác số lượng sản phẩm
                if (import.Status == "Approved")
                {
                    var details = await _context.ImportDetails
                        .Where(d => d.IdInport == id)
                        .ToListAsync();

                    foreach (var detail in details)
                    {
                        var product = await _context.Products.FindAsync(detail.IdProduct);
                        if (product != null)
                        {
                            int currentQuantity = int.TryParse(product.Num, out int num) ? num : 0;
                            product.Num = Math.Max(0, currentQuantity - detail.Quantity).ToString();
                            _context.Entry(product).State = EntityState.Modified;
                        }
                    }
                }

                // Xóa chi tiết phiếu nhập
                var importDetails = await _context.ImportDetails
                    .Where(d => d.IdInport == id)
                    .ToListAsync();

                _context.ImportDetails.RemoveRange(importDetails);

                // Xóa phiếu nhập
                _context.Imports.Remove(import);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResult { Message = "Xóa phiếu nhập thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResult { Message = $"Lỗi khi xóa phiếu nhập: {ex.Message}" });
            }
        }
    }
}