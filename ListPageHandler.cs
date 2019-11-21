using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace ConsoleApp1
{
    public class ListPageHandler : IListPageHandler
    {
        public Uri CurrentPage
        {
            get
            {
                return NextPageStrategy.CurrentPage;
            }
        }

        public KeyValuePair<string, string> ItemSection
        {
            private get;
            set;
        }

        public IExtractStrategy ItemUriExtractor
        {
            private get;
            set;
        }

        public INextPageStrategy NextPageStrategy
        {
            private get;
            set;
        }

        private string[] GetContent(Uri from)
        {
            HttpWebRequest request = WebRequest.CreateHttp(from);
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var rStream = new StreamReader(stream, true))
            {
                return rStream.ReadToEnd().Split('\n');
            }
        }

        public Dictionary<Uri, Dish> GetLinksToItems(Uri from)
        {
            var content = GetContent(from);
            return content
                .Where(line => line.Contains(ItemSection.Key) && line.Contains("a href"))
                .ToDictionary(line => ItemUriExtractor.ExtractUri(line), 
                line =>
                {
                    int begin = line.IndexOf('>', line.IndexOf("<a href=") + 6) + 1;
                    int end = line.IndexOf("</a>", begin - 1);
                    return new Dish { Name = line.Substring(begin, end - begin) };
                });
        }

        public Uri NextListPage()
        {
            return NextPageStrategy.NextPage();
        }
    }
}
