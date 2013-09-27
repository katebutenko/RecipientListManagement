using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.EmailCampaign.Domain;
using Sitecore.EmailCampaign.Data;
using Sitecore.Security;
using System.Web.Profile;
using System.Configuration;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Modules.EmailCampaign.Core;
using System.Web.Security;
using Sitecore.Jobs;
using Sitecore.IO;
using Sitecore.Modules.EmailCampaign.UI.Controls;
using Sitecore;
using Sitecore.Diagnostics;
using System.IO;
using Sitecore.Modules.EmailCampaign.Core.Extensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.StringExtensions;
using Sitecore.Data.Items;
using Sitecore.Web.UI.WebControls;

namespace RecipientListManagement.LimitedUserView
{
    public class ExportUsersWizard : WizardForm
    {
        protected Listview AllProperties;
        protected Listview AllTargetAudiences;
        protected Checkbox DeleteFile;
        private const string EmailKey = "Email";
        protected Listview ExportedProperties;
        protected Listview ExportedTargetAudiences;
        protected Edit Filename;
        private const string FullnameKey = "Fullname";
        private const string NameKey = "Name";
        protected Literal NumExported;
        private const string PhoneKey = "Phone";
        private List<string> propertyList;

        // Methods
        protected override void ActivePageChanged(string page, string oldPage)
        {
            base.ActivePageChanged(page, oldPage);
            string str = page;
            if (str != null)
            {
                if (!(str == "Exporting"))
                {
                    if (str == "Finish")
                    {
                        base.BackButton.Disabled = true;
                    }
                }
                else
                {
                    base.BackButton.Disabled = true;
                    base.NextButton.Disabled = true;
                }
            }
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            if (page.Equals("TargetAudiences") && newpage.Equals("Exporting"))
            {
                if (this.ExportedTargetAudiences.Items.Length == 0)
                {
                    SheerResponse.Alert(EcmTexts.Localize("There are no recipients to export. Please select at least one source.", new object[0]), new string[0]);
                    return false;
                }
                if (!this.StartExport())
                {
                    return false;
                }
            }
            return base.ActivePageChanging(page, ref newpage);
        }

        private void AddTargetAudienceUsers(List<string> users)
        {
            foreach (ListviewItem item in this.ExportedTargetAudiences.Items)
            {
                TargetAudience targetAudience = Factory.GetTargetAudience(item.Value);
                if (targetAudience != null)
                {
                    foreach (string str in targetAudience.SubscribersNames)
                    {
                        if (!users.Contains(str))
                        {
                            users.Add(str);
                        }
                    }
                }
            }
        }

        public void CheckExport()
        {
            Job currentJob = JobHelper.CurrentJob;
            if (currentJob == null)
            {
                base.Next();
            }
            else if (!currentJob.IsDone)
            {
                SheerResponse.Timer("CheckExport", 300);
            }
            else
            {
                if (currentJob.Status.Result != null)
                {
                    string[] strArray = currentJob.Status.Result.ToString().Split(new char[] { '|' });
                    if (strArray.Length == 2)
                    {
                        this.NumExported.Text = strArray[0];
                        this.Filename.Value = strArray[1];
                        SheerResponse.Refresh(this.NumExported);
                    }
                }
                base.Next();
            }
        }

        protected void ConfirmExit(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                SheerResponse.Confirm(EcmTexts.Localize("You have not downloaded the file containing the exported users.", new object[0]) + "\n\n" + EcmTexts.Localize("Are you sure you want to exit without exporting any users?", new object[0]));
                args.WaitForPostBack();
            }
            else if (args.HasResult && (args.Result == "yes"))
            {
                base.Cancel();
            }
        }

        protected void DownloadFile_Click()
        {
            this.IsDownloaded = true;
            if (!string.IsNullOrEmpty(this.Filename.Value))
            {
                SheerResponse.Download(this.Filename.Value);
            }
        }

        protected override void EndWizard()
        {
            string str = this.Filename.Value;
            if ((!string.IsNullOrEmpty(str) && FileUtil.FileExists(str)) && this.DeleteFile.Checked)
            {
                FileUtil.Delete(str);
            }
            base.EndWizard();
        }

        private void FillPropertyListviews()
        {
            foreach (string str in this.PropertyList)
            {
                ListviewItemMod child = new ListviewItemMod
                {
                    ID = Control.GetUniqueID("I"),
                    Header = str + " ",
                    Value = str
                };
                if (str != "Email")
                {
                    child.Icon = "Applications/32x32/star_grey.png";
                    this.AllProperties.Controls.Add(child);
                }
                else
                {
                    child.Icon = "Applications/32x32/star_green.png";
                    this.ExportedProperties.Controls.Add(child);
                }
            }
        }

        private void FillTargetAudienceListview()
        {
            foreach (TargetAudience audience in this.Root.GetTargetAudiences())
            {
                string owner = audience.InnerItem.Statistics.CreatedBy;
                if (String.IsNullOrEmpty(owner))
                {
                    owner = audience.InnerItem.Statistics.CreatedBy;
                }
                if (CurrentUserIsOwner(owner))
                {
                    ListviewItemMod child = new ListviewItemMod
                    {
                        ID = Control.GetUniqueID("I"),
                        Icon = audience.InnerItem.Appearance.Icon,
                        Header = audience.InnerItem.DisplayName,
                        Value = audience.InnerItem.ID.ToString()
                    };
                    this.AllTargetAudiences.Controls.Add(child);
                }                               
            }
        }

        private bool CurrentUserIsOwner(string owner)
        {
            string currentUserName = Sitecore.Context.User.Name;
            if (currentUserName.ToLowerInvariant() == owner.ToLowerInvariant() || Sitecore.Context.User.IsAdministrator)
            {
                return true;
            }
            return false;
        }
        protected override void OnCancel(object sender, EventArgs formEventArgs)
        {
            string str = this.Filename.Value;
            if (!((string.IsNullOrEmpty(str) || !FileUtil.FileExists(str)) || this.IsDownloaded))
            {
                Context.ClientPage.Start(this, "ConfirmExit", new ClientPipelineArgs());
            }
            else
            {
                base.OnCancel(sender, formEventArgs);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                this.FillPropertyListviews();
                this.FillTargetAudienceListview();
                this.NumExported.Text = "0";
            }
        }


        protected string PerformExport(List<string> properties, List<string> users)
        {
            Assert.ArgumentNotNull(properties, "properties");
            Assert.ArgumentNotNull(users, "users");
            int num = 0;
            string path = "/temp/Users_" + DateTime.Now.ToString("s").Replace(":", string.Empty) + ".csv";
            StreamWriter writer = new StreamWriter(FileUtil.MapPath(path));
            try
            {
                string str3 = string.Empty;
                foreach (string str4 in properties)
                {
                    str3 = str3 + "\"" + str4 + "\",";
                }
                writer.WriteLine(str3.TrimEnd(new char[] { ',' }));
                str3 = string.Empty;
                foreach (string str5 in users)
                {
                    Contact contactFromName = Factory.GetContactFromName(str5);
                    if (contactFromName != null)
                    {
                        foreach (string str4 in properties)
                        {
                            str3 = str3 + "\"" + ("Email".Equals(str4) ? contactFromName.Profile.Email : contactFromName.Profile[str4]) + "\",";
                        }
                        writer.WriteLine(str3.TrimEnd(new char[] { ',' }));
                        str3 = string.Empty;
                        num++;
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.LogError(exception);
            }
            finally
            {
                writer.Close();
            }
            return (num + "|" + path);
        }

        protected void PropertiesFromExport_Click()
        {
            foreach (ListviewItem item in this.ExportedProperties.SelectedItems)
            {
                if ("Email".Equals(item.Value))
                {
                    item.Selected = false;
                    break;
                }
            }
            this.ExportedProperties.TransferItems(this.AllProperties, "Applications/32x32/star_grey.png");
        }

        protected void PropertiesToExport_Click()
        {
            this.AllProperties.TransferItems(this.ExportedProperties, "Applications/32x32/star_green.png");
        }
       
        protected bool StartExport()
        {
            List<string> list = new List<string>();
            foreach (ListviewItem item in this.ExportedProperties.Items)
            {
                list.Add(item.Value);
            }
            List<string> users = new List<string>();
            try
            {
                this.AddTargetAudienceUsers(users);
            }
            catch (Exception exception)
            {
                SheerResponse.Alert(exception.Message, new string[0]);
                return false;
            }
            JobHelper.StartJob("Export Users", "PerformExport", this, new object[] { list, users });
            this.CheckExport();
            return true;
        }

        protected void TargetAudienceFromExport_Click()
        {
            this.ExportedTargetAudiences.TransferItems(this.AllTargetAudiences, string.Empty);
        }

        protected void TargetAudienceToExport_Click()
        {
            this.AllTargetAudiences.TransferItems(this.ExportedTargetAudiences, "Applications/16x16/check.png");
        }

        // Properties
        protected bool IsDownloaded
        {
            get
            {
                object obj2 = base.ServerProperties["IsDownloaded"];
                return ((obj2 != null) ? ((bool)obj2) : false);
            }
            set
            {
                base.ServerProperties["IsDownloaded"] = value;
            }
        }

        protected List<string> PropertyList
        {
            get
            {
                if (this.propertyList == null)
                {
                    this.propertyList = new List<string>(this.Root.GetDefaultUserProperties().Keys);
                    this.propertyList.Sort();
                }
                return this.propertyList;
            }
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
