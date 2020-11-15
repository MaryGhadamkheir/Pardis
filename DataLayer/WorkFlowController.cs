using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class WorkFlowController
    {
        public Menus GetMenuInfoByID(int id, out string error)
        {
            error = "";
            try
            {
                PardisDBEntities db = new PardisDBEntities();
                Menus objMenu = db.Menus.Where(w => w.ID == id).SingleOrDefault();
                if (objMenu != null)
                    return objMenu;
                return null;
            }
            catch
            {
                error = "در بدست آوردن اطلاعات منو خطایی رخ داده است !";
                return null;
            }
        }

        public bool isFirstWorkFlowNode(int nodeId, out string errorStr)
        {
            errorStr = "";
            try
            {
                PardisDBEntities db = new PardisDBEntities();
                Workflows objWorkflows = db.Workflows.Where(q => q.StartNodeID == nodeId).FirstOrDefault();
                if (objWorkflows != null)
                    return true;
                return false;
            }
            catch
            {
                errorStr = "سیستم قادر به بدست آوردن وضعیت گره جاری نمی باشد !";
                return false;
            }
        }

        public int GetWorkFlowTypeByNodeId(int nodeId, out string errorStr)
        {
            try
            {
                errorStr = "";
                PardisDBEntities db = new PardisDBEntities();
                WFAssignments objAssgnment = db.WFAssignments.Where(w => w.FromNodeID == nodeId || w.ToNodeID == nodeId).FirstOrDefault();

                if (objAssgnment != null)
                {
                    return objAssgnment.WorkFlowID;
                }

                return 0;
            }
            catch
            {
                errorStr = "سیستم قادر به بدست آوردن نوع گردش کار نمی باشد !";
                return 0;
            }
        }

        public List<vwRequestNodeStatus> GetRequestsByNodeId(int nodeid, out string errorStr)
        {
            errorStr = "";
            PardisDBEntities db = new PardisDBEntities();
            List<int> lstStatus = new List<int>();
            lstStatus.Add(0);
            lstStatus.Add(1);
            IEnumerable<vwRequestNodeStatus> query = db.vwRequestNodeStatus.Where(w => w.ToNodeID == nodeid);
            query = query.Where(q => lstStatus.Contains(q.StatusId));
            query = query.Where(q => q.LogicalDelete == false);
            try
            {
                List<vwRequestNodeStatus> lstrequests = query.ToList();
                if (lstrequests != null)
                {
                    lstrequests = lstrequests.GroupBy(q => q.RequestID).Select(grp => grp.First()).ToList();
                }
                return lstrequests;
            }
            catch(Exception ex)
            {
                errorStr = "در بدست آوردن درخواستهای نود جاری خطایی رخ داده است !";
                return null;
            }
        }

        public Requests GeneratePureRequest(string UserAccount, short RequestType)
        {
            Requests objReq = new Requests();
            objReq.RegDate = DateTime.Now;
            objReq.UserAccount = UserAccount;
            objReq.RequestTypeID = RequestType;

            try
            {
                PardisDBEntities db = new PardisDBEntities();
                db.Requests.Add(objReq);
                int rs = db.SaveChanges();
                if (rs > 0)
                    return objReq;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsExistsWay(short workFlow, int fromNodeId, int toNodeId, short statusCode)
        {
            PardisDBEntities db = new PardisDBEntities();
            try
            {
                WFAssignments objAssignment = db.WFAssignments.Where(w => w.WorkFlowID == workFlow && w.FromNodeID == fromNodeId && w.ToNodeID == toNodeId).FirstOrDefault();
                if (objAssignment != null)
                {
                    bool flag = true;
                    switch (statusCode)
                    {
                        case 2:
                            if (!objAssignment.DoConfirm)
                                return false;
                            return true;
                        case 3:
                            if (!objAssignment.DoReject)
                                return false;
                            return true;
                        case 4:
                            if (!objAssignment.DoReturn)
                                return false;
                            return true;
                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public int GetToNodeIdtoTransferRequestByStatus(short workflow, int fromNodeId, short statusCode)
        {
            try
            {
                PardisDBEntities db = new PardisDBEntities();
                switch (statusCode)
                {
                    case (int)Enums.StatusCodeTypes.Confirm:
                        WFAssignments objA = db.WFAssignments.Where(q => q.WorkFlowID == workflow && q.FromNodeID == fromNodeId && q.DoConfirm == true).FirstOrDefault();
                        if (objA == null)
                            return 0;
                        else
                            return objA.ToNodeID;
                    case (int)Enums.StatusCodeTypes.Reject:
                        WFAssignments objA2 = db.WFAssignments.Where(q => q.WorkFlowID == workflow && q.FromNodeID == fromNodeId && q.DoReject == true).FirstOrDefault();
                        if (objA2 == null)
                            return 0;
                        else
                            return objA2.ToNodeID;
                    case (int)Enums.StatusCodeTypes.Return:
                        WFAssignments objA3 = db.WFAssignments.Where(q => q.WorkFlowID == workflow && q.FromNodeID == fromNodeId && q.DoReturn == true).FirstOrDefault();
                        if (objA3 == null)
                            return 0;
                        else
                            return objA3.ToNodeID;
                    default:
                        return 0;
                }

            }
            catch( Exception ex)
            {
                return 0;
            }
        }

        public Boolean CreateRequestActivity(short RequestTypeId, short WorkFlowID, int FromNodeID, short StatusCode, string FromUserAccount,
                                            string ToUserAccount, int GroupID, string Description, string ClientIP, bool isTransfer, out string error, out long requestId)
        {
            error = "";
            requestId = 0;
            PardisDBEntities db = new PardisDBEntities();
            int StartNode = 0;
            #region 'تعیین نود شروع'
            try
            {
                Workflows objWorkFlow = db.Workflows.Where(q => q.ID == WorkFlowID).SingleOrDefault();
                if (objWorkFlow == null)
                {
                    error = "گردش کار معتبر نمی باشد !";
                    return false;
                }
                else if (objWorkFlow.StartNodeID == 0)
                {
                    error = "نود شروع گردش کار جاری مشخص نشده است !";
                    return false;
                }
                else if (objWorkFlow.StartNodeID != FromNodeID)
                {
                    error = "نود شروع گردش کار با نود اقدام کننده یکی نمی باشد !";
                    return false;
                }
                StartNode = objWorkFlow.StartNodeID;
            }
            catch(Exception ex)
            {
                error = "در بدست آوردن نود شروع گردش کار جاری خطایی رخ داده است !";
                return false;
            }
            #endregion
            Requests objRequest = GeneratePureRequest(FromUserAccount, RequestTypeId);
            if (objRequest == null)
            {
                error = "در ایجاد درخواست خطایی رخ داده است !";
                return false;
            }
            
            WFActivity objSourceActivity = new WFActivity();
            objSourceActivity.WorkFlowID = WorkFlowID;
            objSourceActivity.RequestID = objRequest.ID;
            objSourceActivity.PreActivityID = 0;
            objSourceActivity.FromNodeID = StartNode;
            objSourceActivity.ToNodeID = FromNodeID;
            objSourceActivity.StatusId = (short)Enums.StatusCodeTypes.Waiting;
            objSourceActivity.GroupID = GroupID;
            objSourceActivity.LogicalDelete = false;
            objSourceActivity.ToUserAccount = "";
            objSourceActivity.UserAccount = FromUserAccount;
            objSourceActivity.ActivityDate = System.DateTime.Now;
            objSourceActivity.ActivityDescription = Description;

            try
            {
                db.WFActivity.Add(objSourceActivity);
                db.SaveChanges();
                if (isTransfer)
                {
                    #region 'Transfer'
                    int toNodeId = GetToNodeIdtoTransferRequestByStatus(WorkFlowID, FromNodeID, StatusCode);
                    if (toNodeId > 0)
                    {
                        try
                        {
                            #region
                            WFActivity objConfirmActivity = new WFActivity();
                            objConfirmActivity.WorkFlowID = WorkFlowID;
                            objConfirmActivity.RequestID = objRequest.ID;
                            objConfirmActivity.PreActivityID = objSourceActivity.ID;
                            objConfirmActivity.FromNodeID = objSourceActivity.FromNodeID;
                            objConfirmActivity.ToNodeID = objSourceActivity.ToNodeID;
                            objConfirmActivity.StatusId = (short)Enums.StatusCodeTypes.Confirm;
                            objConfirmActivity.GroupID = GroupID;
                            objConfirmActivity.LogicalDelete = false;
                            objConfirmActivity.ToUserAccount = "";
                            objConfirmActivity.UserAccount = FromUserAccount;
                            objConfirmActivity.ActivityDate = System.DateTime.Now;
                            objConfirmActivity.ActivityDescription = Description;
                            db.WFActivity.Add(objConfirmActivity);
                            db.SaveChanges();
                            WFActivity objNextActivity = new WFActivity();
                            objNextActivity.WorkFlowID = WorkFlowID;
                            objNextActivity.RequestID = objRequest.ID;
                            objNextActivity.PreActivityID = objConfirmActivity.ID;
                            objNextActivity.FromNodeID = objSourceActivity.ToNodeID;
                            objNextActivity.ToNodeID = toNodeId;
                            objNextActivity.StatusId = (short)Enums.StatusCodeTypes.Waiting;
                            objNextActivity.GroupID = GroupID;
                            objNextActivity.LogicalDelete = false;
                            objNextActivity.ToUserAccount = ToUserAccount;
                            objNextActivity.UserAccount = FromUserAccount;
                            objNextActivity.ActivityDate = System.DateTime.Now;
                            objNextActivity.ActivityDescription = Description;
                            db.WFActivity.Add(objNextActivity);
                            db.SaveChanges();
                            #endregion
                            return true;
                        }
                        catch
                        {
                            error = "در ارسال درخواست خطایی رخ داده است !";
                            return false;
                        }
                    }
                    else
                    {
                        error = "از گره جاری مسیری جهت ارسال درخواست وجود ندارد ! ";
                        return false;
                    }
                    #endregion
                }
                requestId = objRequest.ID;
                return true;
            }
            catch (Exception ex) {
                error = "در ثبت درخواست خطایی رخ داده است !";
                return false;
            }

        }

        //public Boolean WFTransferRequest(long RequestID, short RequestTypeId, long WorkFlowID, long FromNodeID, long ToNodeID, int StatusCode, string FromUserAccount,
        //                                    string ToUserAccount, long GroupID, string Description, string ClientIP, out string error)
        //{
        //    error = "";
        //    if (!string.IsNullOrEmpty(FromUserAccount))
        //    {
        //        PardisDBEntities db = new PardisDBEntities();
        //        #region 'Assignment'
        //        try
        //        {
        //            WFAssignments objAssignment = db.WFAssignments.Where(w => w.WorkFlowID == WorkFlowID && w.FromNodeID == FromNodeID && w.ToNodeID == ToNodeID).FirstOrDefault();
        //            if (objAssignment != null)
        //            {
        //                bool flag = true;
        //                switch (StatusCode)
        //                {
        //                    case 2:
        //                        if (!objAssignment.DoConfirm)
        //                            flag = false;
        //                        break;
        //                    case 3:
        //                        if (!objAssignment.DoReject)
        //                            flag = false;
        //                        break;
        //                    case 4:
        //                        if (!objAssignment.DoReturn)
        //                            flag = false;
        //                        break;
        //                }
        //                if (!flag)
        //                {
        //                    error = "هیچ مسیری برای در نقشه فرآیند مورد نظر از گره مبدا به مقصد وجود ندارد";
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                error = "هیچ مسیری برای در نقشه فرآیند مورد نظر از گره مبدا به مقصد وجود ندارد";
        //                return false;
        //            }
        //        }
        //        catch
        //        {
        //            error = "هیچ مسیری برای در نقشه فرآیند مورد نظر از گره مبدا به مقصد وجود ندارد";
        //            return false;
        //        }
        //        #endregion
        //        long GenerateRequestID = RequestID;
        //        if (RequestID == 0)
        //        {
        //            Requests objRequest = GeneratePureRequest(FromUserAccount, RequestTypeId);
        //            if (objRequest == null)
        //            {
        //                error = "در ایجاد درخواست خطایی رخ داده است !";
        //                return false;
        //            }
        //            GenerateRequestID = objRequest.ID;
        //        }




        //        #region 'Activity'

        //        WFActivity objWFActivity = new WFActivity();
        //        objWFActivity.ActivityDate = DateTime.Now;
        //        objWFActivity.ActivityDescription = Description;
        //        objWFActivity.FromNodeID = LastReqActivity.FromNodeID;
        //        objWFActivity.LogicalDelete = false;
        //        objWFActivity.PreActivityID = LastReqActivity.ID;
        //        objWFActivity.RequestID = RequestID;
        //        objWFActivity.StatusCode = StatusCode;
        //        objWFActivity.ToNodeID = LastReqActivity.ToNodeID;
        //        objWFActivity.UserAccount = FromUserAccount;
        //        objWFActivity.WorkFlowID = LastReqActivity.WorkFlowID;
        //        objWFActivity.Inquery = LastReqActivity.Inquery;
        //        objWFActivity.GroupID = LastReqActivity.GroupID;
        //        objWFActivity.RefID = LastReqActivity.RefID;
        //        objWFActivity.ClientIP = ClientIP;
        //        objWFActivity.InquiryResponse = false;
        //        if (LastReqActivity.Inquery)
        //            objWFActivity.InquiryResponse = true;

        //        WFActivityCtrl.Add(objWFActivity);

        //        #endregion


        //        WorkFlowsController WFCtrl = new WorkFlowsController();
        //        WorkFlows objWorkFlows = WFCtrl.GetByID(WorkFlowID);
        //        if (objWorkFlows != null)
        //        {
                    

        //            vwRequestNodeStatusController vwReqNodeStatusCtrl = new vwRequestNodeStatusController();
        //            List<vwRequestNodeStatus> SrcReqNodeStateRefList = vwReqNodeStatusCtrl.GetRequestNodeStateReferences(RequestID, WorkFlowID, FromNodeID);
        //            vwRequestNodeStatus objSrcReqNodeStatus = vwReqNodeStatusCtrl.GetRequestNodeState(RequestID, WorkFlowID, FromNodeID, RefID, false);
        //            vwRequestNodeStatus objDesReqNodeStatus = vwReqNodeStatusCtrl.GetRequestNodeState(RequestID, WorkFlowID, ToNodeID, RefID, false);

        //            using (TransactionScope ts = new TransactionScope(System.Transactions.TransactionScopeOption.Required, Utility.Helper.GetUnCommittedTransactionOption()))
        //            {
        //                try
        //                {
        //                    if (objSrcReqNodeStatus != null)
        //                    {
        //                        WFActivityController WFActivityCtrl = new WFActivityController();
        //                        WFActivity LastReqActivity = WFActivityCtrl.GetByID(Convert.ToInt64(objSrcReqNodeStatus.ID));
        //                        int ErrorCode = 0;
        //                        if (IsValidStateToChange(RequestID, WorkFlowID, FromNodeID, StatusCode, LastReqActivity, FromUserAccount, Description, RefID, SrcReqNodeStateRefList, ref ErrorCode))
        //                        {
        //                            WFActivity objWFActivity = new WFActivity();
        //                            objWFActivity.ActivityDate = DateTime.Now;
        //                            objWFActivity.Description = Description;
        //                            objWFActivity.FromNodeID = LastReqActivity.FromNodeID;
        //                            objWFActivity.LogicalDelete = false;
        //                            objWFActivity.PreActivityID = LastReqActivity.ID;
        //                            objWFActivity.RequestID = RequestID;
        //                            objWFActivity.StatusCode = StatusCode;
        //                            objWFActivity.ToNodeID = LastReqActivity.ToNodeID;
        //                            objWFActivity.UserAccount = FromUserAccount;
        //                            objWFActivity.WorkFlowID = LastReqActivity.WorkFlowID;
        //                            objWFActivity.Inquery = LastReqActivity.Inquery;
        //                            objWFActivity.GroupID = LastReqActivity.GroupID;
        //                            objWFActivity.RefID = LastReqActivity.RefID;
        //                            objWFActivity.ClientIP = ClientIP;
        //                            objWFActivity.InquiryResponse = false;
        //                            if (LastReqActivity.Inquery)
        //                                objWFActivity.InquiryResponse = true;

        //                            WFActivityCtrl.Add(objWFActivity);
        //                            if (!LastReqActivity.Inquery && SrcReqNodeStateRefList.Where(c => (c.RefID == RefID) && (c.StatusCode == 0 || c.StatusCode == 1 || c.StatusCode == 5 || c.StatusCode == 4)).Count() == 1)
        //                            {
        //                                //Renew Last activity of source node
        //                                objSrcReqNodeStatus = vwReqNodeStatusCtrl.GetRequestNodeState(RequestID, WorkFlowID, FromNodeID, RefID, false);
        //                                if (objSrcReqNodeStatus != null)
        //                                {
        //                                    objWFActivity = new WFActivity();
        //                                    objWFActivity.ActivityDate = DateTime.Now;
        //                                    objWFActivity.Description = Description;
        //                                    objWFActivity.FromNodeID = FromNodeID;
        //                                    objWFActivity.LogicalDelete = false;
        //                                    objWFActivity.PreActivityID = Convert.ToInt64(objSrcReqNodeStatus.PreActivityID);
        //                                    objWFActivity.RequestID = RequestID;
        //                                    objWFActivity.ClientIP = ClientIP;
        //                                    if (RefID == 0)
        //                                    {
        //                                        if (ToNodeID == objWorkFlows.EndNodeID)
        //                                            objWFActivity.StatusCode = StatusCodeTypes.Finish;
        //                                        else
        //                                            objWFActivity.StatusCode = StatusCodeTypes.Waiting;
        //                                    }
        //                                    else
        //                                    {
        //                                        // تشخیص اینکه چنانچه گره مقصد ، همان گره شروع کننده ارجاع می باشد، به معنی پایان روند ارجاع در نظر گرفته شود
        //                                        WFActivity obj = WFActivityCtrl.GetActivitiesByRequestID(RequestID).Where(x => x.RefID == RefID).OrderBy(x => x.ID).FirstOrDefault();
        //                                        if (obj != null)
        //                                        {
        //                                            obj = WFActivityCtrl.GetActivitiesByRequestID(RequestID).Where(x => x.ID == obj.PreActivityID).FirstOrDefault();
        //                                            if (obj != null && (obj.ToNodeID == ToNodeID || ToNodeID == objWorkFlows.EndNodeID))
        //                                                objWFActivity.StatusCode = StatusCodeTypes.Finish;
        //                                            else
        //                                                objWFActivity.StatusCode = StatusCodeTypes.Waiting;
        //                                        }
        //                                        else
        //                                            objWFActivity.StatusCode = StatusCodeTypes.Waiting;
        //                                    }
        //                                    objWFActivity.ToNodeID = ToNodeID;
        //                                    objWFActivity.UserAccount = ToUserAccount;
        //                                    objWFActivity.WorkFlowID = WorkFlowID;
        //                                    objWFActivity.Inquery = false;
        //                                    objWFActivity.GroupID = GroupID;
        //                                    objWFActivity.InquiryResponse = false;
        //                                    objWFActivity.RefID = RefID;
        //                                    WFActivityCtrl.Add(objWFActivity);

        //                                    if (ToNodeID == objWorkFlows.EndNodeID && objWorkFlows.AutoArchiveOnFinish)
        //                                        //ArchiveRequestActivities(WorkFlowID, RequestID);

        //                                        if (objWFActivity.StatusCode == StatusCodeTypes.Waiting && objWFActivity.UserAccount != null && objWFActivity.UserAccount != string.Empty)
        //                                        {
        //                                            NodesController NodeCtrl = new NodesController();
        //                                            Nodes ObjNode = NodeCtrl.GetByID(ToNodeID);
        //                                            UsersController UsersCtrl = new UsersController();
        //                                            Users ObjUser = UsersCtrl.GetUserInfoByUserAccount(objWFActivity.UserAccount);

        //                                            //if (ObjNode != null && ObjNode.SMSOnWait && ObjUser != null && ObjUser.MobileNo != null && ObjUser.MobileNo != string.Empty)
        //                                            //{
        //                                            //    SMSQueue objSMSQueue = new SMSQueue();
        //                                            //    objSMSQueue.DestinationNo = ObjUser.MobileNo;
        //                                            //    objSMSQueue.RegisterDate = DateTime.Now;
        //                                            //    objSMSQueue.SendStatus = false;
        //                                            //    objSMSQueue.RequestID = RequestID;
        //                                            //    objSMSQueue.SMSTemplateID = ObjNode.SMSTemplateID;

        //                                            //    objSMSQueue.SMSText = SMSEngineFacadeController.GetSMSTemplateFullText(objSMSQueue.SMSTemplateID, new object[] { RequestID, ObjNode.Title });

        //                                            //    SMSEngineFacadeController SMSEngineFacadeCtrl = new SMSEngineFacadeController();
        //                                            //    SMSEngineFacadeCtrl.QueueSMS(objSMSQueue);
        //                                            //}
        //                                        }

        //                                    //if (StatusCode == StatusCodeTypes.Return)
        //                                    //{
        //                                    //    if (!IsSourceInquiriedByDestinationNode(RequestID, WorkFlowID, FromNodeID, ToNodeID))
        //                                    //    {
        //                                    //        WFSetNodeState(RequestID, WorkFlowID, FromNodeID, UserAccount, Description, StatusCode);
        //                                    //        WFSetNodeState(RequestID, WorkFlowID, ToNodeID, UserAccount, Description, StatusCodeTypes.Waiting);
        //                                    //    }
        //                                    //}
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Utility.BusinessException(ErrorCode.ToString(), Utility.ErrorType.Error);
        //                        }
        //                    }
        //                    ts.Complete();
        //                }
        //                catch (Utility.BusinessException bex)
        //                {
        //                    ts.Dispose();
        //                    switch (Convert.ToInt32(bex.Message))
        //                    {
        //                        case (int)ErrorType.InValidUserAccount:
        //                            throw new WFBusinessException("نام کاربری اقدام کننده معتبر نمی باشد . امکان تغییر وضعیت درخواست  در گره مورد نظر وجود ندارد", ErrorType.InValidUserAccount);
        //                            break;
        //                        case (int)ErrorType.NodeHasStatus:
        //                            throw new WFBusinessException("با توجه به وضعیت جاری درخواست در گره مورد نظر ، امکان تغییر وضعیت آن وجود ندارد", ErrorType.NodeHasStatus);
        //                            break;
        //                        case (int)ErrorType.NodeIsInquiry:
        //                            throw new WFBusinessException("با توجه به اینکه درخواست مورد نظر در گره جاری در وضعیت استعلام می باشد ، امکان تغییر وضعیت آن به وضعیت جدید وجود ندارد", ErrorType.NodeIsInquiry);
        //                            break;
        //                        case (int)ErrorType.NoReturnValidInInquiry:
        //                            throw new WFBusinessException("با توجه به اینکه گره مورد نظر در وضعیت استعلام قرار دارد ، امکان بازگشت وجود ندارد", ErrorType.NoReturnValidInInquiry);
        //                            break;
        //                        case (int)ErrorType.RequestDoesNotExistInNode:
        //                            throw new WFBusinessException("درخواست انتخابی  توسط کاربر دیگری از مرحله جاری خارج شده است . امکان اقدام برای شما وجود ندارد .", ErrorType.RequestDoesNotExistInNode);
        //                            break;
        //                        case (int)ErrorType.InvalidActivityRefID:
        //                            throw new WFBusinessException("شناسه ارجاع فعالیت مورد نظر در گره مبدا معتبر نمی باشد یا این شماره ارجاع در حال حاضر جاری نیست", ErrorType.InvalidActivityRefID);
        //                            break;
        //                    }
        //                    return false;
        //                }
        //                catch
        //                {
        //                    ts.Dispose();
        //                    return false;
        //                }
        //                _onAfterWFTransferRequest?.Invoke(FromUserAccount);
        //                return true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        error = "کاربر ارسال کننده مشخص نمی باشد !";
        //        return false;
        //    }
        //}
    }
}
