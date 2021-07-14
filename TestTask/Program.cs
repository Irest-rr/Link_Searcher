using System;

namespace TestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = string.Empty;
            while (true)
            {

                Console.WriteLine("Input URL");
                address = Console.ReadLine();
                if (Uri.IsWellFormedUriString(address, UriKind.Absolute) && address[address.Length - 1] == '/')
                {
                    try { address.GetUrl(); }
                    catch
                    {
                        Console.WriteLine("Incorrect URl");
                        continue;
                    }
                    break;
                }
                else
                    Console.WriteLine("Incorrect URl");
                Console.WriteLine("=========");
            }


            UrlSearcher urlSearcher = new UrlSearcher(address);


            urlSearcher.CrawlingWebSite();

            if (urlSearcher.SiteMapExist())
            {
                urlSearcher.ReadSiteMap();
                urlSearcher.CombineLists();
                urlSearcher.UniqeSiteMapUrl();
                urlSearcher.UniqeCrawledUrl();
            }

            urlSearcher.Timing();




        }
    }
}
