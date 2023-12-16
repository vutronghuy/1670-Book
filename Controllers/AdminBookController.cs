using _1670_Book.Data;
using _1670_Book.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace _1670_Book.Controllers
{
    [Authorize]
    public class AdminBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;

        public AdminBookController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }

        public async Task<IActionResult> Index()
        {

            var ShopContext = _context.Book.Include(o => o.Category);

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Books");
            }
            if (User.IsInRole("User"))
            {
                return RedirectToAction("Index", "AdminBook");
            }
            return View(await ShopContext.ToListAsync());
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
        public Book GetBookDetail(int id)
        {
            var book = _context.Book.Find(id);
            return book;
        }

        List<Cart> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsoncart = session.GetString("cart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<Cart>>(jsoncart);
            }
            return new List<Cart>();
        }

        public IActionResult ListCart()
        {
            return View(GetCartItems());
        }

        public IActionResult AddToCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart == null) //If cart is empty
            {
                var book = GetBookDetail(id);
                List<Cart> ListCart = new()
        {
          new Cart
          {
            Book = book,
            Quantity = 1
          }
        };
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(ListCart));
            }
            else //if cart has at least 1 product
            {
                List<Cart> CartData = JsonConvert.DeserializeObject<List<Cart>>(cart);
                bool check = true;
                for (int i = 0; i < CartData.Count; i++)
                {
                    if (CartData[i].Book.Id == id)
                    {
                        CartData[i].Quantity++;
                        check = false;
                    }
                }
                if (check)
                {
                    CartData.Add(new Cart
                    {
                        Book = GetBookDetail(id),
                        Quantity = 1
                    });
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(CartData));
            }
            return RedirectToAction("Index");
        }

        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                if (quantity > 0)
                {
                    for (int i = 0; i < dataCart.Count; i++)
                    {
                        if (dataCart[i].Book.Id == id)
                        {
                            dataCart[i].Quantity = quantity;
                        }
                    }
                    HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                }
                return Ok(quantity);
            }
            return BadRequest();
        }

        public IActionResult DeleteCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);

                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].Book.Id == id)
                    {
                        dataCart.RemoveAt(i);
                    }
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));

                return RedirectToAction(nameof(ListCart));
            }
            return RedirectToAction(nameof(Index));
        }



        public string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public double GetTotal()
        {
            // Replace this with your actual logic to calculate the total from the shopping cart
            // For example, if you store the cart in session, you might iterate through the items and sum the totals
            double total = 0;

            // Example: Retrieve cart items from session
            var cart = HttpContext.Session.Get<List<Cart>>("cart");
            // HttpContext.Session.SetString(CARTKEY, JsonConvert.SerializeObject(CartData));

            if (cart != null)
            {
                foreach (var item in cart)
                {
                    total += item.Book.Price * item.Quantity;
                }
            }

            return total;
        }

        public IActionResult CreateOrder()
        {
            //ClaimsPrincipal currentUser = this.User;
            string currentUserID = GetUserId();
            double currentTotal = GetTotal();

            Order order = new Order();
            if (ModelState.IsValid)
            {
                order.UserId = currentUserID;
                order.OrderTime = DateTime.Now;
                order.Total = currentTotal;
                _context.Add(order);
                _context.SaveChanges();
                TempData["OrderId"] = order.Id;
                // Clear the cart after creating the order
                HttpContext.Session.Remove("cart");
            }
            return RedirectToAction("Index", "Order");
        }


    }
}
