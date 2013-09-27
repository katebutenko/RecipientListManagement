using Sitecore.Data;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RecipientListManagement.LimitedUserView
{
    public class MessageLinkColumn : ColumnTemplate
    {
        // Methods
        public void BindData(object sender, EventArgs e)
        {
            HyperLink link = (HyperLink)sender;
            ModelContainer namingContainer = (ModelContainer)link.NamingContainer;
            link.Text = namingContainer.Model.ToString();
            IMessageBase dataItem = namingContainer.DataItem as IMessageBase;
            if (dataItem != null && messageSentByContextUser(dataItem.ID))
            {
                link.NavigateUrl = TaskPageUtil.GetTaskPage(dataItem.ID);
            }
        }

        protected override void DoRender(HtmlTextWriter output)
        {
        }

        public override void InstantiateIn(Control container)
        {
            HyperLink child = new HyperLink();
            child.DataBinding += new EventHandler(this.BindData);
            container.Controls.Add(child);
        }

        private bool messageSentByContextUser(string messageID)
        {
            MessageItem message = Factory.GetMessage(messageID);
            string currentUserName = Sitecore.Context.User.Name;
            string dispatcherUserName = message.InnerItem.Statistics.CreatedBy;

            if (currentUserName.ToLowerInvariant() == dispatcherUserName.ToLowerInvariant() || Sitecore.Context.User.IsAdministrator)
            {
                return true;
            }
            return false;
        }
    }


}
