using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System.Web.UI;
using Sitecore.Modules.EmailCampaign.Speak.Web.UI.WebControls;
using System.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.Controls.Miscellaneous;
using System.Web.UI.HtmlControls;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Commands;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;
using Sitecore.Modules.EmailCampaign;

namespace RecipientListManagement.RecipientsLists.Lists
{
  public class BuildRecipientListGroup : CompositeWebControl, IControlState, IPostBackEventHandler
{
    // Fields
    protected readonly SimpleGroup BuildListPanel;
    protected readonly Button BuildNewRecipientList;
    protected readonly FilterDropList FilterDropList;
    protected readonly FastQueryDataSource RecipientListSource;
    protected readonly Spinner Spinner1;

    // Methods
    public BuildRecipientListGroup()
    {
        SimpleGroup group = new SimpleGroup {
            ID = "Group"
        };
        this.BuildListPanel = group;
        FilterDropList list = new FilterDropList {
            ID = "FilterDropList"
        };
        this.FilterDropList = list;
        FastQueryDataSource source = new FastQueryDataSource {
            ID = "RecipientListSource"
        };
        this.RecipientListSource = source;
        Button button = new Button {
            ID = "BuildNewRecipientList"
        };
        this.BuildNewRecipientList = button;
        Spinner spinner = new Spinner {
            ID = "BuildNewRecipientListSpinner"
        };
        this.Spinner1 = spinner;
    }

    protected override void CreateChildControls()
    {
        this.Controls.Clear();
        HtmlGenericControl child = new HtmlGenericControl("div") {
            ID = "BuildRecipientListGroupButtonGroup"
        };
        child.Attributes.Add("class", "sc-recipient-buttons-group");
        child.Controls.Add(this.BuildNewRecipientList);
        child.Controls.Add(this.Spinner1);
        Label label = new Label {
            ID = "BuildRecipientListGroupLabel",
            Text = this.DataSourceItem["Or"],
            CssClass = "sc-recipietn-label-or"
        };
        child.Controls.Add(label);
        child.Controls.Add(this.RecipientListSource);
        child.Controls.Add(this.FilterDropList);
        this.BuildListPanel.Content.Controls.Add(child);
        this.Controls.Add(this.BuildListPanel);
    }

    private void InitButton()
    {
        if (!string.IsNullOrEmpty(this.DataSourceItem["Build New Button"]))
        {
            this.BuildNewRecipientList.Text = this.DataSourceItem["Build New Button"];
        }
        this.BuildNewRecipientList.CssClass = "sc-button sc-button-important sc-recipient-buildnew";
        string str = string.Format("$.netajax($('#{0}'), 'CreateRecipientList'); return false;", this.ClientID);
        this.BuildNewRecipientList.OnClientClick = str;
    }

    private void InitExistingList()
    {
        MessageItem message = UIFactory.Instance.GetSpeakContext().Message;
        this.RecipientListSource.ID = "RecipientListSource";
        if (Sitecore.Context.User.IsAdministrator)
        {
            this.RecipientListSource.SelectCommand = "fast:@recipientContainerPath[@@id = '@recipientContainerId']//*[(@@templateid='@recipientListTemplate') and @filterExpression]";
        }
        else
        {
            this.RecipientListSource.SelectCommand = "fast:@recipientContainerPath[@@id = '@recipientContainerId']//*[(@__Created by='@ownerName') and (@@templateid='@recipientListTemplate') and @filterExpression]";
        }
        Parameter parameter = new Parameter("@recipientContainerPath") {
            DefaultValue = ItemUtilExt.GetRecipientListsContainerItem(message.InnerItem).Paths.FullPath
        };
        this.RecipientListSource.SelectParameters.Add(parameter);
        Parameter parameter2 = new Parameter("@recipientContainerId") {
            DefaultValue = ItemUtilExt.GetRecipientListsContainerItem(message.InnerItem).ID.ToString()
        };
        this.RecipientListSource.SelectParameters.Add(parameter2);
        Parameter parameter3 = new Parameter("@recipientListTemplate") {
            DefaultValue = "{B95EB9EA-8F86-44FE-B619-4B29C1343F95}"
        };
        this.RecipientListSource.SelectParameters.Add(parameter3);
        if (!Sitecore.Context.User.IsAdministrator)
        {
            Parameter parameter5 = new Parameter("@ownerName")
            {
                DefaultValue = Sitecore.Context.User.Name.Replace(@"\\", @"\")
            };
            this.RecipientListSource.SelectParameters.Add(parameter5);
        }
        ControlParameter parameter4 = new ControlParameter("@filterExpression", this.FilterDropList.ID, "FilterExpression") {
            DefaultValue = "true"
        };
        this.RecipientListSource.SelectParameters.Add(parameter4);
        this.FilterDropList.DataSourceId = this.RecipientListSource.ID;
        this.FilterDropList.Command = typeof(AddRecipientListCommand).FullName;
        this.FilterDropList.Width = new Unit("120px");
        this.FilterDropList.CssClass = "sc-recipient-menu-button";
        if (!string.IsNullOrEmpty(this.DataSourceItem["Use Existing List Button"]))
        {
            this.FilterDropList.Title = this.DataSourceItem["Use Existing List Button"];
        }
    }

    private void InitSimpleGroup()
    {
        if (!string.IsNullOrEmpty(this.DataSourceItem["Title"]))
        {
            this.BuildListPanel.Title = this.DataSourceItem["Title"];
        }
        this.BuildListPanel.Visible = this.Spinner1.Visible = this.QueryState() == ControlState.Active;
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        this.ID = "BuildRecipientListGroup";
        base.Attributes["name"] = this.UniqueID;
        this.InitSimpleGroup();
        this.InitExistingList();
        this.InitButton();
        string[] keys = this.DataSourceItem["EventKey"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        this.RegisterNotify(keys, new EventHandler<EventArgs>(this.Update));
        this.Spinner1.TargetControlId = this.BuildNewRecipientList.ID;
    }

    public ControlState QueryState()
    {
        MessageItem message = UIFactory.Instance.GetSpeakContext().Message;
        if ((message != null) && (message.TargetAudience != null))
        {
            return ControlState.TemporaryHidden;
        }
        return ControlState.Active;
    }

    public void RaisePostBackEvent(string eventArgument)
    {
        MessageItem message = UIFactory.Instance.GetSpeakContext().Message;
        if ((message != null) && (message.TargetAudience == null))
        {
            TargetAudienceSource.Create(message);
            NotificationManager.Instance.Notify("RecipientListCreated");
        }
    }

    private void Update(object sender, EventArgs eventArgs)
    {
        this.InitSimpleGroup();
        ScriptManager.GetCurrent(this.Page).UpdateControl(this);
    }
}

}
