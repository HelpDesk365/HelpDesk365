using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ChatHelper.Models;

namespace ChatHelper.Controllers
{
    public class SingUpViewModel {
        public int MyProperty { get; set; }
    }
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
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
        public ActionResult SignUp(tb_user_info input)
        {
            using (ChatHelperEntities context = new ChatHelperEntities())
            {
                var user = context.tb_user_info.FirstOrDefault(d => d.email == input.email);
                if (user  != null)
                {
                    return Content("Exist");
                }
                input.password = HashText(input.password, "chatHelper", new SHA1CryptoServiceProvider());
                context.tb_user_info.Add(input);
                bool result = context.SaveChanges() > 0;
                if (result)
                {
                    return Content("Ok");
                }
                return Content("Fail");
            }
           
        }


    }
}