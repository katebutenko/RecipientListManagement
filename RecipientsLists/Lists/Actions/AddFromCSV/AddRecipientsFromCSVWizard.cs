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

namespace RecipientListManagement.RecipientsLists.Lists.Creation.AddFromCSV
{
    public class AddRecipientsFromCSVWizard : WizardForm
    {
        protected Checkbox AdvancedOptions;
        protected Listview AlltargetAudiences;
        private const string DeleteImgSrc = "Applications/16x16/delete2.png";
        private const string DelSectionPrefix = "DelSection";
        protected TraceableCombobox DomainCombobox;
        protected Edit DomainInput;
        protected GridPanel DomainPanel;
        private const int DomainThreshold = 15;
        private List<string> fieldList;
        private const string FieldPrefix = "Field";
        protected Scrollbox FieldsSection;
        protected Edit Filename;
        private Dictionary<string, string> mappedProperties;
        protected Sitecore.Web.UI.HtmlControls.Literal NumBroken;
        protected Sitecore.Web.UI.HtmlControls.Literal NumEmailExists;
        protected Sitecore.Web.UI.HtmlControls.Literal NumImported;
        protected Sitecore.Web.UI.HtmlControls.Literal NumNoEmail;
        protected Listview OptOutOf;
        protected Radiobutton OverwriteProperties;
        private System.Web.UI.Control parentControl;
        private List<string> propertyList;
        private const string PropertyPrefix = "Property";
        protected Border Results;
        protected Edit RoleList;
        protected Border RolesBox;
        protected GridPanel RolesPanel;
        protected Radiobutton SkipUser;
        protected Radiobutton StoreProperties;
        protected Listview SubscribeTo;
        protected ID targetAudienceId;

        protected override void ActivePageChanged(string page, string oldPage)
        {
            base.ActivePageChanged(page, oldPage);
            string str = page;
            if (str != null)
            {
                if (!(str == "Importing"))
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

        private void AddDelSection(string idPostfix)
        {
            ImageBuilder builder = new ImageBuilder
            {
                Src = "Applications/16x16/delete2.png",
                Width = 0x10,
                Height = 0x10,
                Style = "cursor: pointer"
            };
            Sitecore.Web.UI.HtmlControls.Literal child = new Sitecore.Web.UI.HtmlControls.Literal
            {
                Text = builder.ToString()
            };
            Border border = new Border
            {
                ID = "DelSection" + idPostfix,
                Click = "DelSection_Click"
            };
            border.Controls.Add(child);
            this.ParentControl.Controls.Add(border);
        }

        private void AddFieldCombobox(string idPostfix, string defValue)
        {
            TraceableCombobox child = new TraceableCombobox
            {
                ID = "Field" + idPostfix,
                Width = new Unit(100.0, UnitType.Percentage),
                SelectOnly = true
            };
            child.SetList(this.FieldList);
            child.Change = "FieldChanged";
            if (!string.IsNullOrEmpty(defValue))
            {
                foreach (string str in this.FieldList)
                {
                    if (defValue.Equals(str, StringComparison.OrdinalIgnoreCase))
                    {
                        child.Value = str;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(child.Value))
            {
                child.Value = this.FormatDefaultText(EcmTexts.Localize("select to add field", new object[0]));
            }
            this.ParentControl.Controls.Add(child);
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            string str = page;
            if (str != null)
            {           
                if (!(str == "SelectFile"))
                {
                    if (str == "Fields")
                    {
                        if (newpage.Equals("SecurityDomain") && !this.FieldsCompleted())
                        {
                            return false;
                        }
                    }
                    else if (str == "SecurityDomain")
                    {
                        if (!this.SecurityDomainCompleted())
                        {
                            return false;
                        }
                            newpage = "Importing";
                    }
                }
                else if (newpage.Equals("Fields") && !this.SelectFileCompleted())
                {
                    return false;
                }
            }
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(newpage, "newpage");
            return true;

        }

   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                Context.ClientPage.RegisterArrayDeclaration("dictionary", "'" + EcmTexts.Localize("is not a CSV file.", new object[0]) + "'");
                this.FillDomains();

                this.NumImported.Text = "0";
                this.NumEmailExists.Text = "0";
                this.NumNoEmail.Text = "0";
                this.NumBroken.Text = "0";
            }
            else
            {
                foreach (object obj2 in this.ParentControl.Controls)
                {
                    if (obj2 is TraceableCombobox)
                    {
                        (obj2 as TraceableCombobox).SelectOnly = true;
                    }
                }
            }
        }

        private void FillDomains()
        {
            List<Sitecore.Security.Domains.Domain> list = new List<Sitecore.Security.Domains.Domain>(Context.User.Delegation.GetManagedDomains());
            if (list.Count <= 15)
            {
                this.DomainCombobox.Visible = true;
                this.DomainCombobox.SetList(list.ConvertAll<string>(d => d.Name.ToString()));
            }
            else
            {
                this.DomainPanel.Visible = true;
            }
        }
        protected virtual bool SecurityDomainCompleted()
        {
            if (string.IsNullOrEmpty(this.DomainInput.Value))
            {
                SheerResponse.Alert(EcmTexts.Localize("You need to select a domain first.", new object[0]), new string[0]);
                return false;
            }
            ImportOptions options = new ImportOptions
            {
                Filename = FileUtil.MapPath("/temp/" + FileUtil.GetFileName(this.Filename.Value)),
                MappedProperties = this.MappedProperties,
                Root = this.Root,
                DomainName = this.DomainInput.Value
            };
            string str = Context.ClientPage.ClientRequest.Form[this.SkipUser.Name];
            if (string.IsNullOrEmpty(str) || str.Equals(this.SkipUser.Value))
            {
                options.ConflictOption = ImportOptions.ConflictOptions.SkipUser;
            }
            else
            {
                options.ConflictOption = str.Equals(this.OverwriteProperties.Value) ? ImportOptions.ConflictOptions.OverwriteProperties : ImportOptions.ConflictOptions.KeepProperties;
            }
            List<string> roles = new List<string>();
            try
            {
                List<Sitecore.Security.Accounts.Role> roleList = RecipientList.OptInList.Roles;
                foreach (Sitecore.Security.Accounts.Role roleToAssign in roleList)
                {
                    roles.Add(roleToAssign.Name);
                }
            }
            catch (Exception exception)
            {
                SheerResponse.Alert(exception.Message, new string[0]);
                return false;
            }
            
            options.Roles = roles.ToArray();
            JobHelper.StartJob("Import Users", "PerformImport", CoreFactory.Instance.GetRecipientImporter(), new object[] { options });
            this.CheckImport();
            return true;
        }


        private void AddOptInRoles(List<string> roles)
        {
            foreach (ListviewItem item in this.SubscribeTo.Items)
            {
                TargetAudience targetAudience = Factory.GetTargetAudience(item.Value);
                if (targetAudience != null)
                {
                    TargetAudienceSource targetAudienceSource = CoreFactory.Instance.GetTargetAudienceSource(targetAudience);
                    string domain = this.DomainCombobox.Value;
                    string str2 = targetAudienceSource.CreateRoleToExtraOptIn(domain);
                    if (!string.IsNullOrEmpty(str2))
                    {
                        roles.Add(str2);
                    }
                }
            }
        }

        private void AddOptOutRoles(List<string> roles)
        {
            foreach (ListviewItem item in this.OptOutOf.Items)
            {
                TargetAudience targetAudience = Factory.GetTargetAudience(item.Value);
                if (targetAudience != null)
                {
                    TargetAudienceSource targetAudienceSource = CoreFactory.Instance.GetTargetAudienceSource(targetAudience);
                    string domain = this.DomainCombobox.Value;
                    string str2 = targetAudienceSource.CreateRoleToExtraOptOut(domain);
                    if (!string.IsNullOrEmpty(str2))
                    {
                        roles.Add(str2);
                    }
                }
            }
        }

        private void AddPropertyCombobox(string idPostfix, string defValue)
        {
            TraceableCombobox child = new TraceableCombobox
            {
                ID = "Property" + idPostfix,
                Width = new Unit(100.0, UnitType.Percentage),
                SelectOnly = true
            };
            child.SetList(this.PropertyList);
            if (string.IsNullOrEmpty(defValue))
            {
                child.Value = this.FormatDefaultText(EcmTexts.Localize("Select property", new object[0]));
            }
            else
            {
                foreach (string str in this.PropertyList)
                {
                    if (defValue.Equals(str, StringComparison.OrdinalIgnoreCase))
                    {
                        child.Value = str;
                        break;
                    }
                }
            }
            this.ParentControl.Controls.Add(child);
        }

        private string AddRow(string prevIDPostfix)
        {
            return this.AddRow(prevIDPostfix, string.Empty);
        }

        private string AddRow(string prevIDPostfix, string defProperty)
        {
            if (this.ParentControl == null)
            {
                return string.Empty;
            }
            string uniqueID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID(string.Empty);
            if (!string.IsNullOrEmpty(prevIDPostfix))
            {
                this.AddDelSection(prevIDPostfix);
            }
            this.AddFieldCombobox(uniqueID, defProperty);
            this.AddPropertyCombobox(uniqueID, defProperty);
            return uniqueID;
        }

        public virtual void CheckImport()
        {
            Job currentJob = JobHelper.CurrentJob;
            if (currentJob == null)
            {
                base.Next();
            }
            else if (!currentJob.IsDone)
            {
                SheerResponse.Timer("CheckImport", 300);
            }
            else
            {
                if (currentJob.Status.Result != null)
                {
                    this.UpdateForm(currentJob.Status.Result.ToString());
                }
                base.Next();
            }
        }

        protected void DelSection_Click()
        {
            string source = Context.ClientPage.ClientRequest.Source;
            if (!string.IsNullOrEmpty(source) && (source.Length > "DelSection".Length))
            {
                string str2 = source.Substring("DelSection".Length);
                if (this.FieldsSection.Controls.Count >= 1)
                {
                    System.Web.UI.Control control = this.FieldsSection.Controls[0];
                    foreach (string str3 in new string[] { "Field", "Property", "DelSection" })
                    {
                        System.Web.UI.Control control2 = control.FindControl(str3 + str2);
                        if (control2 != null)
                        {
                            control.Controls.Remove(control2);
                        }
                    }
                    SheerResponse.Refresh(this.FieldsSection);
                }
            }
        }

        protected void DomainComboboxChanged()
        {
            this.DomainInput.Value = this.DomainCombobox.Value;
        }

        protected void FieldChanged()
        {
            if (this.FieldsSection.Controls.Count >= 1)
            {
                System.Web.UI.Control control = this.FieldsSection.Controls[0];
                for (int i = control.Controls.Count - 1; i >= 0; i--)
                {
                    TraceableCombobox combobox = control.Controls[i] as TraceableCombobox;
                    if ((combobox != null) && combobox.ID.StartsWith("Field"))
                    {
                        if ((combobox.GetList().Count > 0) && (combobox.Value[0] != '<'))
                        {
                            this.AddRow(combobox.ID.Substring("Field".Length));
                            SheerResponse.Refresh(this.FieldsSection);
                        }
                        break;
                    }
                }
            }
        }

        protected virtual bool FieldsCompleted()
        {
            try
            {
                if (!this.MappedProperties.ContainsKey("Email"))
                {
                    SheerResponse.Alert(EcmTexts.Localize("In order to import users, an e-mail address must be available. Select a field in which the e-mail address should be stored.", new object[0]), new string[0]);
                    return false;
                }
            }
            catch (ArgumentException)
            {
                SheerResponse.Alert(EcmTexts.Localize("You have mapped a profile property to more than one field. Each profile property should be mapped only once.", new object[0]), new string[0]);
                return false;
            }
            return true;
        }
        private string FormatDefaultText(string text)
        {
            return HttpUtility.HtmlEncode("<" + text + ">");
        }

        private void InitializeFields()
        {
            if (this.ParentControl != null)
            {
                for (int i = this.ParentControl.Controls.Count - 1; i >= 0; i--)
                {
                    System.Web.UI.Control control = this.ParentControl.Controls[i];
                    if (!string.IsNullOrEmpty(control.ID) && ((control.ID.StartsWith("Field") || control.ID.StartsWith("Property")) || control.ID.StartsWith("DelSection")))
                    {
                        this.ParentControl.Controls.Remove(control);
                    }
                }
                string prevIDPostfix = this.AddRow(string.Empty, "Email");
                foreach (string str2 in this.FieldList)
                {
                    if (!"Email".Equals(str2, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string str3 in this.PropertyList)
                        {
                            if (str2.Equals(str3, StringComparison.OrdinalIgnoreCase))
                            {
                                prevIDPostfix = this.AddRow(prevIDPostfix, str3);
                            }
                        }
                    }
                }
                this.AddRow(prevIDPostfix);
                SheerResponse.Refresh(this.FieldsSection);
            }
        }

        protected override void OnCancel(object sender, EventArgs formEventArgs)
        {
            if (!string.IsNullOrEmpty(this.Filename.Value))
            {
                string filename = FileUtil.MapPath("/temp/" + FileUtil.GetFileName(this.Filename.Value));
                if (FileUtil.FileExists(filename))
                {
                    FileUtil.Delete(filename);
                }
            }
            base.OnCancel(sender, formEventArgs);
        }
        protected void Options_Click()
        {
            SheerResponse.Eval("$('RolesPanel').style.display='" + (this.AdvancedOptions.Checked ? string.Empty : "none") + "'");
        }

        protected void OptOutIn_Click()
        {
            this.AlltargetAudiences.TransferItems(this.OptOutOf, "Applications/16x16/delete.png");
        }

        protected void OptOutOut_Click()
        {
            this.OptOutOf.TransferItems(this.AlltargetAudiences, string.Empty);
        }
        protected void SelectDomain(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                UrlString str = new UrlString("/sitecore modules/shell/~/xaml/EmailCampaign.SelectDomain.aspx");
                str["sc_lang"] = Context.Language.Name;
                SheerResponse.ShowModalDialog(str.ToString(), "480", "530", string.Empty, true);
                args.WaitForPostBack();
            }
            else if (args.HasResult)
            {
                Edit var = args.CustomData["Input"] as Edit;
                Util.AssertNotNull(var);
                var.Value = args.Result;
            }
        }

        protected void SelectDomain_Click()
        {
            ClientPipelineArgs args = new ClientPipelineArgs();
            args.CustomData["Input"] = this.DomainInput;
            Context.ClientPage.Start(this, "SelectDomain", args);
        }

        protected virtual bool SelectFileCompleted()
        {
            if (string.IsNullOrEmpty(this.Filename.Value))
            {
                SheerResponse.Alert(EcmTexts.Localize("Select a file to attach.", new object[0]), new string[0]);
                return false;
            }
            string filename = FileUtil.MapPath("/temp/" + FileUtil.GetFileName(this.Filename.Value));
            if (!FileUtil.FileExists(filename))
            {
                SheerResponse.Alert(EcmTexts.Localize("The '{0}' file does not exist.", new object[] { filename }), new string[0]);
                return false;
            }
            if (!filename.Equals(this.LastFile))
            {
                using (CSVFile file = new CSVFile(filename))
                {
                    List<string> list = file.ReadLine();
                    this.FieldList = list;
                    this.LastFile = filename;
                    this.InitializeFields();
                }
            }
            return true;
        }

        protected void SelectRoles(ClientPipelineArgs args)
        {
            Edit edit = args.CustomData["RoleList"] as Edit;
            if (edit != null)
            {
                if (!args.IsPostBack)
                {
                    UrlString urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Security.SelectRoles.aspx");
                    UrlHandle handle = new UrlHandle();
                    handle["roles"] = edit.Value;
                    handle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), string.Empty, "600", string.Empty, true);
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

        protected void SubscribeIn_Click()
        {
            this.AlltargetAudiences.TransferItems(this.SubscribeTo, "Applications/16x16/check.png");
        }

        protected void SubscribeOut_Click()
        {
            this.SubscribeTo.TransferItems(this.AlltargetAudiences, string.Empty);
        }

        private void UpdateForm(string results)
        {
            if (!string.IsNullOrEmpty(results))
            {
                string[] strArray = results.Split(new char[] { '|' });
                if (strArray.Length >= 4)
                {
                    this.NumImported.Text = strArray[0];
                    this.NumEmailExists.Text = strArray[1];
                    this.NumNoEmail.Text = strArray[2];
                    this.NumBroken.Text = strArray[3];
                    SheerResponse.Refresh(this.Results);
                }
            }
        }

        protected string CurrentRecipientListId
        {
            get
            {
                return Context.ClientPage.Request.QueryString["rid"];
            }
        }

        protected List<string> FieldList
        {
            get
            {
                if (this.fieldList == null)
                {
                    this.fieldList = (List<string>)base.ServerProperties["FieldsList"];
                }
                return this.fieldList;
            }
            set
            {
                base.ServerProperties["FieldsList"] = value;
                this.fieldList = value;
            }
        }

        protected bool FillOptIn
        {
            get
            {
                return !string.IsNullOrEmpty(Context.ClientPage.Request.QueryString["optin"]);
            }
        }

        protected string LastFile
        {
            get
            {
                return (string)base.ServerProperties["LastFile"];
            }
            set
            {
                base.ServerProperties["LastFile"] = value;
            }
        }

        protected Dictionary<string, string> MappedProperties
        {
            get
            {
                if (this.mappedProperties == null)
                {
                    this.mappedProperties = new Dictionary<string, string>();
                    for (int i = 0; i < this.ParentControl.Controls.Count; i++)
                    {
                        TraceableCombobox combobox = this.ParentControl.Controls[i] as TraceableCombobox;
                        if (combobox != null)
                        {
                            TraceableCombobox combobox2 = this.ParentControl.Controls[i + 1] as TraceableCombobox;
                            if (combobox2 != null)
                            {
                                i++;
                                if ((combobox.Value[0] != '<') && (combobox2.Value[0] != '<'))
                                {
                                    this.mappedProperties.Add(combobox2.Value, combobox.Value);
                                }
                            }
                        }
                    }
                }
                return this.mappedProperties;
            }
        }

        protected System.Web.UI.Control ParentControl
        {
            get
            {
                if (this.parentControl == null)
                {
                    if (this.FieldsSection.Controls.Count < 1)
                    {
                        return null;
                    }
                    this.parentControl = this.FieldsSection.Controls[0];
                }
                return this.parentControl;
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
        protected TargetAudience RecipientList
        {
            get
            {
                string var = Context.ClientPage.Request.QueryString["rid"];
                Util.AssertNotNull(var); 
                TargetAudience ta = Factory.GetTargetAudience(var);                              
                Util.AssertNotNull(ta);               
                return ta;
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
