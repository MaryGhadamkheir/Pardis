using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataLayer;

namespace Pardis.Controllers
{
    public class DefaultController : Controller
    {
        PardisDBEntities db = new PardisDBEntities();

        // GET: Default
        public ActionResult Index()
        {
            Session["SelectedMenuID"] = 0;
            Session["SelectedNodeID"] = 0;
            Session["MenuPath"] = "";
            Session["IsFirstNodeId"] = 0;
            Session["WorkFlowTypeID"] = 0;
            Session["RequestID"] = 0;
            return View();
        }


        public ActionResult GetAllMenus()
        {
            ViewBag.NoAccess = false;
            ViewBag.ErrorGetMenu = false;
            try
            {
                List<vwUserGroups> lstUserGroup = db.vwUserGroups.Where(q => q.UserName == User.Identity.Name).ToList();
                if (lstUserGroup == null || lstUserGroup.Count == 0)
                {
                    ViewBag.NoAccess = true;
                    return PartialView(null);
                }
                else
                {
                    List<int> lstUG = lstUserGroup.Select(q => q.GroupID).ToList();
                    List<vwMenuGroup> lstMenus = db.vwMenuGroup.Where(q => lstUG.Contains(q.GroupID)).ToList();
                    return PartialView(lstMenus);
                }
            }
            catch
            {
                ViewBag.ErrorGetMenu = true;
                return PartialView(null);
            }
        }

    }
}