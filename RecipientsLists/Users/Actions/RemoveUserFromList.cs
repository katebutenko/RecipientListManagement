using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecipientListManagement.RecipientsLists.Lists;
using Sitecore.Web.UI.WebControls;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;

namespace RecipientListManagement.RecipientsLists.Users
{
    public class RemoveUserFromList: Sitecore.Web.UI.WebControls.Action
    {
        public override void Execute(ActionContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Util.AssertNotNull(context.Owner);
            
            string listId = ((RecipientListsObjectDetailList)UIUtil.FindControlByType(typeof(RecipientListsObjectDetailList))).LastSelectedRow;
            string userId = "";
            if (context.Model != null)
            {
                userId = context.Model.ToString();
            }
            if (!String.IsNullOrEmpty(listId) && !String.IsNullOrEmpty(userId))
            {
                TargetAudience ta = Factory.GetTargetAudience(listId);

                if (ta != null)
                {
                    string userName = userId.Replace("__at__", "@").Replace("__dot__", ".").Replace("__slash__", "\\");
                    Contact contact = Factory.GetContactFromName(userName);
                    contact.Unsubscribe(ta);
                    NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("Recipient was removed from the recipient list."));
                    NotificationManager.Instance.Notify("RecipientsChanged");
                    //ensure, try/catch
                }
            }

        }
    }
}
