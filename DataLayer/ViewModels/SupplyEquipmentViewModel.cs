using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.ViewModels
{
    public class SupplyEquipmentViewModel
    {
        public Int64 ID { get; set; }
        public Int32 ProfileID { get; set; }
        public Int32 NeedID { get; set; }
        public string RequestTitle { get; set; }
        public DateTime? PriceAnnouncementDeadline { get; set; }
        public string faPriceAnnouncementDeadline { get; set; }
        public DateTime? DeliveryDeadline { get; set; }
        public string faDeliveryDeadline { get; set; }
        public Int16? RequestTypeID { get; set; }
        public string WarrantyTerms { get; set; }
        public string Description { get; set; }
    }
}
