//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JobMe
{
    using System;
    using System.Collections.Generic;
    
    public partial class AvailableService
    {
        public int ID { get; set; }
        public string SubServiceType { get; set; }
        public string ServiceType { get; set; }
        public int ServiceProviderId { get; set; }
        public int SubServiceTypeId { get; set; }
        public int ServiceTypeID { get; set; }
        public string ServiceProviderName { get; set; }
        public string Logo { get; set; }
        public int Rank { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public Nullable<decimal> CalloutFee { get; set; }
    }
}
