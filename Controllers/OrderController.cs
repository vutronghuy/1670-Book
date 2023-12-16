using _1670_Book.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _1670_Book.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;


        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = __context.Order.Include(o => o.Book);
            //return View(await applicationDbContext.ToListAsync());
            var user = await _context.Users.ToListAsync();
            ViewBag.Users = user;
            return View(await _context.Order.ToListAsync());
        }

        public IActionResult Delete(int id)
        {
            var order = _context.Order.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
            return (_context.Order?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
