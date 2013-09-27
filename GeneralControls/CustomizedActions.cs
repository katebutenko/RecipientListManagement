using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using System;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using Sitecore.Speak.Utils;
    

namespace RecipientListManagement.GeneralControls
{
    public class CustomizedActions : Sitecore.Speak.Web.UI.WebControls.Actions
    {

        public CustomizedActions(string id, string tag, string cssClass)
            : base(id, tag, cssClass)
        {
            
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EventHandler<RequireModelEventArgs> handler = (o, ea) => ea.SetModel<object>(this.DataModel);
            base.menu.RequireModel += handler;
        }

        public object DataModel { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }
    }
}
