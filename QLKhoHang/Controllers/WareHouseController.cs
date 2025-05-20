using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Entities;
using QLKhoHang.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhoHang.Controllers
{
    [Route("api/warehouse")]
    [ApiController]
    public class WareHouseController : BaseController
    {
        private readonly ApplicationDbContext _context;

        // Status constants
        public const string STATUS_ACTIVE = "Active";
        public const string STATUS_FULL = "Full";
        public const string STATUS_INACTIVE = "Inactive";

        public WareHouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await _context.Warehouse.ToListAsync();
            return Response(new ApiResult(warehouses));
        }

        

        // GET: Lấy thông tin kho hàng theo Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return Response(new ApiResult { Message = "Không tìm thấy kho hàng với ID này" }, 404);
            }

            return Response(new ApiResult(warehouse));
        }

        // POST: Thêm kho hàng mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateWarehouse(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
            {
                return Response(new ApiResult { Message = "Dữ liệu không hợp lệ" }, 400);
            }

            warehouse.CreatedAt = DateTime.Now;
            warehouse.UpdatedAt = DateTime.Now;
            warehouse.status = STATUS_ACTIVE; // Set default status

            try
            {
                _context.Warehouse.Add(warehouse);
                await _context.SaveChangesAsync();

                return Response(new ApiResult(warehouse), 201);
            }
            catch (Exception ex)
            {
                return Response(new ApiResult { Message = $"Lỗi khi tạo kho hàng: {ex.Message}" }, 500);
            }
        }

        // PUT: Cập nhật thông tin kho hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWarehouse(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                return Response(new ApiResult { Message = "ID không khớp" }, 400);
            }

            if (!ModelState.IsValid)
            {
                return Response(new ApiResult { Message = "Dữ liệu không hợp lệ" }, 400);
            }

            warehouse.UpdatedAt = DateTime.Now;

            try
            {
                _context.Entry(warehouse).State = EntityState.Modified;
                _context.Entry(warehouse).Property(x => x.CreatedAt).IsModified = false;

                await _context.SaveChangesAsync();
                return Response(new ApiResult(warehouse));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseExists(id))
                {
                    return Response(new ApiResult { Message = "Không tìm thấy kho hàng với ID này" }, 404);
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: Xóa kho hàng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);
            if (warehouse == null)
            {
                return Response(new ApiResult { Message = "Không tìm thấy kho hàng với ID này" }, 404);
            }

            try
            {
                _context.Warehouse.Remove(warehouse);
                await _context.SaveChangesAsync();

                return Response(new ApiResult { Message = "Xóa kho hàng thành công" });
            }
            catch (Exception ex)
            {
                return Response(new ApiResult { Message = $"Lỗi khi xóa kho hàng: {ex.Message}" }, 500);
            }
        }

        // PATCH: Cập nhật trạng thái hoạt động của kho hàng
        [HttpPatch("{id}/active-status")]
        public async Task<IActionResult> UpdateWarehouseActiveStatus(int id, [FromBody] bool isActive)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return Response(new ApiResult { Message = "Không tìm thấy kho hàng với ID này" }, 404);
            }

            warehouse.IsActive = isActive;
            warehouse.UpdatedAt = DateTime.Now;
            // Update status field to match IsActive
            warehouse.status = isActive ? STATUS_ACTIVE : STATUS_INACTIVE;

            try
            {
                await _context.SaveChangesAsync();
                string statusText = isActive ? "Hoạt động" : "Không hoạt động";
                return Response(new ApiResult { Message = $"Cập nhật trạng thái kho hàng thành công: {statusText}" });
            }
            catch (Exception ex)
            {
                return Response(new ApiResult { Message = $"Lỗi khi cập nhật trạng thái kho hàng: {ex.Message}" }, 500);
            }
        }

        // PATCH: Cập nhật trạng thái kho hàng (Active, Full, Inactive)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateWarehouseStatus(int id, [FromBody] string status)
        {
            // Validate status
            status = status?.Trim();
            if (string.IsNullOrEmpty(status) ||
                (status != STATUS_ACTIVE && status != STATUS_FULL && status != STATUS_INACTIVE))
            {
                return Response(new ApiResult { Message = "Trạng thái không hợp lệ. Trạng thái phải là 'Active', 'Full', hoặc 'Inactive'" }, 400);
            }

            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return Response(new ApiResult { Message = "Không tìm thấy kho hàng với ID này" }, 404);
            }

            warehouse.status = status;
            warehouse.UpdatedAt = DateTime.Now;

            // Update IsActive to match status
            warehouse.IsActive = status != STATUS_INACTIVE;

            try
            {
                await _context.SaveChangesAsync();

                string statusMessage;
                switch (status)
                {
                    case STATUS_ACTIVE:
                        statusMessage = "Hoạt động";
                        break;
                    case STATUS_FULL:
                        statusMessage = "Đầy hàng";
                        break;
                    case STATUS_INACTIVE:
                        statusMessage = "Không hoạt động";
                        break;
                    default:
                        statusMessage = status;
                        break;
                }

                return Response(new ApiResult { Message = $"Cập nhật trạng thái kho hàng thành công: {statusMessage}" });
            }
            catch (Exception ex)
            {
                return Response(new ApiResult { Message = $"Lỗi khi cập nhật trạng thái kho hàng: {ex.Message}" }, 500);
            }
        }

        // Kiểm tra kho hàng có tồn tại không
        private bool WarehouseExists(int id)
        {
            return _context.Warehouse.Any(e => e.Id == id);
        }
    }
}