using Sitecore;
using Sitecore.Data.Items;
using Sitecore.IO;
using Sitecore.Jobs;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Extensions;
using Sitecore.Modules.EmailCampaign.UI.Controls;
using Sitecore.Modules.EmailCampaign.UI.HtmlControls;
using Sitecore.Resources;
using Sitecore.Security.Domains;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.SecurityModel;
using Sitecore.Diagnostics;
using Sitecore.Data;
using RecipientListManagement.CSVExport;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;

namespace RecipientListManagement.RecipientsLists.Lists.Creation
{
    public class CreateFromSitecoreRolesWizard : WizardForm
    {
        protected Edit RoleList;
        protected Edit RecipientListName;

        // Methods
        protected override void ActivePageChanged(string page, string oldPage)
        {
            base.ActivePageChanged(page, oldPage);
            string str = page;
            if (str != null)
            {               
                    if (str == "Finish")
                    {
                        base.BackButton.Disabled = true;
                    }                             
            }
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            string str = page;
            if (str != null)
            {
                if (str == "SelectName")
                {
                    if (!this.SelectNameCompleted())
                    {
                        return false;
                    }
                    else
                    {
                        //show SelectRoles Dialog
                        if (string.IsNullOrEmpty(this.RoleList.Value))
                        {
                            SelectRoles_Click();
                        }                       
                    }
                }
            }
            if ((str == "SelectRoles") && (!this.SelectRolesCompleted()))
            {                
                return false;
            }            
            return base.ActivePageChanging(page, ref newpage);
        }

        private bool SelectRolesCompleted()
        {            
            //here goes actual import

            List<string> roles = new List<string>();
            try
            {
                this.AddAdvancedRoles(roles);
            }
            catch (Exception exception)
            {
                SheerResponse.Alert(exception.Message, new string[0]);
                return false;
            }
            TargetAudience ta = null;
            Item listsContainerRoot = Root.GetRecipientListContainerItem();
            if (roles.Count > 0)
            {
                using (new SecurityDisabler())
                {
                    string rolename = roles[0];                                      
                    if (listsContainerRoot != null)
                    {
                        try
                        {
                            ta = TargetAudienceSource.Create(this.RecipientListName.Value, listsContainerRoot, rolename, null);
                            roles.RemoveAt(0);
                        }
                        catch (Exception e)
                        {
                            SheerResponse.Alert(EcmTexts.Localize("Error appeared during creating a new Recipient List.", new object[0]), new string[0]);
                            Log.Error("Error appeared during creating a new Recipient List.", e, this);
                        }                        
                    }
                }
            }
            else
            {
                using (new SecurityDisabler())
                {
                    if (listsContainerRoot != null)
                    {
                        try
                        {
                            ta = TargetAudienceSource.Create(this.RecipientListName.Value, listsContainerRoot, null, null);
                        }
                        catch (Exception e)
                        {
                            SheerResponse.Alert(EcmTexts.Localize("Error appeared during creating a new Recipient List.", new object[0]), new string[0]);
                            Log.Error("Error appeared during creating a new Recipient List.", e, this);
                        }
                    }
                }
            }

            if (ta == null)
            {
                SheerResponse.Alert(EcmTexts.Localize("Recipient List could not be created.", new object[0]), new string[0]);
                return false;
            }

            if ((ta != null) && (ta.InnerItem != null))
            {
                try
                {
                    foreach (string str in roles)
                    {
                        ta.Source.AddRoleToExtraOptIn(str);
                    }
                }
                catch (Exception e)
                {
                    SheerResponse.Alert(EcmTexts.Localize("Error appeared during adding extra roles to a new Recipient List.", new object[0]), new string[0]);
                    Log.Error("Error appeared during adding extra roles to a new Recipient List.", e, this);
                }    

            }
            return true;
        }
        private bool SelectNameCompleted()
        {
            if (string.IsNullOrEmpty(this.RecipientListName.Value))
            {
                SheerResponse.Alert(EcmTexts.Localize("Please enter a valid name.", new object[0]), new string[0]);
                return false;
            }
            List<TargetAudience> taList = Root.GetTargetAudiences();
            List<string> taNames = new List<string>();
            foreach (TargetAudience ta in taList)
            {
                taNames.Add(ta.Name);
            }
            if (taNames.Contains(this.RecipientListName.Value))
            {
                SheerResponse.Alert(EcmTexts.Localize("The '{0}' list already exists, please choose another name.", new object[] { this.RecipientListName.Value }), new string[0]);
                return false;
            }
            return true;
        }

        private void AddAdvancedRoles(List<string> roles)
        {          
                foreach (string str in this.RoleList.Value.Split(new char[] { '|' }))
                {
                    if (!((str.Length <= 0) || roles.Contains(str)))
                    {
                        roles.Add(str);
                    }
                }
        }      


        private string FormatDefaultText(string text)
        {
            return HttpUtility.HtmlEncode("<" + text + ">");
        }
        protected void SelectRoles(ClientPipelineArgs args)
        {
            Edit edit = args.CustomData["RoleList"] as Edit;
            if (edit != null)
            {
                if (!args.IsPostBack)
                {
                    UrlString urlString = new UrlString("/sitecore/shell/~/xaml/RecipientListManagement.RecipientsLists.Lists.Creation.AddSitecoreRoles.SelectRoles.aspx");
                    UrlHandle handle = new UrlHandle();
                    handle["roles"] = edit.Value;
                    handle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), "600", "650", string.Empty, true);
                    args.WaitForPostBack();
                }
                else if (args.HasResult)
                {
                    string[] array = args.Result.TrimStart(new char[] { '-' }).Split(new char[] { '|' });
                    Array.Sort<string>(array);
                    string str2 = string.Empty;
                    edit.Value = string.Empty;
                    foreach (string str3 in array)
                    {
                        str2 = str2 + "<div>" + str3.Replace(@"\", "&#92;") + "</div>";
                        edit.Value = edit.Value + str3 + "|";
                    }
                    edit.Value = edit.Value.TrimEnd(new char[] { '|' });
                    SheerResponse.Eval("$('RolesBox').innerHTML = '{0}'".FormatWith(new object[] { str2 }));
                }
            }
        }

        protected void SelectRoles_Click()
        {
            ClientPipelineArgs args = new ClientPipelineArgs();
            args.CustomData["RoleList"] = this.RoleList;
            Context.ClientPage.Start(this, "SelectRoles", args);
        }

        protected ManagerRoot Root
        {
            get
            {
                string var = Context.ClientPage.Request.QueryString["itemID"];
                Util.AssertNotNull(var);
                Item contentDbItem = ItemUtilExt.GetContentDbItem(var);
                Util.AssertNotNull(contentDbItem);
                ManagerRoot managerRootFromChildItem = Factory.GetManagerRootFromChildItem(contentDbItem);
                Util.AssertNotNull(managerRootFromChildItem);
                return managerRootFromChildItem;
            }
        }
       
    }


}
