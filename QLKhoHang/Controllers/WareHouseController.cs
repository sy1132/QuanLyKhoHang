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

        public WareHouseController(ApplicationDbContext context)
        {
            _context = context;
    
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
        {
            return await _context.Warehouse.ToListAsync();
        }

        // GET: Lấy danh sách kho hàng với tìm kiếm
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
                // Không cập nhật CreatedAt
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

        // PATCH: Cập nhật trạng thái kho hàng
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateWarehouseStatus(int id, [FromBody] bool isActive)
        {
            var warehouse = await _context.Warehouse.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound(new { Message = "Không tìm thấy kho hàng với ID này" });
            }

            warehouse.IsActive = isActive;
            warehouse.UpdatedAt = DateTime.Now;

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

        // Kiểm tra kho hàng có tồn tại không
        private bool WarehouseExists(int id)
        {
            return _context.Warehouse.Any(e => e.Id == id);
        }
    }
}