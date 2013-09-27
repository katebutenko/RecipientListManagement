using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Messages;
using RecipientListManagement.LimitedUserView;
using Sitecore.Modules.EmailCampaign.Core;

namespace RecipientListManagement.CSVExport
{
    public class CSVExportHandler : IHttpHandler
    {   
        public void ProcessRequest(HttpContext httpContext)
        {
            Assert.ArgumentNotNull(httpContext, "httpContext");
            string action = httpContext.Request.Params["action"];

            if (action == "AllEmailCampaigns")
            {

                RecentlyDispatchedRepository emailInfoRepository = new RecentlyDispatchedRepository(httpContext.Request.Params["managerroot"], httpContext.Request.Params["dbname"]);
                IEnumerable<DispatchedMessageInfo> dataItems = emailInfoRepository.GetAllRecentlyDispatched("fast://{messages}//*[@@templatename='Message Folder' or @@templatename='Folder']/*[@@templatename!='Folder' and @@templatename!='Message Folder' and (@State='Sent' or @State='Sending')]");

                string detailListId = "{5A5F77F9-FC7F-45D5-B6EB-89804A70C03A}"; // an ID of the item to format csv

                string export = CsvExport.ExportDetailsListToCsv<DispatchedMessageInfo>(dataItems, detailListId);

                string filename = "AllEmailCampaigns_" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
                try
                {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ContentType = "text/csv";
                    HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                    HttpContext.Current.Response.Write(export);
                }
                catch (Exception exception)
                {
                    Log.Error(exception.Message, exception, this);
                }
                return;
            }
                        
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
 

 

    }
}
