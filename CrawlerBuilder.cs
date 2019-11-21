using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;

namespace ConsoleApp1
{
    public class CrawlerBuilder : ICrawlerBuilder
    {
        private string[] _pageContent;
        private Uri _currentPage;

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

        private IEnumerable<string> FilterFor(IEnumerable<string> content, string[] hunted)
        {
            return content.Where(line => hunted.Any(huntedW => line.Contains(huntedW)));
        }

        private IEnumerable<string> FilterForExcept(IEnumerable<string> content, string[] hunted, string[] except)
        {
            return content.Where(line => 
            hunted.Any(huntedW => line.Contains(huntedW)) && !except.Any(e => line.Contains(e)));
        }

        private IExtractStrategy GetItemExtractStrategyFor(string section)
        {
            if (section.Contains("www."))
            {
                return new AbsoluteItemUriExtractor();
            }
            int www = _currentPage.AbsoluteUri.IndexOf("www");
            int end = _currentPage.AbsoluteUri.IndexOf('/', www);
            return new PostFixItemUriExtractor(_currentPage.AbsoluteUri
                .Substring(0, end));
        }

        private void ApplyItemSections(IEnumerable<string> sections, string[] testedDishNames, ListPageHandler handler)
        {
            foreach (var section in sections)
            {
                int begin = section.IndexOf("<a href=");
                int betweenBegin = section.IndexOf('>', begin + 5);
                int betweenEnd = section.IndexOf('<', betweenBegin);
                int end = section.IndexOf("</a>", betweenEnd - 1) + 4;
                var dishName = section.Substring(betweenBegin + 1, betweenEnd - betweenBegin - 1);
                if (testedDishNames.Any(w => dishName.Contains(w)))
                {
                    var preceedingSectionBegin = section.LastIndexOf('<', begin - 1);
                    string sectionBegin = section.Substring(0, begin);
                    var preceedingSectionEnd = section.IndexOf('>', end + 1);

                    string sectionEnd = section.Substring(end);
                    handler.ItemSection = new KeyValuePair<string, string>(sectionBegin, sectionEnd);
                    handler.ItemUriExtractor = GetItemExtractStrategyFor(section.Substring(begin, betweenBegin - begin));
                    // So far the engine sucks a little and misses cases, can't put all the stake on one miss, so so far all hrefs are acceptable
                    // when engine improves add return; after first match
                }
            }
        }

        private void ConfigureItemSections(IEnumerable<string> contentInLowerCase, ListPageHandler handler)
        {
            string[] testedDishNames = new string[] { "kurczak", "gulasz", "makaron", "filet" };
            string[] bannedNames = new string[] { };
            var possibleSections = FilterForExcept(contentInLowerCase, testedDishNames, bannedNames);
            var morePossibleSections = FilterFor(possibleSections, new string[]
                { "<a href"});
            ApplyItemSections(morePossibleSections, testedDishNames, handler);
        }

        private void ConfigureNextPageSwitch(ListPageHandler handler)
        {
            var pageSwitch = new InceremntPageStrategy(_currentPage);
            var page = pageSwitch.NextPage();
            try
            {
                GetContent(page);
                handler.NextPageStrategy = pageSwitch;
            }
            catch(WebException)
            {
                handler.NextPageStrategy = new SearchContentForUriStrategy();
            }
        }

        public IListPageHandler CreateForList(Uri firstListPage)
        {
            _currentPage = firstListPage;
            _pageContent = GetContent(firstListPage);
            ListPageHandler handler = new ListPageHandler();
            ConfigureItemSections(_pageContent.Select(l => l.ToLower()), handler);
            ConfigureNextPageSwitch(handler);
            return handler;
        }

        private string SearchForIngredientListIndication(string[] lines)
        {
            IEnumerable<string> section = lines;
            int offSet = 5;
            int tolerationCount = 3;
            while (section.Count() > offSet)
            {
                section = section.SkipWhile(line => !line.ToLower().Contains("skladniki"));
                if (section.Take(offSet).Any(line => line.Contains("<ul>")))
                {
                    return section.FirstOrDefault();
                }
                section = section.Skip(offSet);
                if (section.Count() < offSet)
                {
                    offSet += 3;
                    tolerationCount--;
                    if (tolerationCount == 0)
                    {
                        return string.Empty;
                    }
                }
            }
            return null;
        }

        public IItemPageHandler CreateForItem(Uri examplePage)
        {
            _currentPage = examplePage;
            _pageContent = GetContent(examplePage);
            ItemPageHandler handler = new ItemPageHandler();
            handler.ItemListIndication = SearchForIngredientListIndication(_pageContent);
            return handler;
        }
    }
}
