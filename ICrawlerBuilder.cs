using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Ingredient
    {
        public string Name
        {
            get;
            set;
        }
    }

    public class Dish
    {
        public string Name
        {
            get;
            set;
        }

        public List<Ingredient> Ingredients
        {
            get;
            set;
        }
    }

    public interface INextPageStrategy
    {
        Uri CurrentPage
        {
            get;
        }

        Uri NextPage();
    }

    public interface IExtractStrategy
    {
        Uri ExtractUri(string from);
    }

    public interface IListPageHandler
    {
        Uri CurrentPage
        {
            get;
        }

        Dictionary<Uri, Dish> GetLinksToItems(Uri from);

        Uri NextListPage();
    }

    public interface IItemPageHandler
    {
        Uri CurrentPage
        {
            get;
        }

        List<Ingredient> GetIngredients(Uri from);
    }

    public interface ICrawlerBuilder
    {
        IListPageHandler CreateForList(Uri firstListPage);

        IItemPageHandler CreateForItem(Uri examplePage);
    }
}
