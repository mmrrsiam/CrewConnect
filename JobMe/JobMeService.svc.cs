using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
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
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";

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
                var r=JsonConvert.SerializeObject(temp, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return r;
            }
        }
    }
}
