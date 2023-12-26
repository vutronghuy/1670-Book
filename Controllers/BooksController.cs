using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _1670_Book.Data;
using _1670_Book.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace _1670_Book.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;
        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }
       
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Book.Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }
       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        [Authorize(Roles = "Admin")]       
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {               
                string fileName = UploadFile(book);
                book.BookUrl = fileName;

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", book.CategoryId);
            return View();
        }
        public string? UploadFile(Book bookModel)
        {

            if (bookModel.BookImage != null)
            {
                string uploadFolder = Path.Combine(env.WebRootPath, "image");
                string FileName = Guid.NewGuid().ToString() + "_" + bookModel.BookImage.FileName;
                string filePath = Path.Combine(uploadFolder, FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    bookModel.BookImage.CopyTo(fileStream);
                }
                return FileName;
            }
            return null;
        }
        [Authorize(Roles = "Admin")]       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", book.CategoryId);
            return View(book);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Book bookModel)
        {
            if (id != bookModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Book.FindAsync(id);

                    if (existingBook == null)
                    {
                        return NotFound();
                    }
                  
                    existingBook.NameBook = bookModel.NameBook;
                    existingBook.CategoryId = bookModel.CategoryId;
                    existingBook.Price = bookModel.Price;
                    existingBook.Quantity = bookModel.Quantity;
                    
                    if (bookModel.BookImage != null)
                    {
                        
                        if (!string.IsNullOrEmpty(existingBook.BookUrl))
                        {
                            DeleteImage(existingBook.BookUrl);
                        }
                       
                        existingBook.BookUrl = UploadFile(bookModel);
                    }

                    _context.Update(existingBook);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(bookModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", bookModel.CategoryId);        
            return View(bookModel);
        }
        private void DeleteImage(string fileName)
        {
            string filePath = Path.Combine(env.WebRootPath, "image", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = _context.Book.FirstOrDefault(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            _context.Book.Remove(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        private bool BookExists(int id)
        {
            return (_context.Book?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
