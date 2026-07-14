using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            ViewBag.Total = cart.Sum(item => item.Price);
            return View(cart);
        }

        [HttpPost]
        [Route("Cart/AddToCart")] // 👈 السطر ده بيجبر الـ .NET يربط المسار ده بالـ POST فوراً
        public IActionResult AddToCart(int id, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var product = _context.Products.Find(id);
            if (product != null)
            {
                var cart = GetCartFromSession();

                for (int i = 0; i < quantity; i++)
                {
                    cart.Add(product);
                }

                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        private List<Product> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString("ShoppingCart");
            return sessionData == null ? new List<Product>() : JsonSerializer.Deserialize<List<Product>>(sessionData) ?? new List<Product>();
        }

        [Authorize]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCartFromSession();

            var itemToRemove = cart.FirstOrDefault(x => x.Id == id);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            SaveCartToSession(cart);

            return RedirectToAction("Index");
        }
        /// <summary>
         [HttpGet]
        [Route("Cart/TestAdd/{id}")]
        public IActionResult TestAdd(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                var cart = GetCartFromSession();
                cart.Add(product);
                SaveCartToSession(cart);
            }
            return Content($"تم إضافة المنتج رقم {id} بنجاح في السلة! افتح السلة وتأكد.");
        }
        /// </summary>
        
        private void SaveCartToSession(List<Product> cart)
        {
            HttpContext.Session.SetString("ShoppingCart", JsonSerializer.Serialize(cart));
        }
    }
}