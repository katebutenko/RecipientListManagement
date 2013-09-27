using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.WebControls;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Text;
using Sitecore.Links;
using System.Web.UI;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore;
using System.Web;
using Sitecore.Shell.Framework.Commands;
using System.Collections.Specialized;
using Sitecore.SecurityModel;
using Sitecore.Web;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Continuations;
using Sitecore.Modules.EmailCampaign.Commands;
using System.ComponentModel;
using Sitecore.ComponentModel;
using Sitecore.Web.UI.WebControls.Extensions;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Configuration;

namespace RecipientListManagement.RecipientsLists.Lists.Actions
{
    public class DeleteSelectedList : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {                       
            Assert.ArgumentNotNull(context, "context");
            string str = context.Parameters["ids"];
            if (!String.IsNullOrEmpty(str))
            {
                TargetAudience ta = Sitecore.Modules.EmailCampaign.Factory.GetTargetAudience(str);
                if (ta != null)
                {
                    Item item = ta.InnerItem;
                    if (CanBeDeleted(ta))
                    {
                        ClientPipelineArgs args = new ClientPipelineArgs();
                        args.Parameters["rid"] = str;
                        args.Parameters["rname"] = ta.Name;
                        Util.StartClientPipeline(this, "Run", args);
                    }
                    else
                    {
                        NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("This Recipient List is used in messages and cannot be deleted."));
                        return;
                    }
                }
            }
            else
            {
                NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("Please select a Recipient List."));
            }
        }

        private bool CanBeDeleted(TargetAudience ta)
        {

            Item listItem = ta.InnerItem;
            ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(listItem);
            if (referrers.Count() > 0)
            {
                return false;
            }
            return true;

        }

        protected void Run(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                string rname = args.Parameters["rname"];
                SheerResponse.Confirm(String.Format("Are you sure you want to delete list: {0}?", rname));
                args.WaitForPostBack();
            }
            else if (args.Result != "cancel")
            {
                if (args.HasResult && (args.Result != "cancel"))
                {
                    if ((args.Result == "yes") || (args.Result == "no"))
                    {
                        if (args.Result == "yes")
                        {
                            string rid = args.Parameters["rid"];
                            DeleteList(rid);
                        }
                        
                    }                    
                }
            }
        }

        private void DeleteList(string rid)
        {
            TargetAudience ta = Sitecore.Modules.EmailCampaign.Factory.GetTargetAudience(rid);
            Item item = ta.InnerItem;
            using (new SecurityDisabler())
            {
                item.Delete();
            }
            NotificationManager.Instance.Notify("RefreshRecipientLists");
            NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("The Recipient List has been deleted."));
            return;
        }
    }
}
