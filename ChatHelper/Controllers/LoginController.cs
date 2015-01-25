using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ChatHelper.Models;

namespace ChatHelper.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        public string HashText(string text, string salt, System.Security.Cryptography.HashAlgorithm hasher)
        {
            byte[] textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(text, salt));
            byte[] hashedBytes = hasher.ComputeHash(textWithSaltBytes);
            hasher.Clear();
            return Convert.ToBase64String(hashedBytes);
        }
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            using (ChatHelperEntities context= new ChatHelperEntities())
            {
                var pwd = HashText(password, "chatHelper", new SHA1CryptoServiceProvider());
                var user = context.tb_user_info.FirstOrDefault(d => d.email.Equals(email, StringComparison.OrdinalIgnoreCase) && d.password.Equals(pwd, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {                   
                    FormsAuthentication.SetAuthCookie(user.email,false);
                    return Content("Ok");
                }
                return Content("Fail");
            }
            
        }
    }
}