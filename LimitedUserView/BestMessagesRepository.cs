using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Analytics;
using Sitecore.Modules.EmailCampaign.Exceptions;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sitecore.StringExtensions;

namespace RecipientListManagement.LimitedUserView
{
    public class BestMessagesRepository
    {
     // Fields
    private readonly AnalyticsDataGateway analyticsDataGateway;
    private string currentSortArgument;
    private SortOrder currentSortOrder;
    private readonly ManagerRoot managerRoot;
    private readonly List<string> orderByList;

    // Methods
    public BestMessagesRepository()
    {
        List<string> list = new List<string>();
        list.Add("Value");
        list.Add("ValuePerVisit");
        list.Add("VisitsPerEmail");
        list.Add("ValuePerEmail");
        this.orderByList = list;
        this.managerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot;
        this.analyticsDataGateway = AnalyticsFactory.Instance.GetAnalyticsDataGateway();
    }

    private void AssertOrderBy(string orderBy)
    {
        if (!this.orderByList.Any<string>(delegate (string order) {
            return string.Equals(orderBy, order, StringComparison.OrdinalIgnoreCase);
        }))
        {
            throw new EmailCampaignException("Sort column is wrong.");
        }
    }

    private BestMessageInfo CreateRow(MessageItem message, MessageData data)
    {
        MessageStateInfo info = new MessageStateInfo(message);
        BestMessageInfo info2 = new BestMessageInfo();
        info2.ID = info.ID;
        info2.CleanID = info.CleanID;
        info2.Name = info.Name;
        info2.Value = data.Value;
        info2.ValuePerVisit = data.ValuePerVisit;
        info2.VisitsPerEmail = data.VisitsPerEmail;
        info2.ValuePerEmail = data.ValuePerEmail;
        return info2;
    }

    protected virtual MessageItem FindMessage(MessageData messageData, Guid rootId)
    {
        return Factory.GetMessage(this.managerRoot.InnerItem.Database.SelectSingleItem("fast://*[@@id='{0}']/Messages//*[@Campaign='{1}']".FormatWith(new object[] { rootId, messageData.CampaignId.ToString("B") })));
    }

    protected virtual List<BestMessageInfo> GetBestExistingData(string orderBy, SortOrder sortOrder, int count)
    {
        List<MessageData> list2;
        Assert.ArgumentNotNullOrEmpty(orderBy, "orderBy");
        this.AssertOrderBy(orderBy);
        Guid managerRoot = this.managerRoot.InnerItem.ID.ToGuid();
        List<BestMessageInfo> list = new List<BestMessageInfo>();
        do
        {
            list2 = analyticsDataGateway.GetAllCampaignsSorted(managerRoot, orderBy, sortOrder);
            if (list2.Count == 0)
            {
                return list;
            }
            int num = Math.Min(list.Count + list2.Count, count);
            List<Guid> campaigns = new List<Guid>();
            foreach (MessageData data in list2)
            {
                MessageItem message = this.FindMessage(data, managerRoot);
                if ((message != null) && (this.messageSentOnlyByContextUser(message)))
                { 
                    list.Add(this.CreateRow(message, data));
                    if (list.Count >= num)
                    {
                        return list;
                    }
                }
            }
            this.analyticsDataGateway.MarkRemovedMessages(campaigns);
        }
        while (list2.Count >= 20);
        return list;
    }

    protected void SetBadge(List<BestMessageInfo> bestMessages, string orderBy)
    {
        object best;
        List<BestMessageInfo> list = ((this.currentSortOrder == SortOrder.Desc) && (orderBy == this.currentSortArgument)) ? bestMessages : this.GetBestExistingData(orderBy, SortOrder.Desc, 1);
        if (list.Count != 0)
        {
            best = DataBinder.Eval(list[0], orderBy);
            BestMessageInfo info = bestMessages.FirstOrDefault<BestMessageInfo>(delegate (BestMessageInfo e) {
                object obj2 = DataBinder.Eval(e, orderBy);
                if (obj2 is int)
                {
                    int num = (int) obj2;
                    return (num > 0) && (num.CompareTo(best) == 0);
                }
                if (!(obj2 is double))
                {
                    return false;
                }
                double num2 = (double) obj2;
                return (num2 > 0.0) && (num2.CompareTo(best) == 0);
            });
            if (info != null)
            {
                info.IsBest = true;
                info.IsBestFieldsNames.Add(orderBy);
            }
        }
    }

    protected virtual void SetBadges(List<BestMessageInfo> bestMessages)
    {
        Assert.ArgumentNotNull(bestMessages, "bestMessages");
        if (bestMessages.Count != 0)
        {
            foreach (string str in this.orderByList)
            {
                this.SetBadge(bestMessages, str);
            }
        }
    }

        
        public List<BestMessageInfo> GetLimitedBestMessages(string sortArgument)
        {

            Assert.ArgumentNotNullOrEmpty(sortArgument, "sortArgument");
            string[] strArray = sortArgument.Split(new char[] { ' ' });
            if (strArray.Length != 2)
            {
                throw new EmailCampaignException("The sortArgument parameter is wrong.");
            }
            this.currentSortArgument = strArray[0];
            this.currentSortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), strArray[1], true);
            List<BestMessageInfo> bestMessages = this.GetBestExistingData(this.currentSortArgument, this.currentSortOrder, 10);
            this.SetBadges(bestMessages);
            return bestMessages;
           
        }

        private bool messageSentOnlyByContextUser(MessageItem message)
        {
            string currentUserName = Sitecore.Context.User.Name;
            string dispatcherUserName = message.InnerItem.Statistics.CreatedBy;

            if (currentUserName.ToLowerInvariant() == dispatcherUserName.ToLowerInvariant())
            {
                return true;
            }
            return false;
        }

    }
}
