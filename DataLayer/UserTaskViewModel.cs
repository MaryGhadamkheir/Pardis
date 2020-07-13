using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    [Serializable]
    public class UserTaskViewModel
    {
        public Int64 UTID { get; set; }
        public Int64 UserID { get; set; }
        public string UserName { get; set; }
        public Int64 TaskID { get; set; }
        public string TaskName { get; set; }
        public DateTime CreateDate { get; set; }
        public string faCreateDate { get; set; }
        public short Status { get; set; }
        public string StatusTitle { get; set; }
        public DateTime? StartDate { get; set; }
        public string faStartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string faEndDate { get; set; }

    }
}
