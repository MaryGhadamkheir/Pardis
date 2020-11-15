using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pardis.Content.Constants
{
    public static class Enums
    {
        public enum SupplyEquipmentRequestTypes : int
        {
            /// <summary>
            /// خرید داخلی
            /// </summary>
            InternalPurchase = 1,
            /// <summary>
            /// مناقصه
            /// </summary>
            TenderOffer = 2,
            /// <summary>
            /// استعلام
            /// </summary>
            Inquiry = 3,
            /// <summary>
            /// اعلام قیمت 
            /// </summary>
            PriceAnnunciation = 4
        }

    }
}