using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Analytics;
using Sitecore.StringExtensions;
using Sitecore.Data;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;

namespace RecipientListManagement.LimitedUserView
{
    public class DispatchedMessageRepository
    {
        // Fields
        private readonly ManagerRoot managerRoot;
        private const int RowsNumber = 6;

        // Methods
        public DispatchedMessageRepository()
            : this(UIFactory.Instance.GetSpeakContext().ManagerRoot)
        {
        }

        public DispatchedMessageRepository(ManagerRoot managerRoot)
        {
            Assert.ArgumentNotNull(managerRoot, "managerRoot");
            this.managerRoot = managerRoot;
        }

        private List<DispatchedMessageInfo> Build(List<MessageItem> messages)
        {
            List<VisitData> visitData = AnalyticsFactory.Instance.GetAnalyticsDataGateway().GetVisitData(messages);
            List<PlanData> planData = AnalyticsFactory.Instance.GetAnalyticsDataGateway().GetPlanData(messages);
            return messages.Select<MessageItem, DispatchedMessageInfo>(delegate(MessageItem message)
            {
                return this.BuildDispatchMessageInfo(message, planData.FirstOrDefault<PlanData>(item => item.PlanId == message.PlanId.ToGuid()), visitData.FirstOrDefault<VisitData>(item => item.CampaignId == message.CampaignId.ToGuid()));
            }).ToList<DispatchedMessageInfo>();
        }

        private DispatchedMessageInfo BuildDispatchMessageInfo(MessageItem message, PlanData planData, VisitData visitData)
        {
            MessageStateInfo info = new MessageStateInfo(message);
            DispatchedMessageInfo info3 = new DispatchedMessageInfo();
            info3.ID = info.ID;
            info3.Name = info.Name;
            info3.MessageType = info.Type;
            info3.Date = info.StartDate;
            info3.State = info.Status;
            DispatchedMessageInfo info2 = info3;
            if (planData != null)
            {
                DateTime time;
                DateTime time2;
                PlanStatistics planStatistics = AnalyticsFactory.Instance.GetPlanStatistics(planData);
                AnalyticsHelper.TryGetCampaignDates(message.CampaignId.ToGuid(), out time, out time2);
                info2.Sent = (time != time2) ? planStatistics.GetTotal() : 0;
                info2.OpenRate = planStatistics.GetOpenRate();
                info2.ClickRate = planStatistics.GetClickRate();
            }
            if (visitData != null)
            {
                info2.ValuePerVisit = visitData.ValuePerVisit;
                info2.Value = visitData.Value;
            }
            return info2;
        }

        public List<DispatchedMessageInfo> GetDispatchedMessageInfo(string query, string sortArgument)
        {
            string newValue = string.Format("*[@@id='{0}']", this.managerRoot.InnerItem.ID.ToString());
            FastItemReader source = new FastItemReader(this.managerRoot.InnerItem.Database.Name, query.Replace("{messages}", newValue));

            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };

            List<MessageItem> messages = source.Where<Item>(funcIfVisible).ToList<Item>().ConvertAll<MessageItem>(new Converter<Item, MessageItem>(Factory.GetMessage)).OrderByDescending<MessageItem, DateTime>(delegate(MessageItem message)
            {
                return message.StartTime;
            }).Take<MessageItem>(6).ToList<MessageItem>();
            return this.Build(messages);
        }


    }
}
