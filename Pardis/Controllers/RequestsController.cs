using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataLayer;

namespace Pardis.Controllers
{
    public class RequestsController : Controller
    {
        PardisDBEntities db = new PardisDBEntities();

        /*public ActionResult CardtableView(int Id)
        {
            return View(Id);
        }*/


        public ActionResult GetViewPathByMenuID(int id)
        {
            WorkFlowController workFlowCtrl = new WorkFlowController();
            string error = "";
            Menus objMenu = workFlowCtrl.GetMenuInfoByID(id, out error);
            if (!string.IsNullOrEmpty(error) || objMenu == null)
            {
                ViewBag.FoundMenuError = error;
                return null;
            }
            else
            {
                Session["SelectedMenuID"] = id;
                Session["SelectedNodeID"] = objMenu.NodeID;
                Session["MenuPath"] = objMenu.Path;
                
                if (objMenu.IsCartable)
                {
                    #region
                        string firstNodeError = "";
                        bool isFirstNode = workFlowCtrl.isFirstWorkFlowNode(objMenu.NodeID, out firstNodeError);
                        ViewBag.IsFirstNode = isFirstNode;
                        ViewBag.IsFirstNodeError = firstNodeError;

                        string typeWorkflowError = "";
                        int workflowTypeId = workFlowCtrl.GetWorkFlowTypeByNodeId(objMenu.NodeID, out typeWorkflowError);
                        ViewBag.WorkFlowType = workflowTypeId;
                        ViewBag.WorkFlowTypeError = typeWorkflowError;

                        string requestsError = "";
                        List<vwRequestNodeStatus> lstRequest = workFlowCtrl.GetRequestsByNodeId(objMenu.NodeID, out requestsError);
                        ViewBag.RequestError = requestsError;

                        Session["IsFirstNodeId"] = isFirstNode ? 1 : 0;
                        Session["WorkFlowTypeID"] = workflowTypeId;
                    #endregion

                    return View("CardtableView", lstRequest);
                }
                else
                {
                    return View("");
                }
            }
        }

        public ActionResult RegisterNewRequest()
        {
            if (Session["MenuPath"] != null) {
                switch (Session["Menupath"].ToString()) {
                    case "/SupplyEquipmentWorkFlow/RegisterSupplyEquipmentRequest":
                        Session["RequestID"] = 0;
                        return View(".." + Session["MenuPath"].ToString());
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }


        public ActionResult CardtableView(int Id)
        {
            WorkFlowController workCtrl = new WorkFlowController();
            string firstNodeError = "";
            bool isFirstNode = workCtrl.isFirstWorkFlowNode(Id, out firstNodeError);
            ViewBag.IsFirstNode = isFirstNode;
            ViewBag.IsFirstNodeError = firstNodeError;

            string typeWorkflowError = "";
            int workflowTypeId = workCtrl.GetWorkFlowTypeByNodeId(Id, out typeWorkflowError);
            ViewBag.WorkFlowType = workflowTypeId;
            ViewBag.WorkFlowTypeError = typeWorkflowError;

            string requestsError = "";
            List<vwRequestNodeStatus> lstRequest = workCtrl.GetRequestsByNodeId(Id, out requestsError);
            ViewBag.RequestError = requestsError;

            return View(lstRequest);
        }

        [HttpPost]
        public JsonResult GetCartableInfoByNodeId(int Id)
        {
            WorkFlowController workCtrl = new WorkFlowController();
            string firstNodeError = "";
            string typeWorkflowError = "";
            string requestsError = "";
            List<vwRequestNodeStatus> lstRequest = workCtrl.GetRequestsByNodeId(Id, out requestsError);
            bool isFirstNode = workCtrl.isFirstWorkFlowNode(Id, out firstNodeError);
            int workflowTypeId = workCtrl.GetWorkFlowTypeByNodeId(Id, out typeWorkflowError);

            return Json(new {
                RequestError = requestsError,
                RequestList = lstRequest,
                WorkFlowError = typeWorkflowError,
                WorkFlowType = workflowTypeId,
                FirstNodeError = firstNodeError,
                IsFirstNodeId = isFirstNode
            }, JsonRequestBehavior.AllowGet);

        }
    }
}