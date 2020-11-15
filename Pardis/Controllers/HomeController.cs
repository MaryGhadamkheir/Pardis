using DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pardis.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSliderImages()
        {
            PardisDBEntities db = new PardisDBEntities();
            try
            {
                List<Sliders> lstSliders = db.Sliders.Where(q => q.IsActive == true).ToList();
                var result = new
                {
                    imagesList = lstSliders.ToArray(),
                    ErrorMessage = "",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                var result = new
                {
                    imagesList = new Sliders(),
                    ErrorMessage = "",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetTaskList() {
            PardisDBEntities db = new PardisDBEntities();
            try
            {
                List<vwTask> lsttask = db.vwTask.Where(w => w.Status == 1).ToList();

                List<UserTaskViewModel> lstUserTask = new List<UserTaskViewModel>();
                if (lsttask != null) {

                    string faCreateDate = "";
                    string faStartDate = "";
                    string faEndDate = "";
                    foreach (var item in lsttask)
                    {
                        faCreateDate = "";
                        if (item.CreateDate != null) {
                            PersianCalendar pc = new PersianCalendar();
                            faCreateDate = string.Format("{0}/{1}/{2}", pc.GetYear(item.CreateDate), pc.GetMonth(item.CreateDate), pc.GetDayOfMonth(item.CreateDate));
                        }
                        faStartDate = "";
                        if (item.StartDate!= null)
                        {
                            PersianCalendar pc = new PersianCalendar();
                            faStartDate = string.Format("{0}/{1}/{2}", pc.GetYear(Convert.ToDateTime(item.StartDate)), pc.GetMonth(Convert.ToDateTime(item.StartDate)), 
                                            pc.GetDayOfMonth(Convert.ToDateTime(item.StartDate)));
                        }
                        faEndDate= "";
                        if (item.EndDate!= null)
                        {
                            PersianCalendar pc = new PersianCalendar();
                            faEndDate = string.Format("{0}/{1}/{2}", pc.GetYear(Convert.ToDateTime(item.EndDate)), pc.GetMonth(Convert.ToDateTime(item.EndDate)),
                                            pc.GetDayOfMonth(Convert.ToDateTime(item.EndDate)));
                        }
                        lstUserTask.Add(new UserTaskViewModel()
                        {
                            faCreateDate = faCreateDate,
                            faEndDate = faEndDate,
                            faStartDate = faStartDate,
                            Status = item.Status,
                            StatusTitle = item.StatusTitle,
                            TaskID = item.TaskID,
                            TaskName = item.TaskName,
                            UserID = item.UserID,
                            UserName = item.UserName,
                            UTID = item.UTID
                        });
                    }
                }


                var result = new
                {
                    taskList = lstUserTask.ToArray(),
                    ErrorMessage = "",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch {
                var result = new
                {
                    taskList = new List<UserTaskViewModel>(),
                    ErrorMessage = "در بدست آوردن لیست کارهای در انتظار بررسی خطایی رخ داده است !",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}