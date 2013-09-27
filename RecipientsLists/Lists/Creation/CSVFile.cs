using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign;
using Sitecore.IO;
using Sitecore.Modules.EmailCampaign.Exceptions;
using Sitecore.StringExtensions;

namespace RecipientListManagement.CSVExport
{
    public class CSVFile : IDisposable
    {
        // Fields
        private StreamReader reader;

        // Methods
        public CSVFile(string filename)
        {
            Assert.ArgumentNotNull(filename, "filename");
            if (!"csv".Equals(FileUtil.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
            {
                throw new EmailCampaignException("'{0}' is not a CSV file!".FormatWith(new object[] { FileUtil.GetFileName(filename) }));
            }
            this.reader = new StreamReader(filename);
            Util.AssertNotNull(this.reader);
        }

        public void Dispose()
        {
            if (this.reader != null)
            {
                this.reader.Close();
            }
        }

        ~ CSVFile()
        {
            this.Dispose();
        }

        public List<string> ReadLine()
        {
            List<string> list = new List<string>();
            if ((this.reader == null) || this.reader.EndOfStream)
            {
                return null;
            }
            this.ReadToList(list, string.Empty, 0);
            return list;
        }

        private void ReadToList(List<string> list, string rest, int quote)
        {
            if (this.reader != null)
            {
                int num3;
                int startIndex = 0;
                string str = this.reader.ReadLine();
                if (!string.IsNullOrEmpty(str))
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (str[i])
                        {
                            case '"':
                                {
                                    quote = (quote == 2) ? 1 : (quote + 1);
                                    continue;
                                }
                            case ',':
                            case ';':
                                if ((quote != 0) && (quote != 2))
                                {
                                    continue;
                                }
                                num3 = i - 1;
                                if (!string.IsNullOrEmpty(rest))
                                {
                                    break;
                                }
                                if (quote == 2)
                                {
                                    startIndex++;
                                    num3--;
                                }
                                list.Add(str.Substring(startIndex, (num3 - startIndex) + 1).Replace("\"\"", "\""));
                                goto Label_012E;

                            default:
                                {
                                    continue;
                                }
                        }
                        string str2 = rest + str.Substring(startIndex, (num3 - startIndex) + 1);
                        if (quote == 2)
                        {
                            str2 = str2.Substring(1, str2.Length - 2);
                        }
                        list.Add(str2.Replace("\"\"", "\""));
                        rest = string.Empty;
                    Label_012E:
                        quote = 0;
                        startIndex = i + 1;
                    }
                }
                if (quote != 1)
                {
                    num3 = str.Length - 1;
                    if (quote == 2)
                    {
                        startIndex++;
                        num3--;
                    }
                    list.Add(str.Substring(startIndex, (num3 - startIndex) + 1).Replace("\"\"", "\""));
                }
                else
                {
                    this.ReadToList(list, rest + str.Substring(startIndex, str.Length - startIndex) + "\r\n", quote);
                }
            }
        }
    }

 

}
