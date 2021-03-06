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
    
    public partial class ServiceRequest_Invoice_Line_Item
    {
        public int Id { get; set; }
        public int ServiceRequestLineId { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public Nullable<int> ServiceRequestQuoteLineItemId { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual ServiceRequest_Invoice_Line ServiceRequest_Invoice_Line { get; set; }
        public virtual ServiceRequest_Quote_Line_Item ServiceRequest_Quote_Line_Item { get; set; }
    }
}
