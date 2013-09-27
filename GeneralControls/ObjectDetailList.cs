using Sitecore.Data.Items;
using Sitecore.Web.UI.WebControls;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace RecipientListManagement.GeneralControls
{
    public class ObjectDetailList : CustomizedDetailList<ObjectDataSource>
    {
        private string[] dataKeyNames;
        private string[] selectParameternames;

        public ObjectDetailList()
            : base(new ObjectDataSource())
        {
        }

        protected override void InitializeDataSourceControl()
        {
            if (!string.IsNullOrEmpty(this.SelectMethod))
            {
                this.InitializeDataSourceControlSelectMethod();
            }
        }

        protected virtual void InitializeDataSourceControlSelectMethod()
        {
            base.dataSourceControl.SelectMethod = this.SelectMethod;
            if (!string.IsNullOrEmpty(this.DataSourceItem["SelectParameterName"]))
            {
                this.SelectParameterNames = Regex.Replace(this.DataSourceItem["SelectParameterName"], @"\s", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            bool flag = (this.DataSourceItem["EnablePaging"] != null) && (this.DataSourceItem["EnablePaging"] == "1");
            string str = this.DataSourceItem["StartRowIndexParameterName"];
            string str2 = this.DataSourceItem["MaximumRowsParameterName"];
            if ((flag && !string.IsNullOrEmpty(str)) && !string.IsNullOrEmpty(str2))
            {
                base.dataSourceControl.EnablePaging = true;
                base.dataSourceControl.StartRowIndexParameterName = str;
                base.dataSourceControl.MaximumRowsParameterName = str2;
            }
            string[] source = Enumerable.Empty<string>().ToArray<string>();
            if (!string.IsNullOrEmpty(this.DataSourceItem["SelectParameterValue"]))
            {
                source = (string[])(this.DataSourceItem["SelectParameterValue"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>().ToArray<string>()).Clone();
            }
            if ((this.ParameterCollection != null) && this.ParameterCollection.HasKeys())
            {
                if ((source != null) && !string.IsNullOrEmpty(source.FirstOrDefault<string>(i => !string.IsNullOrEmpty(i))))
                {
                    for (int k = 0; k < this.SelectParameterNames.Count<string>(); k++)
                    {
                        string str3 = string.Empty;
                        foreach (string str4 in this.DataKeyNames)
                        {
                            if ((source != null) && (this.ParameterCollection.GetValues(str4) != null))
                            {
                                str3 = source.FirstOrDefault<string>(j => !string.IsNullOrEmpty(j)).Replace("$" + str4, this.ParameterCollection.GetValues(str4).FirstOrDefault<string>(j => !string.IsNullOrEmpty(j)));
                            }
                        }
                        if (string.IsNullOrEmpty(str3))
                        {
                            foreach (string str5 in source)
                            {
                                string str6 = str5.TrimStart(new char[0]).TrimEnd(new char[0]).Replace("$", string.Empty);
                                if (!string.IsNullOrEmpty(str6) && (this.ParameterCollection.GetValues(str6) != null))
                                {
                                    string str7 = this.ParameterCollection.GetValues(str6).FirstOrDefault<string>(j => !string.IsNullOrEmpty(j));
                                    if (!string.IsNullOrEmpty(str7))
                                    {
                                        str3 = source.FirstOrDefault<string>(j => !string.IsNullOrEmpty(j)).Replace("$" + str6, str7);
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(str3) && (source != null))
                        {
                            str3 = source.FirstOrDefault<string>(j => !string.IsNullOrEmpty(j));
                        }
                        if (!string.IsNullOrEmpty(str3))
                        {
                            if (string.Compare(source[k].TrimStart(new char[0]).TrimEnd(new char[0]).Replace("$", string.Empty).ToLower(), "expression", StringComparison.InvariantCulture) == 0)
                            {
                                if (base.filterBuidler.EnableFiltering || base.filterBuidler.EnableSearching)
                                {
                                    ControlParameter parameter = new ControlParameter(this.SelectParameterNames[k], base.filterBuidler.ID, "Value")
                                    {
                                        DefaultValue = string.Empty
                                    };
                                    base.dataSourceControl.SelectParameters.Add(parameter);
                                }
                            }
                            else
                            {
                                Parameter parameter2 = new Parameter(this.SelectParameterNames[k])
                                {
                                    DefaultValue = str3
                                };
                                base.dataSourceControl.SelectParameters.Add(parameter2);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string str8 in this.SelectParameterNames)
                    {
                        if ((this.dataKeyNames != null) && !string.IsNullOrEmpty(this.dataKeyNames.FirstOrDefault<string>(i => !string.IsNullOrEmpty(i))))
                        {
                            foreach (string str9 in this.DataKeyNames)
                            {
                                if (this.ParameterCollection.GetValues(str9) != null)
                                {
                                    string str10 = this.ParameterCollection.GetValues(str9).FirstOrDefault<string>(i => !string.IsNullOrEmpty(i));
                                    if (!string.IsNullOrEmpty(str10))
                                    {
                                        Parameter parameter3 = new Parameter(str8)
                                        {
                                            DefaultValue = str10
                                        };
                                        base.dataSourceControl.SelectParameters.Add(parameter3);
                                    }
                                }
                            }
                            continue;
                        }
                        if (source != null)
                        {
                            string str11 = source.FirstOrDefault<string>(i => !string.IsNullOrEmpty(i));
                            if (!string.IsNullOrEmpty(str11))
                            {
                                Parameter parameter4 = new Parameter(str8)
                                {
                                    DefaultValue = str11
                                };
                                base.dataSourceControl.SelectParameters.Add(parameter4);
                            }
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(source.FirstOrDefault<string>(i => !string.IsNullOrEmpty(i))))
            {
                if (this.SelectParameterNames.Count<string>() == source.Count<string>())
                {
                    for (int m = 0; m < this.SelectParameterNames.Count<string>(); m++)
                    {
                        string str12 = this.SelectParameterNames[m];
                        string str13 = source[m].TrimStart(new char[0]).TrimEnd(new char[0]).Replace("$", string.Empty);
                        if (!string.IsNullOrEmpty(str12) && !string.IsNullOrEmpty(str13))
                        {
                            if (string.Compare(str13.ToLower(), "expression", StringComparison.InvariantCulture) == 0)
                            {
                                if (base.filterBuidler.EnableFiltering || base.filterBuidler.EnableSearching)
                                {
                                    ControlParameter parameter5 = new ControlParameter(str12, base.filterBuidler.ID, "Value")
                                    {
                                        DefaultValue = string.Empty
                                    };
                                    base.dataSourceControl.SelectParameters.Add(parameter5);
                                }
                            }
                            else
                            {
                                Parameter parameter6 = new Parameter(str12)
                                {
                                    DefaultValue = str13
                                };
                                base.dataSourceControl.SelectParameters.Add(parameter6);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string str14 in this.SelectParameterNames)
                    {
                        string str15 = string.Empty;
                        if (string.IsNullOrEmpty(str15))
                        {
                            str15 = source.FirstOrDefault<string>(i => !string.IsNullOrEmpty(i));
                        }
                        if (!string.IsNullOrEmpty(str15))
                        {
                            Parameter parameter7 = new Parameter(str14)
                            {
                                DefaultValue = str15
                            };
                            base.dataSourceControl.SelectParameters.Add(parameter7);
                        }
                    }
                }
            }
        }

        protected override void InitializeFilterBuilder()
        {
            Func<Item, FilterField> selector = null;
            Func<Item, Operator> func2 = null;
            base.InitializeFilterBuilder();
            if (base.filterBuidler.EnableFiltering || base.filterBuidler.EnableSearching)
            {
                if (selector == null)
                {
                    selector = i => this.GetFilterField(i);
                }
                base.filterBuidler.Fields.Add(this.DataSourceItem.Children.Where<Item>(delegate(Item i)
                {
                    if (!string.IsNullOrEmpty(i["Hidden"]))
                    {
                        return !(i["Hidden"] == "1");
                    }
                    return true;
                }).Select<Item, FilterField>(selector));
                if (func2 == null)
                {
                    func2 = o => this.GetFilterOperator(o);
                }
                base.filterBuidler.Operators.Add(new ObjectDataSourcePredicateSerializer().OperatorsRoot.Children.Select<Item, Operator>(func2));
                base.filterBuidler.PredicateSerializer = typeof(ObjectDataSourcePredicateSerializer);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.DataSourceItem["TypeName"]))
            {
                base.dataSourceControl.TypeName = this.DataSourceItem["TypeName"];
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["DataObjectTypeName"]))
            {
                base.dataSourceControl.DataObjectTypeName = this.DataSourceItem["DataObjectTypeName"];
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["DataKeyNames"]))
            {
                this.DataKeyNames = new string[] { this.DataSourceItem["DataKeyNames"] };
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["SelectMethod"]))
            {
                this.SelectMethod = this.DataSourceItem["SelectMethod"];
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["UpdateMethod"]))
            {
                this.UpdateMethod = this.DataSourceItem["UpdateMethod"];
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["DeleteMethod"]))
            {
                this.DeleteMethod = this.DataSourceItem["DeleteMethod"];
            }
            if (!string.IsNullOrEmpty(this.DataSourceItem["InsertMethod"]))
            {
                this.InsertMethod = this.DataSourceItem["InsertMethod"];
            }
            if (this.Page is PopupPage)
            {
                PopupPage page = (PopupPage)this.Page;
                if (page.Parameters.HasKeys())
                {
                    this.ParameterCollection = page.Parameters;
                }
            }
            else if (this.Page.Request.QueryString.HasKeys())
            {
                this.ParameterCollection = this.Page.Request.QueryString;
            }
            base.OnInit(e);
        }

        protected virtual string[] DataKeyNames
        {
            get
            {
                return (this.dataKeyNames ?? Enumerable.Empty<string>().ToArray<string>());
            }
            set
            {
                this.dataKeyNames = (string[])(value ?? Enumerable.Empty<string>().ToArray<string>()).Clone();
            }
        }

        protected virtual string DeleteMethod { get; set; }

        public string DeleteParameterName { get; set; }

        protected virtual string InsertMethod { get; set; }

        public override DetailList List
        {
            get
            {
                return base.detailList;
            }
        }

        protected NameValueCollection ParameterCollection { get; set; }

        public string Selected { get; set; }

        protected virtual string SelectMethod { get; set; }

        public string SelectParameterName { get; set; }

        protected virtual string[] SelectParameterNames
        {
            get
            {
                return (this.selectParameternames ?? Enumerable.Empty<string>().ToArray<string>());
            }
            set
            {
                this.selectParameternames = (string[])(value ?? Enumerable.Empty<string>().ToArray<string>()).Clone();
            }
        }

        protected string UpdateKey { get; set; }

        protected virtual string UpdateMethod { get; set; }
    }
}
