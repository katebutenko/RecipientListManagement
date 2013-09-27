using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Links;
using Sitecore.Speak.Extensions;
using Sitecore.Web.UI;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System.Web.UI.HtmlControls;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;

namespace RecipientListManagement.GeneralControls
{
    public abstract class CustomizedDetailList<T> : CompositeWebControl, ISpeakUpdateable where T : DataSourceControl
    {
        protected readonly Accordion accordion;
        protected CustomizedActions actions;
        protected readonly T dataSourceControl;
        protected readonly DetailList detailList;
        protected readonly IList<string> DetailListKeys;
        protected readonly FilterBuilder filterBuidler;
        private string shortId;
        protected readonly Popup smartPanel;
        protected HtmlInputHidden hiddenInput;

        public string LastSelectedRow
        {
            get
            {
                return hiddenInput.Value;
            }
            set
            {
                hiddenInput.Value = value;
            }
        }

        protected CustomizedDetailList(T dataSourceControl)
        {
            Assert.ArgumentNotNull(dataSourceControl, "dataSourceControl");
            this.dataSourceControl = dataSourceControl;
            this.filterBuidler = new FilterBuilder();
            this.detailList = new DetailList();
            this.accordion = new Accordion();
            this.DetailListKeys = new List<string>();
            this.smartPanel = new Popup();
            //this.ViewState["EnabledVS"] = true;
            this.hiddenInput = new HtmlInputHidden();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!String.IsNullOrEmpty(hiddenInput.Value))
            {
                this.actions.DataModel = hiddenInput.Value;
            }
        }

        protected virtual void ActionClicked(object sender, EventArgs e)
        {
            if (e != null)
            {               
                ItemClickedEventArgs<ActionItem> args = (ItemClickedEventArgs<ActionItem>)e;
                //standard functionality:
                //if (((this.detailList != null) && (this.detailList.DataSourceID != null)) && (this.detailList.SelectedRows.Length != 0))
                //overridden functionality:
                if (!String.IsNullOrEmpty(this.LastSelectedRow))
                {
                    this.actions.DataModel = this.LastSelectedRow;
                    //standard functionality:
                    //string str = string.Join("|", this.LastSelectedRow);
                    string str = this.LastSelectedRow;
                    int index = args.Item.Url.IndexOf('(');
                    StringBuilder builder = new StringBuilder();
                    builder.Append(args.Item.Url.Substring(0, (index > 0) ? index : args.Item.Url.Length));
                    builder.Append("(ids=");
                    builder.Append("{" + str + "}");
                    if (index > 0)
                    {
                        builder.Append(',');
                        builder.Append(args.Item.Url.Substring(index + 1, (args.Item.Url.Length - index) - 2));
                    }
                    builder.Append(')');
                    args.Item.Command = builder.ToString();
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            if (this.DataSourceItem != null)
            {
                this.shortId = this.DataSourceItem.ID.ToShortID().ToString();
                this.dataSourceControl.ID = "source_" + this.shortId;
                this.ID = "CompositeDetailList_" + this.shortId;
                this.filterBuidler.ID = "filter_" + this.shortId;
                this.detailList.ID = "detailList_" + this.shortId;
                this.accordion.ID = "accordion_" + this.shortId;
                this.hiddenInput.ID = "hiddenInput_" + this.shortId;
                this.Controls.Add(hiddenInput);
                this.actions = new CustomizedActions("actions_" + this.shortId, "div", string.Empty);
                this.smartPanel.ID = "smartPanel_" + this.shortId;
                this.smartPanel.Type = PopupType.SmartPanel;
                this.accordion.Header.Controls.Add(this.actions);
                this.accordion.Content.Controls.Add(this.filterBuidler);
                this.accordion.Content.Controls.Add(this.dataSourceControl);
                this.accordion.Content.Controls.Add(this.detailList);
                this.Controls.Add(this.accordion);
                this.Controls.Add(this.smartPanel);

                this.actions.DataSource = this.DataSourceItem["Actions"];
                //if (this.DataSourceItem["Actions"] == "{7BBA13F2-CAAE-48FF-86B2-28B0F609598B}")
                //{

                //}
            }
        }

        protected virtual void DetailList_MultiRowClicked(object sender, EventArgs args)
        {
            if (this.DataSourceItem != null)
            {
                if (!string.IsNullOrEmpty(this.DataSourceItem["SmartPanel"]) && Sitecore.Data.ID.IsID(this.DataSourceItem["SmartPanel"]))
                {
                    if (((DetailList)sender).SelectedRows == null)
                    {
                        this.smartPanel.Close();
                    }
                }
                else
                {
                    foreach (Popup popup in from p in this.Page.Controls.Flatten<Popup>()
                                            where p.Type == PopupType.SmartPanel
                                            select p)
                    {
                        if (popup.Visible)
                        {
                            popup.Close();
                        }
                    }
                }
            }
        }

        protected virtual void DetailList_MultiSelectedRowsChanged(object sender, EventArgs args)
        {
        }

        protected virtual void DetailList_SingleRowChanged(object sender, EventArgs args)
        {
            if (((DetailList)sender).SelectedRows.FirstOrDefault<string>() != null)
            {
                string selectedRowId = ((DetailList)sender).SelectedRows.FirstOrDefault<string>();

                if (LastSelectedRow != selectedRowId)
                {
                    LastSelectedRow = selectedRowId;
                    ScriptManager.GetCurrent(this.Page).UpdateControl(this.hiddenInput);
                    //((CustomizedActions)this.actions).DataModel = selectedRowId;
                }
            }

            if (this.DataSourceItem != null)
            {
                if (!string.IsNullOrEmpty(this.DataSourceItem["SmartPanel"]) && Sitecore.Data.ID.IsID(this.DataSourceItem["SmartPanel"]))
                {
                    if (((DetailList)sender).SelectedRows.FirstOrDefault<string>() != null)
                    {
                        foreach (string str in this.DetailListKeys)
                        {
                            this.smartPanel.Parameters.Add(str, ((DetailList)sender).SelectedRows.FirstOrDefault<string>());
                        }
                        this.smartPanel.Show();
                    }
                    else
                    {
                        this.smartPanel.Close();
                    }
                }
                else
                {
                    foreach (Popup popup in from p in this.Page.Controls.Flatten<Popup>()
                                            where p.Type == PopupType.SmartPanel
                                            select p)
                    {
                        if (popup.ID != this.smartPanel.ID)
                        {
                            popup.Close();
                        }
                    }
                }
            }
            //ScriptManager.GetCurrent(this.Page).UpdateControl(this.hiddenInput);
            //((CustomizedActions)this.actions).DataModel = this.hiddenInput.Value;
            //ScriptManager.GetCurrent(this.Page).UpdateControl(this.actions);
        }

        protected virtual void DetailList_SingleRowClicked(object sender, EventArgs args)
        {
            if (this.DataSourceItem != null)
            {
                if (!string.IsNullOrEmpty(this.DataSourceItem["SmartPanel"]) && Sitecore.Data.ID.IsID(this.DataSourceItem["SmartPanel"]))
                {
                    if (((DetailList)sender).SelectedRows.FirstOrDefault<string>() != null)
                    {
                        this.smartPanel.Close();
                    }
                }
                else
                {
                    foreach (Popup popup in from p in this.Page.Controls.Flatten<Popup>()
                                            where p.Type == PopupType.SmartPanel
                                            select p)
                    {
                        if (popup.ID != this.smartPanel.ID)
                        {
                            popup.Close();
                        }
                    }
                }
            }
        }

        protected virtual void DetailList_SmartPanelCancel(object sender, EventArgs args)
        {
        }

        protected virtual void DetailList_SmartPanelClose(object sender, EventArgs args)
        {
            if (this.smartPanel.IsItemUpdated && this.Page.IsPostBack)
            {
                foreach (Control control in this.Page.Controls.Flatten<ISpeakUpdateable>())
                {
                    if ((control != null) && (control.GetType() == base.GetType()))
                    {
                        foreach (DetailList list in control.Controls.Flatten<DetailList>())
                        {
                            list.DataBind();
                            ScriptManager.GetCurrent(this.Page).UpdateControl(list);
                        }
                    }
                }
            }
        }

        protected virtual void DetailList_SmartPanelShow(object sender, EventArgs args)
        {
        }

        protected virtual Column GetColumn(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Column column = new Column
            {
                Title = item["HeaderText"],
                Name = string.IsNullOrEmpty(item["DataField"]) ? item.Name : item["DataField"],
                Sortable = (item["Sortable"] != null) && (item["Sortable"] == "1"),
                Key = (item["Key"] != null) && (item["Key"] == "1"),
                Hidden = (item["Hidden"] != null) && (item["Hidden"] == "1")
            };
            if (!string.IsNullOrEmpty(item["Alignment"]))
            {
                Item item2 = Sitecore.Context.Database.GetItem(item["Alignment"]);
                column.Alignment = (item2 != null) ? EnumExtensions.FromString<TextAlignment>(item2.Name, TextAlignment.Center) : TextAlignment.Center;
            }
            if (!string.IsNullOrEmpty(item["Formatter"]))
            {
                Item item3 = Sitecore.Context.Database.GetItem(item["Formatter"]);
                column.Formatter = (item3 != null) ? EnumExtensions.FromString<Formatter>(item3.Name, Formatter.None) : Formatter.None;
            }
            if (column.Key && !this.DetailListKeys.Contains(column.Name))
            {
                this.DetailListKeys.Add(column.Name);
            }
            this.ResolveColumnTemplate(column, item);
            return column;
        }

        protected virtual ITemplate GetColumnRendering(Item item)
        {
            if (item.Visualization.GetRenderings(Sitecore.Context.Device, true).Length > 0)
            {
                RenderingReference reference = item.Visualization.GetRenderings(Sitecore.Context.Device, true).FirstOrDefault<RenderingReference>();
                if (reference != null)
                {
                    return (reference.GetControl() as ITemplate);
                }
            }
            return null;
        }

        protected virtual FilterField GetFilterField(Item item)
        {
            return new FilterField { Name = item["DataField"], Title = item["HeaderText"] };
        }

        protected virtual Operator GetFilterOperator(Item item)
        {
            return new Operator { Name = item.Name, Title = item.DisplayName };
        }

        protected virtual void InitializeAccordion()
        {
            if (this.DataSourceItem != null)
            {
                this.accordion.EnableCollapsing = (this.DataSourceItem["EnableCollapsing"] != null) && (this.DataSourceItem["EnableCollapsing"] == "1");
                this.accordion.Collapsed = (this.DataSourceItem["Collapsed"] != null) && (this.DataSourceItem["Collapsed"] == "1");
                this.accordion.Header.Title = !string.IsNullOrEmpty(this.DataSourceItem["Title"]) ? this.DataSourceItem["Title"] : string.Empty;
            }
        }

        protected virtual void InitializeActions()
        {
            if (this.actions != null)
            {
                Sitecore.Web.UI.WebControls.Actions actions = this.actions.Controls.Flatten<Sitecore.Web.UI.WebControls.Actions>().FirstOrDefault<Sitecore.Web.UI.WebControls.Actions>(p => p != null);
                if (actions != null)
                {
                    actions.Click += new EventHandler<EventArgs>(this.ActionClicked);
                }
            }
        }

        protected abstract void InitializeDataSourceControl();
        protected virtual void InitializeDetailList()
        {
            if (this.DataSourceItem != null)
            {
                this.detailList.Columns.Add(this.DataSourceItem.Children.Select<Item, Column>(new Func<Item, Column>(this.GetColumn)));
                this.detailList.DataKeyNames = (from c in this.detailList.Columns
                                                where c.Key
                                                select c.Name).ToArray<string>();
                this.detailList.DataSourceID = this.dataSourceControl.ID;
                Item item = Sitecore.Context.Database.GetItem(this.DataSourceItem["DefaultSortColumn"]);
                string str = (item != null) ? item["DataField"] : string.Empty;
                string str2 = this.DataSourceItem["DefaultSortDirection"];
                if (!string.IsNullOrEmpty(str))
                {
                    this.detailList.SortColumn = str;
                    this.detailList.SortDirection = EnumExtensions.FromString<SortDirection>(str2, SortDirection.Ascending);
                }
                this.detailList.LoadDataWith = EnumExtensions.FromString<LoadMode>(this.DataSourceItem["LoadDataWith"], LoadMode.PageScroll);
                if (this.detailList.LoadDataWith == LoadMode.ElementScroll)
                {
                    int result = 6;
                    if (int.TryParse(this.DataSourceItem["Rows"], out result))
                    {
                        this.detailList.RowNum = result;
                    }
                }
                bool flag = this.DataSourceItem["Multiselect"] == "1";
                this.detailList.Multiselect = flag;
                if (flag)
                {
                    this.detailList.RowClicked += new EventHandler<RowEventArgs>(this.DetailList_MultiRowClicked);
                    this.detailList.SelectedRowsChanged += new EventHandler<EventArgs>(this.DetailList_MultiSelectedRowsChanged);
                }
                else
                {
                    this.detailList.RowClicked += new EventHandler<RowEventArgs>(this.DetailList_SingleRowClicked);
                    this.detailList.SelectedRowsChanged += new EventHandler<EventArgs>(DetailList_SingleRowChanged);
                }
            }
        }

        protected virtual void InitializeFilterBuilder()
        {
            Func<Item, FilterField> selector = null;
            this.filterBuidler.Attributes["UpdateControls"] = this.detailList.UniqueID;
            if (this.filterBuidler.EnableSearching && !this.filterBuidler.EnableFiltering)
            {
                if (selector == null)
                {
                    selector = i => this.GetFilterField(i);
                }
                this.filterBuidler.Fields.Add(this.DataSourceItem.Children.Where<Item>(delegate(Item i)
                {
                    if (!string.IsNullOrEmpty(i["Hidden"]))
                    {
                        return !(i["Hidden"] == "1");
                    }
                    return true;
                }).Select<Item, FilterField>(selector));
            }
            else if (this.filterBuidler.EnableFiltering && !string.IsNullOrEmpty(this.DataSourceItem["Filters"]))
            {
                foreach (string str in this.DataSourceItem["Filters"].Split(new char[] { '|' }))
                {
                    Item item = Sitecore.Context.Database.GetItem(str);
                    if ((item != null) && item.HasChildren)
                    {
                        FilterField field = new FilterField
                        {
                            Title = string.IsNullOrEmpty(item["title"]) ? string.Empty : item["title"],
                            Name = string.IsNullOrEmpty(item["name"]) ? string.Empty : item["name"]
                        };
                        field.Type = EnumExtensions.FromString<FilterFieldType>(item["type"], FilterFieldType.Select);
                        field.Group = string.IsNullOrEmpty(item["title"]) ? string.Empty : item["title"];
                        foreach (Item item2 in item.Children)
                        {
                            Sitecore.Web.UI.WebControls.Expression expression = new Sitecore.Web.UI.WebControls.Expression
                            {
                                Title = item2["title"],
                                Value = item2["value"],
                                Operator = item2["operator"]
                            };
                            field.Expressions.Add(expression);
                        }
                        this.filterBuidler.Fields.Add(field);
                    }
                }
            }
        }

        protected virtual void InitializeSmartPanel()
        {
            if (((this.DataSourceItem != null) && !string.IsNullOrEmpty(this.DataSourceItem["SmartPanel"])) && Sitecore.Data.ID.IsID(this.DataSourceItem["SmartPanel"]))
            {
                Item item = Sitecore.Context.Database.GetItem(this.DataSourceItem["SmartPanel"]);
                if (item != null)
                {
                    this.smartPanel.Url = LinkManager.GetItemUrl(item, UrlOptions.DefaultOptions);
                    this.smartPanel.PopupShow += new EventHandler<EventArgs>(this.DetailList_SmartPanelShow);
                    this.smartPanel.PopupClose += new EventHandler<EventArgs>(this.DetailList_SmartPanelClose);
                    this.smartPanel.PopupCancel += new EventHandler<EventArgs>(this.DetailList_SmartPanelCancel);
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.DataSourceItem != null)
            {
                this.filterBuidler.EnableFiltering = !string.IsNullOrEmpty(this.DataSourceItem["EnableFiltering"]) && (this.DataSourceItem["EnableFiltering"] == "1");
                this.filterBuidler.EnableSearching = !string.IsNullOrEmpty(this.DataSourceItem["EnableSearching"]) && (this.DataSourceItem["EnableSearching"] == "1");
            }
            this.InitializeDataSourceControl();
            this.InitializeActions();
            this.InitializeAccordion();
            this.InitializeDetailList();
            this.InitializeFilterBuilder();
            this.InitializeSmartPanel();
        }

        protected virtual void ResolveColumnTemplate(Column column, Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            ITemplate columnRendering = this.GetColumnRendering(item);
            if (columnRendering != null)
            {
                Sitecore.Web.UI.WebControl control = columnRendering as Sitecore.Web.UI.WebControl;
                if (control != null)
                {
                    control.DataSource = item.ID.ToString();
                }
                column.Template = columnRendering;
            }
        }

        public abstract DetailList List { get; }
    }
}