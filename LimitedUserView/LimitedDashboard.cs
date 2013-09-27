using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipientListManagement.LimitedUserView
{
    public class LimitedDashboard : Sitecore.Modules.EmailCampaign.Speak.Sublayouts.Dashboard
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Sitecore.Context.User.IsAdministrator || UIFactory.Instance.GetSpeakContext().Message != null)
            {
                base.Page_Load(sender, e);
            }
            else
            {
                this.Visible = false;
            }
        }

 
    }
}
