using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataLayer;
using DataLayer.ViewModels;
using Pardis.Content.Constants;
using Pardis.Utility;

namespace Pardis.Controllers
{
    public class SupplyEquipmentWorkFlowController : Controller
    {

        private long ReqID
        {
            get
            {
                if (Session["RequestID"] != null)
                {
                    return Convert.ToInt64(Session["RequestID"].ToString());
                }
                return 0;
            }
            set { Session["RequestID"] = value; }
        }
        private short WorkFlowID
        {
            get
            {
                if (Session["WorkFlowTypeID"] != null)
                {
                    return Convert.ToInt16(Session["WorkFlowTypeID"].ToString());
                }
                return 0;
            }
            set { Session["WorkFlowTypeID"] = value; }
        }
        private short NodeID
        {
            get
            {
                if (Session["SelectedNodeID"] != null)
                {
                    return Convert.ToInt16(Session["SelectedNodeID"].ToString());
                }
                return 0;
            }
            set { Session["SelectedNodeID"] = value; }
        }


        public ActionResult RegisterSupplyEquipmentRequest()
        {
            if (Session["RequestID"] != null)
            {
                return View();
            }
            else
            {
                return View();
            }
        }

        public JsonResult GetAllProfiles()
        {
            PardisDBEntities db = new PardisDBEntities();
            try
            {
                List<tbl_profiles> lstProfiles = db.tbl_profiles.ToList();

                List<KeyPairValueViewModel> lstProfileVM = new List<KeyPairValueViewModel>();
                if (lstProfiles != null)
                {
                    foreach (var item in lstProfiles)
                    {
                        lstProfileVM.Add(new KeyPairValueViewModel()
                        {
                            Key = item.id,
                            Value = item.name
                        });
                    }
                }

                var result = new
                {
                    profileList = lstProfileVM.ToArray(),
                    ErrorMessage = "",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                var result = new
                {
                    profileList = new List<KeyPairValueViewModel>(),
                    ErrorMessage = "در بدست آوردن لیست مشتریان موجود خطایی رخ داده است !",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetNeedsByprofileID(int profileId)
        {
            PardisDBEntities db = new PardisDBEntities();
            try
            {
                List<tbl_needs> lstneeds = db.tbl_needs.Where(q => q.profileid == profileId).ToList();

                List<KeyPairValueViewModel> lstNeedVM = new List<KeyPairValueViewModel>();
                if (lstneeds != null)
                {
                    foreach (var item in lstneeds)
                    {
                        lstNeedVM.Add(new KeyPairValueViewModel()
                        {
                            Key = item.id,
                            Value = item.title
                        });
                    }
                }

                var result = new
                {
                    needList = lstNeedVM.ToArray(),
                    ErrorMessage = "",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                var result = new
                {
                    needList = new List<KeyPairValueViewModel>(),
                    ErrorMessage = "در بدست آوردن لیست نیازسنجی های مشتری موجود خطایی رخ داده است !",
                    successful = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSupplyEquipmentRequestTypes()
        {
            List<KeyPairValueViewModel> lstReqtypes = new List<KeyPairValueViewModel>();
            foreach (var item in Enum.GetValues(typeof(Content.Constants.Enums.SupplyEquipmentRequestTypes)))
            {
                switch (Convert.ToInt32(item))
                {
                    case (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.InternalPurchase:
                        lstReqtypes.Add(new KeyPairValueViewModel() { Key = (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.InternalPurchase, Value = "خرید داخلی" });
                        break;
                    case (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.TenderOffer:
                        lstReqtypes.Add(new KeyPairValueViewModel() { Key = (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.TenderOffer, Value = "مناقصه" });
                        break;
                    case (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.Inquiry:
                        lstReqtypes.Add(new KeyPairValueViewModel() { Key = (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.Inquiry, Value = "استعلام" });
                        break;
                    case (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.PriceAnnunciation:
                        lstReqtypes.Add(new KeyPairValueViewModel() { Key = (int)Pardis.Content.Constants.Enums.SupplyEquipmentRequestTypes.PriceAnnunciation, Value = "اعلام قیمت" });
                        break;
                    default:
                        break;
                }
            }
            var result = new
            {
                ReqTypeList = lstReqtypes,
                ErrorMessage = "",
                successful = true
            };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public JsonResult SaveSupplyEquipment(SupplyEquipmentViewModel objSE, string activityDesc, bool isSendRequest)
        {
            #region 'Validation'
            if (!Utility.Utility.IsDateValid(objSE.faPriceAnnouncementDeadline, true))
            {
                var rs = new
                {
                    ID = objSE.ID,
                    ErrorMessage = "فرمت تاریخ مهلت اعلام قیمت صحیح نمی باشد !",
                    successful = false
                };
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            else
            {
                objSE.PriceAnnouncementDeadline = Utility.Utility.ShamsiDateToGregorianDate(objSE.faPriceAnnouncementDeadline);
            }
            if (!Utility.Utility.IsDateValid(objSE.faDeliveryDeadline.ToString(), true))
            {
                var rs2 = new
                {
                    ID = objSE.ID,
                    ErrorMessage = "فرمت تاریخ مهلت تحویل صحیح نمی باشد !",
                    successful = false
                };
                return Json(rs2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                objSE.DeliveryDeadline = Utility.Utility.ShamsiDateToGregorianDate(objSE.faDeliveryDeadline);
            }
            #endregion
            PardisFacadeController facadeCtrl = new PardisFacadeController();
            SupplyEquipments obj = Utility.Utility.Cast<SupplyEquipments>(objSE);
            obj.RequestID = ReqID;
            if (ReqID == 0)
            {
                string ErrorStr = "";
                long RID = 0;
                bool result = facadeCtrl.CreateSupplyEquipmentRequest(obj, WorkFlowID, WorkFlowID, NodeID, (short)DataLayer.Enums.StatusCodeTypes.Confirm, User.Identity.Name,
                                                                    "", 0, activityDesc, Request.UserHostAddress, isSendRequest, out ErrorStr, out RID);
                if (result)
                {
                    var rs = new
                    {
                        RequestID = RID,
                        ErrorMessage = ErrorStr,
                        successful = true
                    };
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rs = new
                    {
                        ID = RID,
                        ErrorMessage = ErrorStr,
                        successful = false
                    };
                    return Json(rs, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var rs = new
                {
                    ID = objSE.ID,
                    ErrorMessage = "فرمت تاریخ مهلت اعلام قیمت صحیح نمی باشد !",
                    successful = false
                };
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
        }

        //public JsonResult SaveSupplyEqipment(SupplyEquipmentViewModel objSupplyEquipment, bool isSendRequest)
        //{
        //    try
        //    {
        //        #region 'Validation'
        //        if (!Utility.Utility.IsDateValid(objSupplyEquipment.faPriceAnnouncementDeadline, true))
        //        {
        //            var rs = new
        //            {
        //                ID = objSupplyEquipment.ID,
        //                ErrorMessage = "فرمت تاریخ مهلت اعلام قیمت صحیح نمی باشد !",
        //                successful = false
        //            };
        //            return Json(rs, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            objSupplyEquipment.PriceAnnouncementDeadline = Utility.Utility.ShamsiDateToGregorianDate(objSupplyEquipment.faPriceAnnouncementDeadline);
        //        }
        //        if (!Utility.Utility.IsDateValid(objSupplyEquipment.faDeliveryDeadline.ToString(), true))
        //        {
        //            var rs2 = new
        //            {
        //                ID = objSupplyEquipment.ID,
        //                ErrorMessage = "فرمت تاریخ مهلت تحویل صحیح نمی باشد !",
        //                successful = false
        //            };
        //            return Json(rs2, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            objSupplyEquipment.DeliveryDeadline = Utility.Utility.ShamsiDateToGregorianDate(objSupplyEquipment.faDeliveryDeadline);
        //        }
        //        #endregion

                

        //        PardisFacadeController ctrl = new PardisFacadeController();
        //        if (ctrl.SaveSupplyEquipment(obj))
        //        {
        //            if (isSendRequest)
        //            {

        //            }
        //            else
        //            {
        //                var result = new
        //                {
        //                    ID = obj.ID,
        //                    ErrorMessage = "",
        //                    successful = true
        //                };
        //                return Json(result, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        else
        //        {
        //            var result = new
        //            {
        //                ID = obj.ID,
        //                ErrorMessage = "در ثبت اطلاعات خطایی رخ داده است !",
        //                successful = false
        //            };
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        var result2 = new
        //        {
        //            ID = objSupplyEquipment.ID,
        //            ErrorMessage = ex.Message,
        //            successful = false
        //        };
        //        return Json(result2, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}