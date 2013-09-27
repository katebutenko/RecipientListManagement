using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipientListManagement.RecipientsLists.Lists
{
    public class RecipientListInfo
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public int Count { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
