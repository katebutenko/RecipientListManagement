using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Security.Accounts;
using Sitecore.Modules.EmailCampaign;
using RecipientListManagement.RecipientsLists.Lists;
using Sitecore;
using Sitecore.EmailCampaign.Domain;
using Sitecore.EmailCampaign.Data;
using Sitecore.Security;
using System.Web.Profile;
using System.Configuration;

namespace RecipientListManagement.RecipientsLists
{
    public class UsersInListRepository
    {
        public class Visitor
        {

            public string Email { get; set; }

            public DateTime EntryDateTime { get; set; }

            public string FullName { get; set; }
            public string UserName { get; set; }
            public string StateName { get; set; }
            public string Key { get; set; }
            public int NumberOfClicks { get; set; }

        }

        private readonly FilterBase filter;
        private readonly SpeakExpressionParserBase speakExpressionParser;

        public UsersInListRepository()
            : this(new Filter(), new SpeakExpressionParser())
        {
        }

        public UsersInListRepository(FilterBase filter, SpeakExpressionParserBase speakExpressionParser)
        {
            this.filter = filter;
            this.speakExpressionParser = speakExpressionParser;
        }

        public List<Visitor> GetUsersFromList(string recipientListId,bool subscribed)
        {
            //get target audience
            //get users in it
            //foreach username get user
            List<Visitor> userlist = new List<Visitor>();
            if (recipientListId != "defaultParameter")
            {
                TargetAudience recipientList = Factory.GetTargetAudience(recipientListId);
                if (recipientList != null)
                {
                    //ContactList optInContactList = recipientList.OptInList;
                    List<string> subscriberNames;
                    if (subscribed)
                    {
                        subscriberNames = recipientList.SubscribersNames;
                    }
                    else
                    {
                        subscriberNames = recipientList.UnsubscribersNames;
                    }

                    foreach (string userName in subscriberNames)
                    {
                        Contact contact = Contact.FromName(userName);
                        if (contact != null)
                        {
                            string rowId = userName;//check if it needs to be an Email
                            rowId = rowId.Replace("@", "__at__").Replace(".", "__dot__").Replace("\\", "__slash__");
                            Visitor item = new Visitor
                            {
                                Email = contact.Profile.Email,
                                FullName = contact.Profile.FullName,
                                Key = rowId
                            };
                            userlist.Add(item);
                        }
                    }
                }
                return userlist;
            }
            return userlist;
        }
                      
        public List<Visitor> GetUsers(string expression)
        {
            RecipientListsObjectDetailList recipientListsControl = (RecipientListsObjectDetailList)UIUtil.FindControlByType(typeof(RecipientListsObjectDetailList));
            if (recipientListsControl != null)
            {
                string recipientListId = recipientListsControl.LastSelectedRow;
                if (string.IsNullOrEmpty(expression))
                {
                    return this.GetUsersFromList(recipientListId,true);
                }
                expression = this.speakExpressionParser.Parse("o", null, expression);
                string[] referencedAssemblies = new string[] { typeof(Visitor).Assembly.Location, typeof(UserProfile).Assembly.Location, typeof(ProfileBase).Assembly.Location, typeof(SettingsBase).Assembly.Location, typeof(IQueryable).Assembly.Location };
                return this.filter.ApplyFilter<Visitor>(this.GetUsersFromList(recipientListId,true), expression, referencedAssemblies).ToList<Visitor>();

            }
            return new List<Visitor>();
        }
        public List<Visitor> GetOptOutUsers(string expression)
        {
            RecipientListsObjectDetailList recipientListsControl = (RecipientListsObjectDetailList)UIUtil.FindControlByType(typeof(RecipientListsObjectDetailList));
            if (recipientListsControl != null)
            {
                string recipientListId = recipientListsControl.LastSelectedRow;
                if (string.IsNullOrEmpty(expression))
                {
                    return this.GetUsersFromList(recipientListId,false);
                }
                expression = this.speakExpressionParser.Parse("o", null, expression);
                string[] referencedAssemblies = new string[] { typeof(Visitor).Assembly.Location, typeof(UserProfile).Assembly.Location, typeof(ProfileBase).Assembly.Location, typeof(SettingsBase).Assembly.Location, typeof(IQueryable).Assembly.Location };
                return this.filter.ApplyFilter<Visitor>(this.GetUsersFromList(recipientListId,false), expression, referencedAssemblies).ToList<Visitor>();

            }
            return new List<Visitor>();
        }
    
        
    }
}
