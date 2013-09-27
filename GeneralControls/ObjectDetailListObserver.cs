using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;
using Sitecore.Modules.EmailCampaign.Speak.Web.UI;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using System;
using System.Threading;
using System.Web.UI;
using Sitecore.Web.UI.WebControls.Extensions;
using Sitecore.Diagnostics;

namespace RecipientListManagement.GeneralControls
{
    public class ObjectDetailListObserver : RecipientListManagement.GeneralControls.ObjectDetailList
    {
        public event EventHandler<RowEventArgs> RowSelected;

        protected override void InitializeDetailList()
        {
            base.InitializeDetailList();
            base.detailList.RowClicked += new EventHandler<RowEventArgs>(this.SelectedRow);
        }

        protected override void InitializeFilterBuilder()
        {
            base.InitializeFilterBuilder();
            base.filterBuidler.PredicateSerializer = typeof(DefaultPredicateSerializer);
        }

        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
            if (!string.IsNullOrEmpty(this.DataSourceItem["EventKey"]))
            {
                string[] keys = this.DataSourceItem["EventKey"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string key in keys)
                {
                    if (key.StartsWith("Message"))
                    {
                        this.RegisterNotify(key, new EventHandler<EventArgs>(MessageReceived));
                    }
                    else
                    {
                        this.RegisterNotify(key, new EventHandler<EventArgs>(Update));
                    }
                }
            }
        }

        public virtual void MessageReceived(object sender, EventArgs eventArgs)
        {
            Log.Info("Uncatched message received from control. Please add a MessageReceived handler to the receiver.",this);
        }

        private void OnRowSelected(RowEventArgs e)
        {
            EventHandler<RowEventArgs> rowSelected = this.RowSelected;
            if (rowSelected != null)
            {
                rowSelected(this, e);
            }
        }

        private void SelectedRow(object sender, RowEventArgs e)
        {
            this.OnRowSelected(e);
        }

        public virtual void Update(object sender, EventArgs eventArgs)
        {
            this.DataBind();
            ScriptManager.GetCurrent(this.Page).UpdateControl(this);
        }
    }
}
