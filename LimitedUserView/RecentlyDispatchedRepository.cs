using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Analytics;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.StringExtensions;
using Sitecore.Data;

namespace RecipientListManagement.LimitedUserView
{
    public class RecentlyDispatchedRepository
    {
        private readonly AnalyticsFactory analyticsFactory;

        private readonly ManagerRoot managerRoot;

        private readonly string itemid;

        private readonly string dbname;

        public RecentlyDispatchedRepository()
        {
            this.managerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot;
            this.analyticsFactory = AnalyticsFactory.Instance;
        }

        public RecentlyDispatchedRepository(string itemid, string dbname)
        {
            this.itemid = itemid;
            this.dbname = dbname;
        }      

        public List<DispatchedMessageInfo> GetAllRecentlyDispatched(string query)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            
            List<MessageItem> messages = GetSortedItem(query, "Data DESC").Select<Item, MessageItem>(new Func<Item, MessageItem>(Factory.GetMessage)).Where<MessageItem>(delegate(MessageItem m)
            {
                return (m != null);
            }).ToList<MessageItem>();
            if (messages.Count == 0)
            {
                return new List<DispatchedMessageInfo>();
            }
            List<VisitData> visitDataList = AnalyticsFactory.Instance.GetAnalyticsDataGateway().GetVisitData(messages);
            List<PlanData> planDataList = AnalyticsFactory.Instance.GetAnalyticsDataGateway().GetPlanData(messages);
            return messages.Select<MessageItem, DispatchedMessageInfo>(delegate(MessageItem m)
            {
                return GetDispatchedInfo(m, visitDataList.FirstOrDefault<VisitData>(item => item.CampaignId == m.CampaignId.ToGuid()), planDataList.FirstOrDefault<PlanData>(item => item.PlanId == m.PlanId.ToGuid()));
            }).Where<DispatchedMessageInfo>(delegate(DispatchedMessageInfo m1) { return messageSentByContextUser(m1); }).ToList<DispatchedMessageInfo>();
        }


        private DispatchedMessageInfo GetDispatchedInfo(MessageItem message, VisitData visitData, PlanData planData)
        {
            MessageStateInfo messageStateInfo = this.GetMessageStateInfo(message);
            DispatchedMessageInfo info3 = new DispatchedMessageInfo();
            info3.ID = messageStateInfo.ID;
            info3.Name = messageStateInfo.Name;
            info3.State = messageStateInfo.Status;
            info3.Date = messageStateInfo.Updated;
            info3.Sent = messageStateInfo.Sent;
            info3.NumSubscribers = messageStateInfo.NumSubscribers;
            info3.MessageState = messageStateInfo.MessageState;
            DispatchedMessageInfo info2 = info3;
            int emailCount = -1;
            if (planData != null)
            {
                PlanStatistics planStatistics = AnalyticsFactory.Instance.GetPlanStatistics(planData);
                info2.OpenRate = planStatistics.GetOpenRate();
                info2.ClickRate = planStatistics.GetClickRate();
                emailCount = planStatistics.GetActual();
            }
            if (visitData != null)
            {
                info2.ValuePerVisit = visitData.ValuePerVisit;
                if (emailCount > -1)
                {
                    VisitStatistics visitStatistics = AnalyticsFactory.Instance.GetVisitStatistics(visitData);
                    info2.ValuePerEmail = visitStatistics.GetValuePerEmail(emailCount);
                    info2.VisitsPerEmail = visitStatistics.GetVisitPerEmail(emailCount);
                }
            }
            return info2;
        }

 

        private bool messageSentByContextUser(DispatchedMessageInfo messageInfo)
        {
            MessageItem message = Factory.GetMessage(messageInfo.ID);
            string currentUserName = Sitecore.Context.User.Name;
            string dispatcherUserName = message.InnerItem.Statistics.CreatedBy;

            if (currentUserName.ToLowerInvariant() == dispatcherUserName.ToLowerInvariant() || Sitecore.Context.User.IsAdministrator)
            {
                return true;
            }
            return false;
        }

        protected virtual MessageStateInfo GetMessageStateInfo(MessageItem message)
        {
            return new MessageStateInfo(message);
        }

        public List<DispatchedMessageInfo> GetRecentlyDispatched(string query, string sortArgument, int startIndex, int pageSize)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            Assert.ArgumentNotNullOrEmpty(sortArgument, "sortArgument");
            List<MessageItem> messages = GetSortedItem(query, sortArgument).Skip<Item>(startIndex).Take<Item>(pageSize).Select<Item, MessageItem>(new Func<Item, MessageItem>(Factory.GetMessage)).Where<MessageItem>(delegate(MessageItem m)
            {
                return (m != null);
            }).ToList<MessageItem>();
            if (messages.Count == 0)
            {
                return new List<DispatchedMessageInfo>();
            }
            List<VisitData> visitDataList = this.analyticsFactory.GetAnalyticsDataGateway().GetVisitData(messages);
            List<PlanData> planDataList = this.analyticsFactory.GetAnalyticsDataGateway().GetPlanData(messages);
            return messages.Select<MessageItem, DispatchedMessageInfo>(delegate(MessageItem m)
            {
                return this.GetDispatchedInfo(m, visitDataList.FirstOrDefault<VisitData>(item => item.CampaignId == m.CampaignId.ToGuid()), planDataList.FirstOrDefault<PlanData>(item => item.PlanId == m.PlanId.ToGuid()));
            }).ToList<DispatchedMessageInfo>();
        }

        protected virtual IEnumerable<Item> GetSortedItem(string query, string sortArgument)
        {
            Func<Item, object> func;
            Assert.ArgumentNotNullOrEmpty(query, "query");
            Assert.ArgumentNotNullOrEmpty(sortArgument, "sortArgument");
            if (sortArgument.StartsWith("Name "))
            {
                func = delegate(Item i)
                {
                    return i.DisplayName;
                };
            }
            else
            {
                func = delegate(Item i)
                {
                    return i.Statistics.Updated;
                };
            }

            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };
            string id = (itemid != null) ? itemid : this.managerRoot.InnerItem.ID.ToString();
            string newValue = "*[@@id='{0}']".FormatWith(new object[] { id });

            string databasename = (dbname != null) ? dbname : this.managerRoot.InnerItem.Database.Name;
            FastItemReader source = new FastItemReader(databasename, query.Replace("{messages}", newValue));
            if (!sortArgument.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
            {
                return source.OrderByDescending<Item, object>(func).Where<Item>(funcIfVisible);
            }
            return source.OrderBy<Item, object>(func).Where<Item>(funcIfVisible);
        }

 

 


    }
}
