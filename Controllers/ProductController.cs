using System;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.Models;

namespace StoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    // สร้าง Object ของ ApplicationDBContext
    private readonly ApplicationDbContext _context;

    // IWebHostEnvironment คืออะไร
    private readonly IWebHostEnvironment _env;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ทดสอบเขียนฟังก์ชั่นการเชื่อมต่อ database
    [HttpGet("product")]
    public void TestConnection()
    {
        // ถ้าเชื่อมต่อได้จะแสดงข้อความ "Connected"
        if (_context.Database.CanConnect())
        {
            Response.WriteAsync("Connected");
        }
        // ถ้าเชื่อมต่อไม่ได้จะแสดงข้อมูล "Not connected"
        else
        {
            Response.WriteAsync("Not connected");
        }
    }

    // ฟังก์ชั่นสำหรับการดึงข้อมูลสินค้าทั้งหมด
    [HttpGet]
    public ActionResult<product> GetProducts()
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ทั้งหมด
        // var products = _context.products.ToList();

        // แบบเชื่อมกับตารางอื่น
        var products = _context.products
        .Join(
            _context.categories,
            p => p.category_id,
            c => c.category_id,
            (p, c) => new
            {
                p.product_id,
                p.product_name,
                p.unit_price,
                p.unit_in_stock,
                c.category_name
            }
        ).ToList();
        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(products);
    }

    [HttpGet("{id}")]
    public ActionResult<product> GetProductList(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ทั้งหมด
        var product = _context.products.FirstOrDefault(p => p.category_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<product>> CreateProduct([FromForm] product product, IFormFile image)
    {
        // เพิ่มข้อมูลลงในตาราง Products
        _context.products.Add(product);
        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if (image != null)
        {
            // กำหนดชื่อไฟล์รูป
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพ
            string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
            // ตรวจสอบว่าโฟลเดอร์ uploads มีหรือไม่
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            //บักทึกชื่อไฟล์ลงในฐานข้อมูล
            product.product_picture = fileName;
        }

        _context.SaveChanges();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id, product product)
    {
        var existingProduct = _context.products.FirstOrDefault(p => p.product_id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.product_name = product.product_name;
        existingProduct.unit_price = product.unit_price;
        existingProduct.unit_in_stock = product.unit_in_stock;
        existingProduct.category_id = product.category_id;

        // บันทึกข้อมูล
        _context.SaveChanges();

        return Ok(existingProduct);
    }

    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id)
    {
        var product = _context.products.FirstOrDefault(p => p.product_id == id);

        if (product == null)
        {
            return NotFound();
        }
        // ลบข้อมูล
        _context.products.Remove(product);
        _context.SaveChanges();

        return Ok(product);
    }
}
