using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Modules.EmailCampaign.Core.Analytics;
using Sitecore;
using System.Configuration;
using System.Data;
using Sitecore.Data;
using Sitecore.Analytics.Data.DataAccess;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Data.Items;
using Sitecore.StringExtensions;

namespace RecipientListManagement
{
    public static class AnalyticsSQLDataGatewayExtensions
    {

        private static StringBuilder GetBestMessagesQueryPrefix(Guid managerRoot)
        {
            string str = string.Join("','", new string[] { "Recipient Queued", "Send in Progress", "Invalid Address", "Soft Bounce", "Hard Bounce" });
            StringBuilder builder = new StringBuilder();
            builder.Append("WITH VisitData AS \r\n(\r\n SELECT\r\n  CampaignId AS CampaignId,\r\n  SUM(Value) AS Value,\r\n  COUNT(VisitId) AS Visits\r\n FROM Visits\r\n WHERE\r\n  CampaignId is not null\r\n  and CampaignId != '00000000-0000-0000-0000-000000000000'\r\n GROUP BY CampaignId\r\n),\r\nAutomationData AS \r\n(\r\n SELECT\r\n  a.CampaignId AS CampaignId,\r\n  COUNT(s.AutomationStateId) AS Emails\r\n FROM Automations a JOIN Campaigns c ON a.Campaignid = c.CampaignId ");
            builder.AppendFormat("and c.Data like '{0}%'", managerRoot.ToString("B"));
            builder.Append("\r\n  JOIN AutomationStates s ON a.AutomationId = s.AutomationId\r\n WHERE\r\n  a.Data != ''\r\n");
            builder.AppendFormat(" and s.StateName not in ('{0}')", str);
            builder.Append("\r\nGROUP BY a.CampaignId\r\n),\r\nData AS \r\n(\r\n SELECT\r\n  ad.CampaignId,\r\n  vd.Value,\r\n  CAST(vd.Value AS real)/vd.Visits AS ValuePerVisit,\r\n  CAST(vd.Visits AS real)/ad.Emails AS VisitsPerEmail,\r\n  CAST(vd.Value AS real)/ad.Emails AS ValuePerEmail \r\n FROM VisitData vd right JOIN AutomationData ad ON vd.CampaignId = ad.CampaignId\r\n)\r\n");
            return builder;
        }


        public static List<MessageData> GetAllCampaignsSorted(this AnalyticsDataGateway gateway, Guid managerRoot, string orderBy, SortOrder sortOrder)
        {
            StringBuilder bestMessagesQueryPrefix = AnalyticsSQLDataGatewayExtensions.GetBestMessagesQueryPrefix(managerRoot);
            bestMessagesQueryPrefix.AppendFormat(" SELECT * FROM Data ORDER BY {0} {1}", orderBy, sortOrder);
            return GetAnalyticsSqlCommand().ExecuteCommand<MessageData>(new Func<IDataReader, MessageData>(AnalyticsFactory.Instance.GetAnalyticsDataMapper().GetMessageData), bestMessagesQueryPrefix.ToString(), new CommandParameter[0]);
        }

        private static int GetSqlCommandTimeout()
        {
            return Sitecore.Configuration.Settings.GetIntSetting("AnalyticsSqlDataGateway.CommandTimeout", 90);
        }

        private static AnalyticsSqlCommand GetAnalyticsSqlCommand()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["analytics"].ConnectionString;
            return AnalyticsFactory.Instance.GetAnalyticsSqlCommand(connectionString, GetSqlCommandTimeout());

        }
        
    }
}
