using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Controls;
using ComponentArt.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web;
using Sitecore;
using Sitecore.Web.UI.Sheer;
using Sitecore.Shell.Framework.Commands.UserManager;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.XamlSharp.Xaml;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Extensions;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Modules.EmailCampaign;

namespace RecipientListManagement.RecipientsLists.Lists.Creation.AddSitecoreRoles
{
    public class SelectRolesPage : DialogPage
    {
        // Fields
        protected Grid Roles;
        protected HtmlSelect SelectedRoles;
        protected HtmlInputHidden SelectedValues;

        // Methods
        protected override void OK_Click()
        {
            string str = string.IsNullOrEmpty(this.SelectedValues.Value) ? "-" : this.SelectedValues.Value;
            str = HttpUtility.UrlDecode(StringUtil.RemovePostfix('|', str));
            if (ValidationHelper.ValidateRoleForCommaWithMessage(str))
            {
                SheerResponse.SetDialogValue(str);
                base.OK_Click();
            }
        }
        protected TargetAudience RecipientList
        {
            get
            {
                string var = Context.Request.QueryString["rid"];
                if (var == null) return null;
                TargetAudience ta = Factory.GetTargetAudience(var);
                Util.AssertNotNull(ta);
                return ta;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            SheerResponse.RegisterTranslation("Please select a role", "Select a role.");
            SheerResponse.RegisterTranslation("Selected roles are already in the Recipient List", "Selected roles are already in the Recipient List");

            IEnumerable<Role> managedRoles = Sitecore.Context.User.Delegation.GetManagedRoles(false);
            ComponentArtGridHandler<Role>.Manage(this.Roles, new GridSource<Role>(managedRoles), !XamlControl.AjaxScriptManager.IsEvent);
            if (!XamlControl.AjaxScriptManager.IsEvent)
            {
                if (RecipientList != null)
                {
                    string list = RecipientList.InnerItem["Opt-in Role"] + "|" + RecipientList.InnerItem["Extra Opt-in Roles"];

                    foreach (string str2 in new ListString(list))
                    {
                        if (!String.IsNullOrEmpty(str2))
                        {
                            if (System.Web.Security.Roles.RoleExists(str2))
                            {
                                this.SelectedRoles.Items.Add(str2);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(list))
                    {
                        this.SelectedValues.Value = list;
                    }
                }
                this.Roles.ClientEvents.ItemDoubleClick = new ClientEvent("onDoubleClick");
                this.Roles.LocalizeGrid();
            }
        }
    }


}
