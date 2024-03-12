using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WatcherApi.Classes;

namespace WatcherUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly Context _context;

        public LoginController(Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Admins.FirstOrDefault(x => x.Username.ToLower() == username.ToLower());

            if (user != null && PasswordMatches(user.Password, password) && UsernameMatches(user.Username, username))
            {
                return RedirectToAction("Index", "WatchUI");
            }
            else
            {
                return View();
            }
        }

        private bool PasswordMatches(string storedPassword, string enteredPassword)
        {
            // Şifre karşılaştırmasını gerçekleştirir.
            // İhtiyaca göre şifre karmaşıklığı, büyük/küçük harf duyarlılığı kontrolü ekleyebilirsiniz.
            return storedPassword == enteredPassword;
        }

        private bool UsernameMatches(string storedUsername, string enteredUsername)
        {

            return storedUsername.Equals(enteredUsername, StringComparison.Ordinal);
        }

        //private bool IsPasswordComplexEnough(string password)
        //{  
        //  en az bir büyük harf, bir küçük harf, bir sayı ve bir özel karakter içermeli
        //    return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,}$");
        //}

        [HttpGet]
        public IActionResult AddUsers()
        {
            return View();
        }
        [HttpPost("Login/AddUsers")]
        public IActionResult AddVirtualMachine(Admin admin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Admins.Add(admin);
                    _context.SaveChanges();

                    return RedirectToAction("Index", "WatchUI");
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", "Sanal makine eklenirken bir hata oluştu.");
                    return View(admin);
                }
            }
            return View(admin);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Kullanıcının çıkış yapmasını sağla
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Çıkış yapıldıktan sonra tarayıcı önbelleğinden silme işlemi
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            //Sayfayı tarayıcı geçmişinde bir sayfa olarak işaretleme
            Response.Headers.Add("Referrer-Policy", "no-referrer");

            // Çıkış yapıldıktan sonra yönlendirilecek sayfayı belirle
            return RedirectToAction("Index", "Login");
        }
    }
}
