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
    
    public partial class ServiceRequest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServiceRequest()
        {
            this.ServiceRequest_Invoice = new HashSet<ServiceRequest_Invoice>();
            this.ServiceRequest_Quote = new HashSet<ServiceRequest_Quote>();
            this.ServiceRequest_Rating = new HashSet<ServiceRequest_Rating>();
        }
    
        public int Id { get; set; }
        public string RequesterAspNetUserId { get; set; }
        public int ServiceProviderLocationId { get; set; }
        public System.DateTime RequestFromDate { get; set; }
        public Nullable<System.DateTime> RequestToDate { get; set; }
        public string Details { get; set; }
        public Nullable<System.DateTime> DateAccepted { get; set; }
        public Nullable<System.DateTime> DateCompleted { get; set; }
        public Nullable<decimal> ActualCalloutFee { get; set; }
        public Nullable<System.DateTime> TermsAcceptedDate { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual ServiceProvider_Location ServiceProvider_Location { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceRequest_Invoice> ServiceRequest_Invoice { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceRequest_Quote> ServiceRequest_Quote { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceRequest_Rating> ServiceRequest_Rating { get; set; }
    }
}
