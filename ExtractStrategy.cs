using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    abstract class ItemUriExtractor : IExtractStrategy
    {
        public abstract Uri ExtractUri(string from);

        protected string GetHrefText(string from)
        {
            var begin = from.IndexOf("href=") + 6;
            var end = from.IndexOf("\"", begin + 1);
            return from.Substring(begin, end - begin);
        }
    }

    class PostFixItemUriExtractor : ItemUriExtractor
    {
        private readonly string _prefix;

        public override Uri ExtractUri(string from)
        {
            return new Uri(_prefix + GetHrefText(from));
        }

        public PostFixItemUriExtractor(string prefix)
        {
            _prefix = prefix;
        }
    }

    class AbsoluteItemUriExtractor : ItemUriExtractor
    {
        public override Uri ExtractUri(string from)
        {
            return new Uri(GetHrefText(from));
        }
    }
}
