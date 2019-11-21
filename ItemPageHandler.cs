using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;

namespace ConsoleApp1
{
    public class ItemPageHandler : IItemPageHandler
    {
        public Uri CurrentPage
        {
            get;
            private set;
        }

        public string ItemListIndication
        {
            private get;
            set;
        }

        private string GetContent()
        {
            WebRequest request = WebRequest.Create(CurrentPage);
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, true))
            {
                return reader.ReadToEnd();
            }
        }

        public List<Ingredient> GetIngredients(Uri url)
        {
            CurrentPage = url;
            List<Ingredient> ingredients = new List<Ingredient>();
            var content = GetContent();
            int listBegin = content.IndexOf(ItemListIndication);
            int listEnd = content.IndexOf("</ul>", listBegin);
            for (int i = listBegin; i < listEnd;)
            {
                i = content.IndexOf("<li>", i) + 4;
                int end = content.IndexOf("</li>", i);
                ingredients.Add(new Ingredient { Name = content.Substring(i, end - i).Trim() });
            }
            return ingredients;
        }
    }
}
