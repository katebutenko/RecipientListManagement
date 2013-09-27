using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Web.UI.WebControls;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Modules.EmailCampaign.Messages;
using System.Web;
using System.Globalization;
using Sitecore;

namespace RecipientListManagement.CSVExport.AllEmailCampaigns
{
    public class ExportAction : Sitecore.Web.UI.WebControls.Action
    {
        public override void Execute(ActionContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            context.Owner.Page.Response.Redirect("/sitecore modules/shell/EmailCampaign/Handlers/RecipientListManagement/CsvExportHandler.ashx?action=AllEmailCampaigns&managerroot="+UIFactory.Instance.GetSpeakContext().ManagerRoot.InnerItem.ID.ToString()+"&dbname="+UIFactory.Instance.GetSpeakContext().ManagerRoot.InnerItem.Database.Name);
        }
    }
}
