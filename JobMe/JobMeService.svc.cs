using AngleSharp.Media.Dom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.EnterpriseServices;
using System.Linq;
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

namespace JobMe
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
        IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    public class JobMeService
    {
        private static string CurrentUri = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Replace(System.Web.HttpContext.Current.Request.Url.PathAndQuery, "");
        private string Email(List<string> Emails, List<string> Bcc, string subject, string content, List<Attachment> attachments = null, List<string> ImagePaths = null)
        {
            try
            {
                if (Bcc == null)
                    Bcc = new List<string>();
                if (attachments == null)
                    attachments = new List<Attachment>();
                if (ImagePaths == null)
                    ImagePaths = new List<string>();
                if (Emails.Count == 0)
                {
                    return "Unable to send email";
                }
                var ToAdd = new List<string>();
                var ToRemove = new List<string>();
                foreach (var em in Emails)
                    if (em.IndexOf(';') >= 0)
                    {
                        ToAdd.AddRange(em.Split(';').ToList());
                        ToRemove.Add(em);
                    }
                foreach (var rm in ToRemove)
                    Emails.Remove(rm);
                foreach (var ad in ToAdd)
                    Emails.Add(ad);
                Emails.RemoveAll(x => (x == "") || (x == ";"));

                var fromAddress = new MailAddress(Properties.Settings.Default.EmailUsername);
                string fromPassword = Properties.Settings.Default.EmailPassword;
                string body = content;
                var p = new PreMailer.Net.PreMailer(body);
                body = p.MoveCssInline().Html;

                var smtp = new SmtpClient
                {
                    Host = Properties.Settings.Default.EmailHostServer,
                };
                if (Properties.Settings.Default.IsSSL)
                {
                    smtp.EnableSsl = true;
                }
                if (Properties.Settings.Default.EmailPassword != "")
                {
                    smtp.Port = Properties.Settings.Default.EmailSMTPPort;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress.Address, fromPassword);
                }

                using (var message = new MailMessage(fromAddress, new MailAddress(Emails.First().Trim()))
                {
                    Subject = subject,
                    Body = body
                })
                {
                    message.IsBodyHtml = true;
                    if (ImagePaths.Count() > 0)
                    {
                        int i = 0;
                        List<LinkedResource> inlines = new List<LinkedResource>();
                        foreach (var img in ImagePaths)
                        {
                            var inline = new LinkedResource(img, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                            inline.ContentId = Guid.NewGuid().ToString();
                            inline.ContentType = new System.Net.Mime.ContentType("image/jpg");
                            message.Body = message.Body.Replace("{ImageSource" + i++ + "}", "cid:" + inline.ContentId);
                            inlines.Add(inline);
                        }
                        AlternateView alternateView = AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Html);
                        foreach (var inline in inlines)
                            alternateView.LinkedResources.Add(inline);
                        message.AlternateViews.Add(alternateView);
                    }
                    if (Emails.Count() > 1)
                        foreach (var mail in Emails.Skip(1))
                            try
                            {
                                if ((mail.Trim() == "") || (mail == ";")) continue;
                                message.CC.Add(new MailAddress(mail.Trim()));
                            }
                            catch (Exception e) { }
                    foreach (var mail in Bcc)
                        try
                        {
                            if ((mail.Trim() == "") || (mail == ";")) continue;
                            message.Bcc.Add(new MailAddress(mail.Trim()));
                        }
                        catch (Exception e) { }

                    foreach (var att in attachments)
                        message.Attachments.Add(att);
                    smtp.Send(message);
                    smtp.Dispose();
                }
            }
            catch (Exception e)
            {

                return "Unable to send email (Subject: " + subject + ").\n\n" + e.Message + "\n" + e.InnerException;
            }
            return "Email Sent";
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

                // Split the key/values out for each cookie.
                string[] cookieKeyValue = cookie.Split('=');

                // Compare the keys
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

            // Note: The result may still be empty if there wasn't a value set for the cookie.
            // e.g. 'key=' rather than 'key=123'
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
                //user = (UserInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(GetCookieValue("User"));
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
                // var temp=db.ServiceTypes.ToDictionary(x => x.Id, x => x.SubServiceTypes);
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

                    Email(new List<string>() { rq.ServiceProvider_Location.Email }, null, "You have a new Job oppotunity.", "Link:" + CurrentUri + "/main.html&CreateQuote&RequestID=" + rq.Id);
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
        public string PayQuote(int ServiceRequestID, decimal TotalDiscountPercentage, decimal Vat, string itemsJson)
        {
            //PayFast.
            return "";

        }
            [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string CreateQoute(int ServiceRequestID, decimal TotalDiscountPercentage,decimal Vat,string itemsJson)
        {
            try
            {
                using (JobMeEntities db = new JobMeEntities())
                {
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
                    db.ServiceRequest_Quote.Add(qt);
                    db.SaveChanges();

                    foreach (var line in items)
                    {
                        var qtl = new ServiceRequest_Quote_Line();
                        qtl.Description = line.Key;
                        qtl.IsDeleted = false;
                        qtl.ServiceRequest_Quote = qt;
                        foreach (var i in line.Value)
                        {
                            i.ServiceRequest_Quote_Line = qtl;
                            db.ServiceRequest_Quote_Line_Item.Add(i);
                        }
                        
                    }
                    var requester = db.ServiceRequests.Single(x => x.Id == ServiceRequestID).AspNetUser;

                    Email(new List<string>() { requester.Email }, null, "Your quote is ready for requested service.", "Link: " + CurrentUri + "/QuotePayment");
                }
                return "Quote Created";
            }
            catch
            {
                return "";
            }
        }

    }
}