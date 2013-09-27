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
using System.Web.UI;
using System.Web;
using Sitecore.Web.UI.WebControls.Extensions;


namespace RecipientListManagement.RecipientsLists.Lists.Actions.AddSitecoreRoles
{
    [Serializable]
    public class AddSitecoreRolesToSelectedListCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            string str = context.Parameters["ids"];
            TargetAudience ta = Factory.GetTargetAudience(str);
            if (ta != null)
            {
                ClientPipelineArgs args = new ClientPipelineArgs();
                args.Parameters.Add("rid", str);
                string methodName = "Run";
                Util.StartClientPipeline(this, methodName, args);
            }
            else
            {               
                NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("Please select a Recipient List."));
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            string var = args.Parameters["rid"];
            Util.AssertNotNull(var);
            if (!args.IsPostBack)
            {
                using (new SecurityDisabler())
                {
                    UrlString urlString = new UrlString("/sitecore/shell/~/xaml/RecipientListManagement.RecipientsLists.Lists.Creation.AddSitecoreRoles.SelectRoles.aspx");
                    new UrlHandle().Add(urlString);
                    foreach (string str in args.Parameters.AllKeys)
                    {
                        urlString.Add(str, args.Parameters[str]);
                    }
                    SheerResponse.ShowModalDialog(urlString.ToString(), "600", "650", string.Empty, true);
                    args.WaitForPostBack();
                    return;
                }
            }
            if (args.HasResult)
            {
                ListString roles = new ListString((args.Result == "-") ? string.Empty : args.Result);

                TargetAudience recipientList = Factory.GetTargetAudience(var);
                if ((recipientList != null) && (recipientList.InnerItem != null))
                {
                    IEnumerable<string> enumerable = null;
                    if (recipientList.ExtraOptInList != null)
                    {
                        enumerable = (recipientList.OptInList == null) ? roles : (from role in roles
                                                                                                      where recipientList.OptInList.Roles.All<Role>(x => x.Name != role) && recipientList.ExtraOptInList.Roles.All<Role>(x => x.Name != role)
                                                                                                      select role).ToList<string>() as IEnumerable<string>;
                    }
                    else
                    {
                        enumerable = (recipientList.OptInList == null) ? roles : (from role in roles
                                                                                                      where recipientList.OptInList.Roles.All<Role>(x => x.Name != role)
                                                                                                      select role).ToList<string>() as IEnumerable<string>;
                    }
                    foreach (string str in enumerable)
                    {
                        recipientList.Source.AddRoleToExtraOptIn(str);
                    }
                    NotificationManager.Instance.Notify("MessageFromCommand", new MessageEventArgs("Recipients were imported to the selected list."));
                    NotificationManager.Instance.Notify("RecipientsChanged");
                    NotificationManager.Instance.Notify("RefreshRecipientLists");   
                }
            }
        }
    }
}
