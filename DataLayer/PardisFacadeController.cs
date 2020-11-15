using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class PardisFacadeController
    {

        public bool CreateSupplyEquipmentRequest(SupplyEquipments objSupplyEquipment, short RequestTypeId, short WorkFlowID, int FromNodeID, short StatusCode, string FromUserAccount,
                                        string ToUserAccount, int GroupID, string Description, string ClientIP, bool isTransfer, out string error, out long requestId)
        {
            error = "";
            requestId = 0;
            WorkFlowController workCtrl = new WorkFlowController();

            if (workCtrl.CreateRequestActivity(RequestTypeId, WorkFlowID, FromNodeID, StatusCode, FromUserAccount,
                                                ToUserAccount, GroupID, Description, ClientIP, isTransfer, out error, out requestId) && requestId > 0 )
            {
                try
                {
                    PardisDBEntities db = new PardisDBEntities();
                    objSupplyEquipment.RequestID = requestId;
                    db.SupplyEquipments.Add(objSupplyEquipment);
                    int result = db.SaveChanges();
                    return result > 0 ? true : false;
                }
                catch
                {
                    error = "در ثبت اطلاعات خطایی رخ داده است !";
                    return false;
                }
            }
            else
            {
                error = string.IsNullOrEmpty(error) ? "در ایجاد درخواست خطایی رخ داده است !" : error;
                return false;
            }

        }

    }
}
