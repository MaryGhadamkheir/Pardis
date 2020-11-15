using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Enums
    {
        public enum StatusCodeTypes : short
        {
            /// <summary>
            /// در انتظار بررسی 
            /// </summary>
            Waiting = 0,
            /// <summary>
            /// اقدام
            /// </summary>
            Action = 1,
            /// <summary>
            /// تائید
            /// </summary>
            Confirm = 2,
            /// <summary>
            /// عدم تائید
            /// </summary>
            Reject = 3,
            /// <summary>
            /// برگشت
            /// </summary>
            Return = 4,
            /// <summary>
            /// پایان
            /// </summary>
            Finish = 5
        }
    }
}
