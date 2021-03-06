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
    
    public partial class ServiceRequest_Quote_Line
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServiceRequest_Quote_Line()
        {
            this.ServiceRequest_Invoice_Line = new HashSet<ServiceRequest_Invoice_Line>();
            this.ServiceRequest_Quote_Line_Item = new HashSet<ServiceRequest_Quote_Line_Item>();
        }
    
        public int Id { get; set; }
        public int ServiceRequestQuoteId { get; set; }
        public string Description { get; set; }
        public string PaymentGatewayRefNo { get; set; }
        public Nullable<System.DateTime> DateOfPayment { get; set; }
        public Nullable<decimal> PaymentTotal { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceRequest_Invoice_Line> ServiceRequest_Invoice_Line { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceRequest_Quote_Line_Item> ServiceRequest_Quote_Line_Item { get; set; }
        public virtual ServiceRequest_Quote ServiceRequest_Quote { get; set; }
    }
}
