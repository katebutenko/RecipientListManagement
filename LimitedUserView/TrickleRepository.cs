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
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;

namespace RecipientListManagement.LimitedUserView
{
    public class TrickleRepository
    {
        // Fields
        private readonly AnalyticsFactory analyticsFactory = AnalyticsFactory.Instance;
        private readonly ManagerRoot managerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot;

        // Methods
        protected virtual IEnumerable<Item> GetItems(string query)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };

            string newValue = "*[@@id='{0}']".FormatWith(new object[] { this.managerRoot.InnerItem.ID.ToString() });
            FastItemReader source = new FastItemReader(this.managerRoot.InnerItem.Database.Name, query.Replace("{messages}", newValue));
            return source.Where<Item>(funcIfVisible).ToList<Item>();
        }

        public List<TrickleInfo> GetList(string query, int startIndex, int pageSize)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            return this.GetList(query, null, startIndex, pageSize);
        }

        public List<TrickleInfo> GetList(string query, string sortArgument, int startIndex, int pageSize)
        {
            if (!string.IsNullOrEmpty(sortArgument) && !sortArgument.StartsWith("Name"))
            {
                sortArgument = null;
            }
            Assert.ArgumentNotNullOrEmpty(query, "query");
            IEnumerable<Item> items = this.GetItems(query);
            if (!string.IsNullOrEmpty(sortArgument))
            {
                sortArgument = sortArgument.Replace("Name", "DisplayName");
                items = items.Sort<Item>(sortArgument);
            }
            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };

            List<MessageItem> messages = items.Skip<Item>(startIndex).Take<Item>(pageSize).Where<Item>(funcIfVisible).Select<Item, MessageItem>(new Func<Item, MessageItem>(Factory.GetMessage)).Where<MessageItem>(delegate(MessageItem m)
            {
                return (m != null);
            }).ToList<MessageItem>();
            if (messages.Count < 1)
            {
                return new List<TrickleInfo>();
            }
            List<VisitData> visitDataList = this.analyticsFactory.GetAnalyticsDataGateway().GetVisitData(messages);
            List<PlanData> planDataList = this.analyticsFactory.GetAnalyticsDataGateway().GetPlanData(messages);
            return messages.Select<MessageItem, TrickleInfo>(delegate(MessageItem m)
            {
                return this.GetTrickleInfo(m, visitDataList.FirstOrDefault<VisitData>(item => item.CampaignId == m.CampaignId.ToGuid()), planDataList.FirstOrDefault<PlanData>(item => item.PlanId == m.PlanId.ToGuid()));
            }).ToList<TrickleInfo>();
        }

        protected virtual MessageStateInfo GetMessageStateInfo(MessageItem message)
        {
            return new MessageStateInfo(message);
        }

        private TrickleInfo GetTrickleInfo(MessageItem message, VisitData visitData, PlanData planData)
        {
            MessageStateInfo messageStateInfo = this.GetMessageStateInfo(message);
            TrickleInfo info3 = new TrickleInfo();
            info3.ID = messageStateInfo.ID;
            info3.Name = messageStateInfo.Name;
            info3.HasAbn = messageStateInfo.HasAbn;
            TrickleInfo info2 = info3;
            int emailCount = -1;
            if (planData != null)
            {
                PlanStatistics planStatistics = this.analyticsFactory.GetPlanStatistics(planData);
                info2.OpenRate = planStatistics.GetOpenRate();
                info2.Recipients = planStatistics.GetTotal();
                emailCount = planStatistics.GetActual();
            }
            if (visitData != null)
            {
                info2.ValuePerVisit = visitData.ValuePerVisit;
                if (emailCount > -1)
                {
                    info2.ValuePerEmail = this.analyticsFactory.GetVisitStatistics(visitData).GetValuePerEmail(emailCount);
                }
            }
            return info2;
        }


    }
}
