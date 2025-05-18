using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhoHang.Controllers
{
    [Route("api/warehouse")]
    [ApiController]
    public class WareHouseController : ControllerBase
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
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
        {
            return await _context.Warehouse.ToListAsync();
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetWarehouses(string searchString)
        {
            var warehouses = from w in _context.Warehouse
                             select w;

            if (!string.IsNullOrEmpty(searchString))
            {
                warehouses = warehouses.Where(w =>
                    w.Name.Contains(searchString) ||
                    w.Address.Contains(searchString) ||
                    w.Manager.Contains(searchString));
            }

            return Ok(await warehouses.ToListAsync());
        }

        // GET: Lấy thông tin kho hàng theo Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
            }

            return warehouse;
        }

        // POST: Thêm kho hàng mới
        [HttpPost("create")]
        public async Task<ActionResult<Warehouse>> CreateWarehouse(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            warehouse.CreatedAt = DateTime.Now;
            warehouse.UpdatedAt = DateTime.Now;
            warehouse.status = STATUS_ACTIVE; // Set default status

            try
            {
                _context.Warehouse.Add(warehouse);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi tạo kho hàng: {ex.Message}" });
            }
        }

        // PUT: Cập nhật thông tin kho hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWarehouse(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                return BadRequest(new { Message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            warehouse.UpdatedAt = DateTime.Now;

            try
            {
                _context.Entry(warehouse).State = EntityState.Modified;
                _context.Entry(warehouse).Property(x => x.CreatedAt).IsModified = false;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseExists(id))
                {
                    return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: Xóa kho hàng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
            }

            try
            {
                _context.Warehouse.Remove(warehouse);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Xóa kho hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi xóa kho hàng: {ex.Message}" });
            }
        }

        // PATCH: Cập nhật trạng thái hoạt động của kho hàng
        [HttpPatch("{id}/active-status")]
        public async Task<IActionResult> UpdateWarehouseActiveStatus(int id, [FromBody] bool isActive)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
            }

            warehouse.IsActive = isActive;
            warehouse.UpdatedAt = DateTime.Now;
            // Update status field to match IsActive
            warehouse.status = isActive ? STATUS_ACTIVE : STATUS_INACTIVE;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Cập nhật trạng thái kho hàng thành công: {(isActive ? "Hoạt động" : "Không hoạt động")}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi cập nhật trạng thái kho hàng: {ex.Message}" });
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
                return BadRequest(new { Message = "Trạng thái không hợp lệ. Trạng thái phải là 'Active', 'Full', hoặc 'Inactive'" });
            }

            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
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

                return Ok(new { Message = $"Cập nhật trạng thái kho hàng thành công: {statusMessage}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi khi cập nhật trạng thái kho hàng: {ex.Message}" });
            }
        }

        // Kiểm tra kho hàng có tồn tại không
        private bool WarehouseExists(int id)
        {
            return _context.Warehouse.Any(e => e.Id == id);
        }
    }
}