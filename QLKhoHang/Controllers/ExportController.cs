using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QLKhoHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllExports()
        {
            try
            {
                var exports = _context.Exports
                    .Select(e => new
                    {
                        e.id,
                        e.Name,
                        e.exportDate,
                        e.note,
                        e.status,
                        e.productCount,
                        SourceWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == e.idWarehouseExport),
                        DestinationWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == e.idWarehouseImport),
                        Details = _context.ExportDetails
                            .Where(d => d.idWareHouseExport == e.id)
                            .Select(d => new
                            {
                                d.id,
                                d.quantity,
                                Product = _context.Products.FirstOrDefault(p => p.Id == d.idProduct)
                            }).ToList()
                    })
                    .ToList();

                return Ok(exports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve exports", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetExportById(int id)
        {
            try
            {
                var export = _context.Exports
                    .Where(e => e.id == id)
                    .Select(e => new
                    {
                        e.id,
                        e.Name,
                        e.exportDate,
                        e.note,
                        e.status,
                        e.productCount,
                        SourceWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == e.idWarehouseExport),
                        DestinationWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == e.idWarehouseImport),
                        Details = _context.ExportDetails
                            .Where(d => d.idWareHouseExport == e.id)
                            .Select(d => new
                            {
                                d.id,
                                d.quantity,
                                Product = _context.Products.FirstOrDefault(p => p.Id == d.idProduct)
                            }).ToList()
                    })
                    .FirstOrDefault();

                if (export == null)
                {
                    return NotFound(new { error = $"Export with ID {id} not found" });
                }

                return Ok(export);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to retrieve export with ID {id}", message = ex.Message });
            }
        }

        [HttpPost("create-transfer")]
        [Consumes("application/json")]
        public IActionResult CreateTransfer([FromBody] TransferOrderJsonDto dto)
        {
            try
            {
                if (dto.details == null || !dto.details.Any())
                    return BadRequest(new { error = "No products specified for transfer" });

                var sourceWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == dto.idWarehouseExport);
                if (sourceWarehouse == null)
                    return BadRequest(new { error = $"Source warehouse with ID {dto.idWarehouseExport} not found" });

                var destWarehouse = _context.Warehouse.FirstOrDefault(w => w.Id == dto.idWarehouseImport);
                if (destWarehouse == null)
                    return BadRequest(new { error = $"Destination warehouse with ID {dto.idWarehouseImport} not found" });

                var exportDetails = new List<ExportDetail>();
                int totalQuantity = 0;
                var errors = new Dictionary<string, string>();

                for (int i = 0; i < dto.details.Count; i++)
                {
                    var item = dto.details[i];
                    if (string.IsNullOrEmpty(item.barcode))
                    {
                        errors.Add($"product_{i}", "Barcode cannot be empty");
                        continue;
                    }
                    if (item.quantity <= 0)
                    {
                        errors.Add($"quantity_{i}", $"Invalid quantity value for barcode {item.barcode}");
                        continue;
                    }
                    var sourceProduct = _context.Products.FirstOrDefault(p =>
                        p.Barcode == item.barcode &&
                        p.WarehouseId == dto.idWarehouseExport);

                    if (sourceProduct == null)
                    {
                        errors.Add($"barcode_{i}", $"Product with barcode '{item.barcode}' does not exist in source warehouse {dto.idWarehouseExport}");
                        continue;
                    }
                    if (!int.TryParse(sourceProduct.Num, out int currentQuantity))
                    {
                        errors.Add($"quantity_{i}", $"Invalid quantity format for product with barcode '{item.barcode}'");
                        continue;
                    }
                    if (currentQuantity < item.quantity)
                    {
                        errors.Add($"quantity_{i}", $"Not enough quantity for product with barcode '{item.barcode}'. Available: {currentQuantity}, Requested: {item.quantity}");
                        continue;
                    }
                    exportDetails.Add(new ExportDetail
                    {
                        idProduct = sourceProduct.Id,
                        quantity = item.quantity
                    });
                    totalQuantity += item.quantity;
                }

                if (errors.Count > 0)
                    return BadRequest(new { error = "Validation errors", details = errors });

                var export = new Export
                {
                    idWarehouseExport = dto.idWarehouseExport,
                    idWarehouseImport = dto.idWarehouseImport,
                    productCount = totalQuantity,
                    note = dto.note,
                    exportDate = DateTime.Now,
                    Name = dto.name,
                    status = "Pending"
                };
                _context.Exports.Add(export);
                _context.SaveChanges();

                foreach (var detail in exportDetails)
                {
                    var exportDetail = new ExportDetail
                    {
                        idWareHouseExport = export.id,
                        idProduct = detail.idProduct,
                        quantity = detail.quantity
                    };
                    _context.ExportDetails.Add(exportDetail);
                }
                _context.SaveChanges();

                return Ok(new { success = true, message = "Transfer created successfully", id = export.id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create transfer", message = ex.Message });
            }
        }

        [HttpPost("confirm/{id}")]
        public IActionResult ConfirmExport(int id)
        {
            try
            {
                var export = _context.Exports.FirstOrDefault(e => e.id == id);
                if (export == null)
                {
                    return NotFound(new { error = $"Export with ID {id} not found" });
                }

                if (export.status == "Completed")
                {
                    return BadRequest(new { error = "This export order has already been confirmed" });
                }

                var exportDetails = _context.ExportDetails
                    .Where(d => d.idWareHouseExport == export.id)
                    .ToList();

                Dictionary<string, string> errors = new Dictionary<string, string>();
                int processedCount = 0;

                foreach (var detail in exportDetails)
                {
                    var sourceProduct = _context.Products.FirstOrDefault(p =>
                        p.Id == detail.idProduct &&
                        p.WarehouseId == export.idWarehouseExport);

                    if (sourceProduct == null)
                    {
                        errors.Add($"product_{detail.idProduct}", $"Product ID {detail.idProduct} no longer exists in source warehouse {export.idWarehouseExport}");
                        continue;
                    }

                    int currentQuantity;
                    if (!int.TryParse(sourceProduct.Num, out currentQuantity) || currentQuantity < detail.quantity)
                    {
                        errors.Add($"quantity_{detail.idProduct}", $"Insufficient quantity for product ID {detail.idProduct}");
                        continue;
                    }

                    int remainingQuantity = currentQuantity - detail.quantity;
                    sourceProduct.Num = remainingQuantity.ToString();

                    if (remainingQuantity <= 0)
                    {
                        sourceProduct.Status = "Out of stock";
                    }

                    var destProduct = _context.Products.FirstOrDefault(p =>
                        p.Barcode == sourceProduct.Barcode &&
                        p.WarehouseId == export.idWarehouseImport);

                    if (destProduct != null)
                    {
                        int destQuantity;
                        if (!int.TryParse(destProduct.Num, out destQuantity))
                        {
                            destQuantity = 0;
                        }
                        destProduct.Num = (destQuantity + detail.quantity).ToString();
                        destProduct.Status = "In stock";
                    }
                    else
                    {
                        var newProduct = new Products
                        {
                            Barcode = sourceProduct.Barcode,
                            Name = sourceProduct.Name,
                            CategoryID = sourceProduct.CategoryID,
                            Brand = sourceProduct.Brand,
                            Price = sourceProduct.Price,
                            Cost = sourceProduct.Cost,
                            Description = sourceProduct.Description,
                            Status = "In stock",
                            WarehouseId = export.idWarehouseImport,
                            Image = sourceProduct.Image,
                            location = sourceProduct.location,
                            Num = detail.quantity.ToString(),
                            createdDate = DateTime.Now,
                            finaldDate = sourceProduct.finaldDate
                        };
                        _context.Products.Add(newProduct);
                    }

                    processedCount++;
                }

                if (errors.Count > 0)
                {
                    return BadRequest(new
                    {
                        error = "Failed to confirm some products",
                        details = errors,
                        processed = processedCount,
                        total = exportDetails.Count
                    });
                }

                export.status = "Completed";
                UpdateWarehouseProductCounts(export.idWarehouseExport);
                UpdateWarehouseProductCounts(export.idWarehouseImport);

                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Export order confirmed successfully",
                    id = export.id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to confirm export", message = ex.Message });
            }
        }

        private void UpdateWarehouseProductCounts(int warehouseId)
        {
            var warehouse = _context.Warehouse.FirstOrDefault(w => w.Id == warehouseId);
            if (warehouse != null)
            {
                var productCount = _context.Products
                    .Where(p => p.WarehouseId == warehouseId && p.Status == "In stock")
                    .Count();

                warehouse.productCount = productCount;
            }
        }
    }

    // DTOs for JSON input
    public class TransferOrderJsonDto
    {
        public int idWarehouseExport { get; set; }
        public int idWarehouseImport { get; set; }
        public string note { get; set; }
        public string name { get; set; }
        public List<TransferOrderProductDto> details { get; set; }
    }

    public class TransferOrderProductDto
    {
        public string barcode { get; set; }
        public int quantity { get; set; }
    }
}