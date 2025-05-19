using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Data;
using QLKhoHang.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;
using QLKhoHang.Service;
using QLKhoHang.Models;
namespace QLKhoHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudiary;

        public ProductsController(ApplicationDbContext context , CloudinaryService cloudinary)
        {
            _context = context;
            _cloudiary = cloudinary;
        }

        // GET: api/Products/list
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/detail/5
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Products>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return product;
        }

        [HttpPost("create")]
        public async Task<ActionResult<Products>> CreateProduct([FromForm] ProductsModel productModel)
        {
            var product = new Products
            {
                Barcode = productModel.Barcode,
                Name = productModel.Name,
                CategoryID = productModel.CategoryID,
                Brand = productModel.Brand,
                Price = productModel.Price,
                // Fix for CS0266: Explicitly cast 'float' to 'decimal'.
                Cost = (decimal)productModel.Cost,
                Description = productModel.Description,
                Status = productModel.Status,
                WarehouseId = productModel.WarehouseId,
                location = productModel.location,
                createdDate = DateTime.Now, // Gán thời gian hiện tại
                finaldDate = DateTime.Now.AddDays(90),
                Num = "0" // Gán thời gian hết hạn (nếu cần)
            };  

            // Upload image if provided
            if (productModel.Image != null && productModel.Image.Length > 0)
            {
                string imageUrl = await _cloudiary.UploadImageAsync(productModel.Image);
                product.Image = imageUrl;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }


        // PUT: api/Products/update/5
        // PUT: api/Products/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductsModel productModel)
        {
            if (id != productModel.Id)
                return BadRequest("Id mismatch");

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound("Product not found");

            // Cập nhật các thuộc tính khác
            existingProduct.Barcode = productModel.Barcode;
            existingProduct.Name = productModel.Name;
            existingProduct.CategoryID = productModel.CategoryID;
            existingProduct.Brand = productModel.Brand;
            existingProduct.Price = productModel.Price;
            existingProduct.Cost = (decimal)productModel.Cost;
            existingProduct.Description = productModel.Description;
            existingProduct.Status = productModel.Status;
            existingProduct.WarehouseId = productModel.WarehouseId;
            existingProduct.location = productModel.location;
            existingProduct.Num = productModel.Num;

            // ✅ Nếu có ảnh mới, thì cập nhật
            if (productModel.Image is IFormFile file && file.Length > 0)
            {
                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingProduct.Image))
                {
                    await _cloudiary.DeleteImageAsync(existingProduct.Image);
                }

                // Upload ảnh mới
                string imageUrl = await _cloudiary.UploadImageAsync(file);
                existingProduct.Image = imageUrl;
            }
            // ❌ Nếu không có ảnh mới: giữ nguyên existingProduct.Image

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                    return NotFound("Product not found");
                else
                    throw;
            }

            return Ok(new { Message = "Product updated successfully." });
        }



        // DELETE: api/Products/delete/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportProductsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var products = new List<Products>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                    var rowCount = worksheet.LastRowUsed().RowNumber();

                    for (int row = 2; row <= rowCount; row++) // Bỏ qua dòng tiêu đề
                    {
                        var product = new Products
                        {
                            Barcode = worksheet.Cell(row, 1).GetString(),
                            Name = worksheet.Cell(row, 2).GetString(),
                            CategoryID = worksheet.Cell(row, 3).GetString(),
                            Brand = worksheet.Cell(row, 4).GetString(),
                            Price = worksheet.Cell(row, 5).GetString(),
                            Cost = float.TryParse(worksheet.Cell(row, 6).GetString(), out float cost) ? (decimal)cost : 0,
                            Description = worksheet.Cell(row, 7).GetString(),
                            Status = worksheet.Cell(row, 8).GetString(),
                            WarehouseId = int.TryParse(worksheet.Cell(row, 9).GetString(), out int wid) ? wid : 0,
                            location = worksheet.Cell(row, 11).GetString(),
                            Image = worksheet.Cell(row, 10).GetString(),
                            createdDate = DateTime.Now // Gán thời gian import
                        };
                        products.Add(product);
                    }
                }
            }

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Imported {products.Count} products successfully." });
        }
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportProductsToExcel()
        {
            var products = await _context.Products.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");

                // Header
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Barcode";
                worksheet.Cell(1, 3).Value = "Name";
                worksheet.Cell(1, 4).Value = "CategoryID";
                worksheet.Cell(1, 5).Value = "Brand";
                worksheet.Cell(1, 6).Value = "Price";
                worksheet.Cell(1, 7).Value = "Cost";
                worksheet.Cell(1, 8).Value = "Description";
                worksheet.Cell(1, 9).Value = "Status";
                worksheet.Cell(1, 10).Value = "WarehouseId";
                worksheet.Cell(1, 11).Value = "Image";
                worksheet.Cell(1, 12).Value = "Location";
                worksheet.Cell(1, 13).Value = "Created Date";
                worksheet.Cell(1, 14).Value = "Final Date";

                // Data
                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    int row = i + 2;
                    worksheet.Cell(row, 1).Value = p.Id;
                    worksheet.Cell(row, 2).Value = p.Barcode;
                    worksheet.Cell(row, 3).Value = p.Name;
                    worksheet.Cell(row, 4).Value = p.CategoryID;
                    worksheet.Cell(row, 5).Value = p.Brand;
                    worksheet.Cell(row, 6).Value = p.Price;
                    worksheet.Cell(row, 7).Value = p.Cost;
                    worksheet.Cell(row, 8).Value = p.Description;
                    worksheet.Cell(row, 9).Value = p.Status;
                    worksheet.Cell(row, 10).Value = p.WarehouseId;
                    worksheet.Cell(row, 11).Value = p.Image;
                    worksheet.Cell(row, 12).Value = p.location;
                    worksheet.Cell(row, 13).Value = p.createdDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 14).Value = p.finaldDate.ToString("yyyy-MM-dd HH:mm");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var fileName = $"products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                fileName);
                }
            }
        }
        // POST: api/Products/upload-image/5
        [HttpPost("upload-image/{id}")]
        public async Task<IActionResult> UploadProductImage(int id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            // Delete existing image if there is one
            if (!string.IsNullOrEmpty(product.Image))
            {
                await _cloudiary.DeleteImageAsync(product.Image);
            }

            // Upload the new image
            string imageUrl = await _cloudiary.UploadImageAsync(image);
            product.Image = imageUrl;

            await _context.SaveChangesAsync();

            return Ok(new { ImageUrl = imageUrl });
        }

        // DELETE: api/Products/delete-image/5
        [HttpDelete("delete-image/{id}")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            // Check if product has an image
            if (string.IsNullOrEmpty(product.Image))
                return BadRequest("Product does not have an image.");

            // Delete the image from Cloudinary
            await _cloudiary.DeleteImageAsync(product.Image);

            // Update the product
            product.Image = null;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Image deleted successfully." });
        }
    }
}