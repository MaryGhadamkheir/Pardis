using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Pardis.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Users login, string redirectUrl = "/")
        {
            if (ModelState.IsValid)
            {
                PardisDBEntities db = new PardisDBEntities();
                //string hashPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(login.Password, "MD5");

                var user = db.Users.SingleOrDefault(w => w.UserName == login.UserName && w.Password == login.Password);
                if (user != null)
                {
                    LogUserLogin objLogOutLog = new LogUserLogin()
                    {

                        LogDate = System.DateTime.Now,
                        Status = 1,
                        UserID = user.UserID,
                    };
                    Log(objLogOutLog);
                    FormsAuthentication.SetAuthCookie(login.UserName, false);
                    return Redirect(redirectUrl);
                }
                else
                {
                    ModelState.AddModelError("UserName", "کاربری یافت نشد !");
                }
            }
            return View();
        }

        public ActionResult LogOff()
        {
            PardisDBEntities db = new PardisDBEntities();

            long uid = db.Users.SingleOrDefault(q => q.UserName == User.Identity.Name) != null ?
                        db.Users.SingleOrDefault(q => q.UserName == User.Identity.Name).UserID : 0;
            LogUserLogin objLogOutLog = new LogUserLogin()
            {

                LogDate = System.DateTime.Now,
                Status = 2,
                UserID = uid,
            };
            Log(objLogOutLog);
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        private void Log(LogUserLogin objlog)
        {
            PardisDBEntities db = new PardisDBEntities();
            if (objlog != null) {
                try {
                    db.LogUserLogin.Add(objlog);
                    db.SaveChanges();
                }
                catch(Exception ex) { }
            }
        }

    }
}