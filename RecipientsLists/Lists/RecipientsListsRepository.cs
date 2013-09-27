using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecipientListManagement.RecipientsLists.Lists;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.EmailCampaign.Domain;
using Sitecore.EmailCampaign.Data;
using Sitecore.Security;
using System.Web.Profile;
using System.Configuration;
using System.Linq;

namespace RecipientListManagement.RecipientsLists
{
    public class RecipientsListsRepository
    {
         private readonly FilterBase filter;
        private readonly SpeakExpressionParserBase speakExpressionParser;

        public RecipientsListsRepository()
            : this(new Filter(), new SpeakExpressionParser())
        {
        }

        public RecipientsListsRepository(FilterBase filter, SpeakExpressionParserBase speakExpressionParser)
        {
            this.filter = filter;
            this.speakExpressionParser = speakExpressionParser;
        }
        public List<RecipientListInfo> GetAllRecipientLists(string expression)
        {
                if (string.IsNullOrEmpty(expression))
                {
                    return this.GetAllRecipientLists();
                }
                expression = this.speakExpressionParser.Parse("o", null, expression);
                string[] referencedAssemblies = new string[] { typeof(RecipientListInfo).Assembly.Location, typeof(UserProfile).Assembly.Location, typeof(ProfileBase).Assembly.Location, typeof(SettingsBase).Assembly.Location, typeof(IQueryable).Assembly.Location };
                return this.filter.ApplyFilter<RecipientListInfo>(this.GetAllRecipientLists(), expression, referencedAssemblies).ToList<RecipientListInfo>();

        }
        public List<RecipientListInfo> GetAllRecipientLists()
        {
            ManagerRoot root = UIFactory.Instance.GetSpeakContext().ManagerRoot;
            List<RecipientListInfo> list = new List<RecipientListInfo>();
            if (root != null)
            {
                List<TargetAudience> taList = root.GetTargetAudiences();
                foreach (TargetAudience ta in taList)
                {
                    string owner = ta.InnerItem.Statistics.CreatedBy;
                    if (String.IsNullOrEmpty(owner))
                    {
                        owner = ta.InnerItem.Statistics.CreatedBy;
                    }
                    if (String.IsNullOrEmpty(owner))
                    {
                        owner = "Admin/Unknown";
                    }
                    if (CurrentUserIsOwner(owner))
                    {
                        list.Add(new RecipientListInfo()
                        {
                            Key = ta.ID,
                            Name = ta.Name,
                            Owner = owner,
                            Count = ta.SubscribersNames.Count,
                            CreatedDate = ta.InnerItem.Statistics.Created
                        });
                    }
                }
            }
            return list;
        }

        private bool CurrentUserIsOwner(string owner)
        {
            string currentUserName = Sitecore.Context.User.Name;
            if (currentUserName.ToLowerInvariant() == owner.ToLowerInvariant() || Sitecore.Context.User.IsAdministrator)
            {
                return true;
            }
            return false;
        }
    }
}
