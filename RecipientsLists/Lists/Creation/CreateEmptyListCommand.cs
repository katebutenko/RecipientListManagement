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

namespace RecipientListManagement.RecipientsLists.Lists.Creation
{
    public class CreateEmptyListCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {          
            ClientPipelineArgs args = new ClientPipelineArgs();            
            Util.StartClientPipeline(this, "Run", args);
        }

        private Page GetCurrentPage()
        {
            return (HttpContext.Current.Handler as Page);
        }

        protected void Run(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                SheerResponse.CheckModified(true);
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
                            this.SaveChanges();
                        }
                        SheerResponse.Input(EcmTexts.Localize("Enter the name of the new Recipient List:", new object[0]), "New Recipient List", Settings.ItemNameValidation, EcmTexts.Localize("'{0}' is not a valid name.", new object[] { "$Input" }), 100);
                        args.WaitForPostBack();
                    }
                    else
                    {
                        if (CreateList(args.Result))
                        {
                            NotificationManager.Instance.Notify("RefreshRecipientLists", new EventArgs());
                        }
                        else
                        {
                            SheerResponse.Alert(EcmTexts.Localize("The '{0}' list already exists, please choose another name.", new object[] { args.Result }), new string[0]);
                        }
                    }
                }
            }
        }

        private bool CreateList(string nameParameter)
        {
            nameParameter = nameParameter.Trim();
            if (!String.IsNullOrEmpty(nameParameter))
            {
                using (new SecurityDisabler())
                {
                    ManagerRoot root = UIFactory.Instance.GetSpeakContext().ManagerRoot;
                    Util.AssertNotNull(root);

                    List<TargetAudience> taList = root.GetTargetAudiences();
                    List<string> taNames = new List<string>();
                    foreach (TargetAudience ta in taList)
                    {
                        taNames.Add(ta.Name);
                    }
                    if (taNames.Contains(nameParameter))
                    {                        
                        return false;
                    }

                    Item listsContainerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot.GetRecipientListContainerItem();
                    if (listsContainerRoot != null)
                    {
                        TargetAudience ta = TargetAudienceSource.Create(nameParameter, listsContainerRoot, null, null);
                        return true;
                    }
                }
            }
            return false;
        }

        private void SaveChanges()
        {
            Page currentPage = this.GetCurrentPage();
            if (currentPage != null)
            {
                IEnumerable<IChangeTracking> enumerable = currentPage.Controls.Flatten<IChangeTracking>();
                if (currentPage.Controls.Flatten<IValidateChangeTracking>().All<IValidateChangeTracking>(control => control.Validate(null)))
                {
                    foreach (IChangeTracking tracking in enumerable)
                    {
                        if (tracking.IsChanged)
                        {
                            tracking.AcceptChanges();
                        }
                    }
                }
            }
        }
    }

}
