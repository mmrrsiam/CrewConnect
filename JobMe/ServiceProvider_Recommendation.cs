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
    
    public partial class ServiceProvider_Recommendation
    {
        public int Id { get; set; }
        public string RecommenderAspNetUserId { get; set; }
        public int ServiceProviderId { get; set; }
        public string Recommendation { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual ServiceProvider ServiceProvider { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
