using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace RecipientListManagement.LimitedUserView
{
    public class MessageStateRepository
    {      
        private bool messageSentByContextUser(MessageStateInfo messageInfo)
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

        // Fields
        private readonly MessageStateInfoTypeConverter converter;
        private readonly ManagerRoot managerRoot;

        // Methods
        public MessageStateRepository()
            : this(UIFactory.Instance.GetSpeakContext().ManagerRoot, new MessageStateInfoTypeConverter())
        {
        }

        public MessageStateRepository(ManagerRoot managerRoot, MessageStateInfoTypeConverter converter)
        {
            Assert.ArgumentNotNull(managerRoot, "managerRoot");
            Assert.ArgumentNotNull(converter, "converter");
            this.managerRoot = managerRoot;
            this.converter = converter;
        }

        public IEnumerable<MessageStateInfo> GetMessageStateInfo(string query, string sortArgument)
        {
            return this.GetMessageStateInfo(query, null, null, null, sortArgument);
        }

        public IEnumerable<MessageStateInfo> GetMessageStateInfo(string query, string expression, string sortArgument)
        {
            return this.GetMessageStateInfo(query, null, null, expression, sortArgument);
        }

        public IEnumerable<MessageStateInfo> GetMessageStateInfo(string query, string underlyingDataSourceSortArgument, int listSize, string sortArgument)
        {
            return this.GetMessageStateInfo(query, underlyingDataSourceSortArgument, new int?(listSize), null, sortArgument);
        }

        private IEnumerable<MessageStateInfo> GetMessageStateInfo(string query, string underlyingDataSourceSortArgument, int? listSize, string expression, string sortArgument)
        {
            Func<MessageStateInfo, object> keySelector = null;
            Func<MessageStateInfo, object> func2 = null;
            Assert.ArgumentNotNull(query, "query");
            string newValue = string.Format("*[@@id='{0}']", this.managerRoot.InnerItem.ID);
            if (string.IsNullOrEmpty(expression))
            {
                expression = "true";
            }
            FastItemReader reader2 = new FastItemReader(this.managerRoot.InnerItem.Database.Name, query.Replace("{messages}", newValue).Replace("{expression}", expression));
            reader2.SortExpression = underlyingDataSourceSortArgument;
            FastItemReader source = reader2;

            Func<Item, bool> funcIfVisible;
            funcIfVisible = delegate(Item i)
            {
                return (Sitecore.Context.User.IsAdministrator) || (i.Statistics.CreatedBy.ToLowerInvariant() == Sitecore.Context.User.Name.ToLowerInvariant());
            };

            IEnumerable<MessageStateInfo> enumerable = source.Take<Item>((listSize.HasValue ? listSize.Value : source.Count<Item>()))
                                                        .Where<Item>(funcIfVisible)
                                                        .Select<Item, object>(new Func<Item, object>(this.converter.ConvertFrom))
                                                        .Cast<MessageStateInfo>();
            if (string.IsNullOrEmpty(sortArgument))
            {
                return enumerable;
            }
            string asc = " ASC";
            if (sortArgument.EndsWith(asc))
            {
                if (keySelector == null)
                {
                    keySelector = delegate(MessageStateInfo info)
                    {
                        return DataBinder.Eval(info, sortArgument.Substring(0, sortArgument.Length - asc.Length));
                    };
                }
                return enumerable.OrderBy<MessageStateInfo, object>(keySelector);
            }
            string desc = " DESC";
            if (!sortArgument.EndsWith(desc))
            {
                throw new ArgumentException("Only 'ASC' and 'DESC' string endings can be used to specify sort order.");
            }
            if (func2 == null)
            {
                func2 = delegate(MessageStateInfo info)
                {
                    return DataBinder.Eval(info, sortArgument.Substring(0, sortArgument.Length - desc.Length));
                };
            }
            return enumerable.OrderByDescending<MessageStateInfo, object>(func2);
        }



    }
}
