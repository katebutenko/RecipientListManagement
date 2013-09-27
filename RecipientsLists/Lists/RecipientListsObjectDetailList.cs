using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Modules.EmailCampaign.Speak.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Modules.EmailCampaign.Speak.Web.UI;
using Sitecore.Web.UI.WebControls.Extensions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Modules.EmailCampaign.Messages;
using System.Web.UI.HtmlControls;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;

namespace RecipientListManagement.RecipientsLists.Lists
{
    public class RecipientListsObjectDetailList : RecipientListManagement.GeneralControls.ObjectDetailListObserver
    {
        // Methods

        protected override void InitializeFilterBuilder()
        {
            base.InitializeFilterBuilder();
            base.filterBuidler.PredicateSerializer = typeof(DefaultPredicateSerializer);
        }

        protected override void InitializeDataSourceControlSelectMethod()
        {
            base.InitializeDataSourceControlSelectMethod();
            //in case if you need sorting. Do not forget to add selectMethod(sortParameter) to Repository
            //base.dataSourceControl.SortParameterName = this.DataSourceItem.Fields["SortParameterName"].Value;
            RowSelected += new EventHandler<RowEventArgs>(this.RowClicked);
        }

        private void RowClicked(object sender, RowEventArgs e)
        {
            NotificationManager.Instance.Notify("RecipientListClicked", e);
        }
        public override void MessageReceived(object sender, EventArgs eventArgs)
        {
            MessageEventArgs args = eventArgs as MessageEventArgs;
            if (args != null)
            {
                ScriptManager.GetCurrent(this.Page).Info(args.Message, false);
            }
        }
    }
}
