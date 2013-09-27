using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipientListManagement.RecipientsLists.Lists
{
    public class MessageEventArgs: EventArgs
    {
        public string Message { get; set; }
        public string[] Parameters { get; set; }

        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
