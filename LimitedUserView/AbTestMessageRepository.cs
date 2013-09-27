using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Analytics;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.StringExtensions;
using Sitecore.Data;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Extensions;
using Sitecore.Analytics.Data.Items;

namespace RecipientListManagement.LimitedUserView
{
    public class AbTestMessageRepository
    {
        // Fields
        private readonly AnalyticsDataGateway analyticsDataGateway = AnalyticsFactory.Instance.GetAnalyticsDataGateway();
        private readonly ManagerRoot managerRoot = UIFactory.Instance.GetSpeakContext().ManagerRoot;

        // Methods
        private Guid GetBestCampaignId()
        {
            Guid empty;
            Database contentDb = Util.GetContentDb();
            do
            {
                List<Guid> abnTestBestCampaigns = this.analyticsDataGateway.GetAbnTestBestCampaigns(this.managerRoot.InnerItem.ID.ToString());
                if (abnTestBestCampaigns.Count == 0)
                {
                    return Guid.Empty;
                }
                List<Guid> campaigns = new List<Guid>();
                empty = Guid.Empty;
                foreach (Guid guid2 in abnTestBestCampaigns)
                {
                    if (contentDb.SelectSingleItem(string.Format("fast://*[@Campaign='{0}']", ID.Parse(guid2))) == null)
                    {
                        campaigns.Add(guid2);
                    }
                    else
                    {
                        empty = guid2;
                        break;
                    }
                }
                if (campaigns.Count > 0)
                {
                    this.analyticsDataGateway.MarkRemovedMessages(campaigns);
                }
            }
            while (!(empty != Guid.Empty));
            return empty;
        }

        private List<MessageItem> GetMessages(string query)
        {
            Database contentDb = Util.GetContentDb();
            query = string.Format(query, UIFactory.Instance.GetSpeakContext().ManagerRoot.InnerItem.ID);           

            Item[] source = contentDb.SelectItems(query);

            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };

            if ((source != null) && (source.Length != 0))
            {
                return source.Where<Item>(funcIfVisible).Select<Item, MessageItem>(new Func<Item, MessageItem>(Factory.GetMessage)).ToList<MessageItem>();
            }
            return new List<MessageItem>();
        }

        public IEnumerable<AbTestMessage> GetRunningAbTestMessages(string query, string sortArgument, int startRows, int maxRows)
        {
            if (!sortArgument.StartsWith("StartTime"))
            {
                sortArgument = "StartTime DESC";
            }
            
            List<MessageItem> messages = this.GetMessages(query);
            if ((messages == null) || !messages.Any<MessageItem>())
            {
                return new List<AbTestMessage>();
            }
            messages = messages.Sort<MessageItem>(sortArgument).Skip<MessageItem>(startRows).Take<MessageItem>(maxRows).ToList<MessageItem>();
            Guid[] messageCampaigns = messages.Where<MessageItem>(delegate(MessageItem message)
            {
                return !message.CampaignId.IsNull;
            }).Select<MessageItem, Guid>(delegate(MessageItem message)
            {
                return message.CampaignId.ToGuid();
            }).ToArray<Guid>();
            List<AbnTestResult> source = this.analyticsDataGateway.GetAbnTestWinners(messageCampaigns) ?? new List<AbnTestResult>();
            Guid bestCampaignId = this.GetBestCampaignId();
            List<AbTestMessage> list3 = new List<AbTestMessage>();
            using (List<MessageItem>.Enumerator enumerator = messages.GetEnumerator())
            {
                Func<AbnTestResult, bool> predicate = null;
                MessageItem message;
                while (enumerator.MoveNext())
                {
                    message = enumerator.Current;
                    if (predicate == null)
                    {
                        predicate = delegate(AbnTestResult e)
                        {
                            return e.CampaignId == message.CampaignId.ToGuid();
                        };
                    }
                    AbnTestResult result = (source.Count > 0) ? source.FirstOrDefault<AbnTestResult>(predicate) : null;
                    AbTestMessage message2 = new AbTestMessage();
                    message2.ID = message.ID;
                    message2.Name = message.InnerItem.DisplayName;
                    message2.CurrentlyWinning = this.GetTestValueIndex(message, (result == null) ? 0 : result.TestCandidateIndex);
                    message2.Value = (result == null) ? 0 : result.Value;
                    message2.StartTime = message.StartTime;
                    AbTestMessage item = message2;
                    bool flag = (message.CampaignId.ToGuid() == bestCampaignId) && (item.Value > 0);
                    item.IsBest = flag;
                    List<string> list4 = new List<string>();
                    list4.Add("Value");
                    item.IsBestFieldsNames = flag ? list4 : new List<string>();
                    list3.Add(item);
                }
            }
            return list3;
        }

        private int GetTestValueIndex(MessageItem message, int testCandidateIndex)
        {
            AbnTest abnTest = CoreFactory.Instance.GetAbnTest(message);
            if (((abnTest == null) || (abnTest.TestDefinition == null)) || (abnTest.TestDefinition.Variables.Count == 0))
            {
                return 0;
            }
            List<PageLevelTestValueItem> values = abnTest.TestDefinition.Variables[0].Values;
            if (values.Count == 0)
            {
                return 0;
            }
            PageLevelTestValueItem winner = values[testCandidateIndex] ?? values[0];
            return abnTest.TestCandidates.FindIndex(delegate(Item v)
            {
                return v.ID == winner.Datasource.TargetID;
            });
        }


    }
}
