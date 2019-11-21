using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class InceremntPageStrategy : INextPageStrategy
    {
        public Uri CurrentPage
        {
            get;
            private set;
        }

        public Uri NextPage()
        {
            var str = CurrentPage.OriginalString;
            var pageIndication = str.IndexOf("page=");
            var numAsStr = new string(str
                .Skip(pageIndication + 5)
                .TakeWhile(c => char.IsDigit(c))
                .ToArray());
            if (int.TryParse(numAsStr, out int currentNumber))
            {
                return CurrentPage = new Uri(str.Replace(numAsStr, (currentNumber + 1).ToString()));
            }
            return CurrentPage = null;
        }

        public InceremntPageStrategy(Uri start)
        {
            CurrentPage = start;
        }
    }

    public class SearchContentForUriStrategy : INextPageStrategy
    {
        public Uri CurrentPage
        {
            get;
            private set;
        }

        public Uri NextPage()
        {
            throw new NotImplementedException();
        }
    }
}
