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

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Book.Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Books/Details/5
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
        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                //Upload file
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
        // GET: Books/Edit/5
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

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                    // Update non-file properties
                    existingBook.NameBook = bookModel.NameBook;
                    existingBook.CategoryId = bookModel.CategoryId;
                    existingBook.Price = bookModel.Price;
                    existingBook.Quantity = bookModel.Quantity;

                    // Check if a new image is provided
                    if (bookModel.BookImage != null)
                    {
                        // Delete the existing image file
                        if (!string.IsNullOrEmpty(existingBook.BookUrl))
                        {
                            DeleteImage(existingBook.BookUrl);
                        }

                        // Upload the new image
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
