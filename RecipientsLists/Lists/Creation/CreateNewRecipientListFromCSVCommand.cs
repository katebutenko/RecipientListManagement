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

namespace RecipientListManagement.RecipientsLists.Lists.Creation
{
    public class CreateNewRecipientListFromCSVCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Item innerItem;
            if (context.Items.Length == 0)
            {
                ManagerRoot managerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot;
                if (managerRoot == null)
                {
                    return;
                }
                innerItem = managerRoot.InnerItem;
            }
            else
            {
                innerItem = context.Items[0];
            }
            ClientPipelineArgs args = new ClientPipelineArgs();
            args.Parameters["itemID"] = innerItem.ID.ToString();
            if (context.Parameters["type"] != null)
            {
                args.Parameters["type"] = context.Parameters["type"];
            }
            Util.StartClientPipeline(this, "Run", args);

        }

        protected virtual void Run(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                Context.SetActiveSite("shell");
                string var = args.Parameters["itemID"];
                Util.AssertNotNullOrEmpty(var);
                UrlString str2 = new UrlString(UIUtil.GetUri(this.GetWizardUri()));
                foreach (string str3 in args.Parameters.AllKeys)
                {
                    str2.Add(str3, args.Parameters[str3]);
                }
                SheerResponse.ShowModalDialog(str2.ToString(), "540px", "590px", string.Empty, true);
                args.WaitForPostBack();
            }
            else
            {
                string arg = args.Parameters["type"] ?? string.Empty;
                this.HandlePostBack(arg);
            }
        }

        protected virtual string GetWizardUri()
        {
            return "control:RecipientListManagement.ImportUsersWizard";
        }


        protected virtual void HandlePostBack(string arg)
        {
            NotificationManager.Instance.Notify("RefreshRecipientLists", new EventArgs());
        }

    }
}
