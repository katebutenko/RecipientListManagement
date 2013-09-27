using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Web.UI.WebControls;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Web.UI.Sheer;
using Sitecore.SecurityModel;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Security.Accounts;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Extensions;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System.Web;

namespace RecipientListManagement.RecipientsLists.Users.Actions
{
    [Serializable]
    public class EditSelectedUser : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            string userId = context.Parameters["ids"];
            if (userId != null)
            {
                string userName = userId.Replace("__at__", "@").Replace("__dot__", ".").Replace("__slash__", "\\").Replace("}", "").Replace("{", "");

                using (new SecurityDisabler())
                {
                    if (Sitecore.Security.Accounts.User.Exists(userName))
                    {
                        ClientPipelineArgs args = new ClientPipelineArgs();
                        args.Parameters.Add("uid", HttpUtility.UrlEncode(userName));
                        string methodName = "Run";
                        Util.StartClientPipeline(this, methodName, args);
                    }
                }
            }
        }

        protected virtual void Run(ClientPipelineArgs args)
        {
            string var = args.Parameters["uid"];
            Util.AssertNotNull(var);
            if (!args.IsPostBack)
            {
                using (new SecurityDisabler())
                {
                    UrlString urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Security.EditUser.aspx");
                    urlString.Query = "us=" + var;
                    new UrlHandle().Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), "530", "600", string.Empty, true);
                    NotificationManager.Instance.Notify("RecipientsChanged");
                    args.WaitForPostBack();
                    return;
                }
            }
        }

    }
}
