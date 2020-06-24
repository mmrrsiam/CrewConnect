using AngleSharp.Media.Dom;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.EnterpriseServices;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using EntityFramework.Extensions;

namespace JobMe
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
        IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    public class JobMeService
    {
        private static string CurrentUri = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Replace(System.Web.HttpContext.Current.Request.Url.PathAndQuery, "");
        private async System.Threading.Tasks.Task<string> EmailAsync(string Email, string Name, string subject, string content)
        {
            var client = new SendGridClient(Properties.Settings.Default.SendGridAPIKey);
            var from = new EmailAddress(Properties.Settings.Default.AdminEmail, Properties.Settings.Default.AdminUsername);
            var to = new EmailAddress(Email, Name);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);

            return "Email Sent";
        }
        private async System.Threading.Tasks.Task<string> EmailAsync(UserInfo user, string subject, string content)
        {
            return await EmailAsync(user.Email, user.FullName, subject, content);
        }

        private string GetCookieValue(string cookieKey)
        {
            string cookieHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Cookie];
            string[] cookies = cookieHeader.Split(';');
            string result = string.Empty;
            bool cookieFound = false;

            foreach (string currentCookie in cookies)
            {
                string cookie = currentCookie.Trim();

                string[] cookieKeyValue = cookie.Split('=');

                if (cookieKeyValue[0] == cookieKey)
                {
                    result = cookieKeyValue[1];
                    cookieFound = true;
                    break;
                }
            }

            if (!cookieFound)
            {
                string msg = string.Format("Unable to find cookie value for cookie key '{0}'", cookieKey);
                throw new KeyNotFoundException(msg);
            }

            return result;
        }

        /// <summary>Sets the cookie header.</summary>
        /// <param name="cookie">The cookie value to set.</param>
        private void SetCookie(string cookie)
        {
            // Set the cookie for all paths.
            cookie = cookie + "; path=/;";
            string currentHeaderValue = WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie];

            if (!string.IsNullOrEmpty(currentHeaderValue))
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie]
                    = currentHeaderValue + "\r\n" + cookie;
            }
            else
            {
                WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie] = cookie;
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string Login(string username, string password, string ip)
        {
            UserInfo ret = null;
            User user = null;

            using (JobMeEntities db = new JobMeEntities())
            {
                try
                {

                    user = db.Users.Single(x => x.Username == username && x.Password == password);

                }
                catch { return "Invalid User or Password"; }
                if (!user.Active)
                {
                    return "User is Inactive.";
                }
            }
            var retString = JsonConvert.SerializeObject(UserInfo.FromTable(user));
            SetCookie(retString);
            HttpContext.Current.Session["User"] = UserInfo.FromTable(user);

            return retString;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetServiceTypesList()
        {
            using (JobMeEntities db = new JobMeEntities())
            {
                try
                {
                    var ret = Newtonsoft.Json.JsonConvert.SerializeObject(db.SubServiceTypes.Where(x => x.IsDeleted == false));
                    return ret;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message + ex.InnerException;
                }
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetUser()
        {
            UserInfo user = null;
            try
            {
                user = (UserInfo)HttpContext.Current.Session["User"];
                if (user == null)
                    return "Login";
            }
            catch (Exception e) { return "Login"; }
            return JsonConvert.SerializeObject(user);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string FindSkillsByRegion(string Skill, double LatA, double LonA, double LatB, double LonB)
        {
            var ret = new List<AvailableService>();

            var rect = new RectangleF((float)Math.Min(LatA, LatB),
                                        (float)Math.Min(LonA, LonB),
                                        (float)Math.Abs(LatA - LatB),
                                        (float)Math.Abs(LonA - LonB));

            using (JobMeEntities db = new JobMeEntities())
            {
                foreach (var lc in db.AvailableServices.Where(x => x.ServiceType.Contains(Skill) || x.SubServiceType.Contains(Skill)))
                {
                    var loc = new PointF((float)lc.Latitude, (float)lc.Longitude);
                    if (rect.Contains(loc))
                        ret.Add(lc);
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(ret);
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetServiceTree()
        {
            var res = new Dictionary<ServiceType, List<object>>();
            using (JobMeEntities db = new JobMeEntities())
            {
                var temp = db.SubServiceTypes.Select(x => new { ID = x.Id, Name = x.Name, ServiceTypeName = x.ServiceType.Name }).ToList();
                var r = JsonConvert.SerializeObject(temp, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return r;
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetServiceProviderInfo(int ServiceProviderID)
        {
            try
            {
                dynamic re = new System.Dynamic.ExpandoObject();

                using (JobMeEntities db = new JobMeEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var info = db.ServiceProviders.Include("ServiceProvider_TermsAndConditions").Single(x => x.Id == ServiceProviderID);
                    return JsonConvert.SerializeObject(info, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            catch
            {
                return "";
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetServiceProviderInfoByLocationID(int ServiceProvider_LocationID)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var info = db.ServiceProvider_Location.Include("ServiceProvider").Include("ServiceProvider_TermsAndConditions").Single(x => x.Id == ServiceProvider_LocationID);
                    return JsonConvert.SerializeObject(info, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            catch
            {
                return "";
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public bool RequestService(int ServiceProvider_LocationID, DateTime FromDate, DateTime ToDate, string Details)
        {
            var res = new Dictionary<ServiceType, List<object>>();
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    var rq = new ServiceRequest();
                    rq.ServiceProviderLocationId = ServiceProvider_LocationID;
                    rq.RequestFromDate = FromDate;
                    rq.RequestToDate = ToDate;
                    rq.Details = Details;
                    rq.TermsAcceptedDate = DateTime.Now;
                    db.ServiceRequests.Add(rq);
                    db.SaveChanges();

                     EmailAsync(rq.ServiceProvider_Location.Email ,"" , "You have a new Job oppotunity.", "Link:" + CurrentUri + "/main.html&Quote&ServiceRequestID=" + rq.Id);
                }

                return true;
            }
            catch { return false; }
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetRequestServiceInfo(int ServiceRequestID)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var info = db.ServiceRequests.Single(x => x.Id == ServiceRequestID);
                    return JsonConvert.SerializeObject(info, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            catch
            {
                return "";
            }
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetQuoteInfo(int QuoteID)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    var info = db.ServiceRequest_Quote.Single(x => x.Id == QuoteID);
                    return JsonConvert.SerializeObject(info, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }

            } catch (Exception e)
            {
                return "";
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetRequestList()
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    var info = db.ServiceRequests;
                    return JsonConvert.SerializeObject(info, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }

            } catch (Exception e)
            {
                return "";
            }
        }


        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string CreateQuote(int ServiceRequestID, decimal TotalDiscountPercentage, decimal Vat, string itemsJson)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    itemsJson = itemsJson.TrimStart('['); itemsJson = itemsJson.TrimEnd(']');
                    var items = JsonConvert.DeserializeObject<Dictionary<string, List<ServiceRequest_Quote_Line_Item>>>(itemsJson);
                    var qt = new ServiceRequest_Quote();
                    qt.ServiceRequestId = ServiceRequestID;
                    qt.QuoteNo = (db.ServiceRequest_Quote.Count() + 1).ToString();
                    qt.DateOfQuote = DateTime.Now;
                    qt.Total = items.Values.Sum(x => x.Sum(y => y.Qty * y.Amount));
                    qt.Discount = qt.Total * (TotalDiscountPercentage / 100);
                    qt.Subtotal = qt.Total - qt.Discount.Value;
                    qt.Subtotal -= qt.Subtotal * qt.Vat;
                    qt.IsDeleted = false;
                    qt.PaymentGatewayRefNo = Guid.NewGuid().ToString();
                    db.ServiceRequest_Quote.Add(qt);
                    db.SaveChanges();
                    
                    foreach (var line in items)
                    {
                        var qtl = new ServiceRequest_Quote_Line();
                        qtl.Description = line.Key;
                        qtl.IsDeleted = false;
                        qtl.ServiceRequest_Quote = qt;
                        qtl.PaymentGatewayRefNo = Guid.NewGuid().ToString();
                        foreach (var i in line.Value)
                        {
                            i.ServiceRequest_Quote_Line = qtl;
                            db.ServiceRequest_Quote_Line_Item.Add(i);
                        }

                    }
                    db.SaveChanges();

                    var requester = db.ServiceRequests.Single(x => x.Id == ServiceRequestID).AspNetUser;

                    EmailAsync( requester.Email , "", "Your quote is ready for requested service.", "Link: " + CurrentUri + "Main.html/Quote&QuoteID=" + qt.Id);
                }
                return "Quote Created";
            }
            catch (Exception e)
            {
                return "";
            }
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string PaymentSuccessful(string PaymentID)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    var qt = db.ServiceRequest_Quote.Single(x => x.PaymentGatewayRefNo == PaymentID);
                    qt.DateOfPayment = DateTime.Now;
                    foreach(var ql in qt.ServiceRequest_Quote_Line)
                        ql.DateOfPayment = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch { return "Error"; }
            return "Done";
        }
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GetPaymentInfo(int QuoteID)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
                    var qt = db.ServiceRequest_Quote.Single(x => x.Id == QuoteID);
                    var requester = db.ServiceRequests.Single(x => x.Id == qt.ServiceRequestId).AspNetUser;

                    StringBuilder str = new StringBuilder();
                    str.AppendFormat(@"<form id='FullPayment' action='https://sandbox.payfast.co.za/eng/process' method='POST'>
                                       <input type='hidden' name='merchant_id' value='{0}'>
                                       <input type='hidden' name='merchant_key' value='{1}'>
                                       <input type='hidden' name='return_url' value='{2}'>", Properties.Settings.Default.PayfastMerchantID, Properties.Settings.Default.PayfastMerchantKey,
                                                                          CurrentUri + "/main.html&PaymentDone&PaymentID=" + qt.PaymentGatewayRefNo);

                    str.AppendFormat(@"<input type='hidden' name='name_first' value='{0}'>
                                       <input type='hidden' name='name_last' value='{1}'>
                                       <input type='hidden' name='email_address' value='{2}'>
                                       <input type='hidden' name='cell_number' value='{3}'>", requester.FirstName, requester, requester.Surname, requester.Email, requester.PhoneNumber);

                    str.AppendFormat(@"<input type='hidden' name='m_payment_id' value='{0}'>
                                       <input type='hidden' name='amount' value='{1}'>
                                       <input type='hidden' name='item_name' value='{2}'>", qt.PaymentGatewayRefNo, qt.Total, "Quote No: " + qt.QuoteNo);

                    return str.ToString();
                }
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}