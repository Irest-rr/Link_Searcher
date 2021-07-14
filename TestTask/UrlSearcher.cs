using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace TestTask
{
    class UrlSearcher
    {
        private string Address { get; set; }

        private SortedDictionary<string, long> urlsByCrawling = new SortedDictionary<string, long>();
        private SortedDictionary<string, long> urlFromSiteMap = new SortedDictionary<string, long>();


        private List<IHtmlCollection<IElement>> needDownload = new List<IHtmlCollection<IElement>>();


        private List<string> allUrl = new List<string>();

        private string Domain { get; set; }
        private string Protocol { get; set; }
        private string Path { get; set; }


        public UrlSearcher(string address)
        {
            Address = address;
            SetProtocol();
            SetDomain();
            SetPath();
        }

        private void SetProtocol()
        {
            Protocol = Address.Substring(0, Address.IndexOf(":")+1);
        }

        private void SetDomain()
        {
            var uri = new Uri(Address);
            Domain = uri.Authority;
        }

        private void SetPath()
        {
            Path = Address.Substring(Address.IndexOf(":") + 3, Address.Length - (Address.IndexOf("://") + 4));
        }

        public void CrawlingWebSite() 
        {
            Console.WriteLine("Page crawling to find links");
            var parser = new HtmlParser();
            var document = GetAndPutPageInfo(urlsByCrawling, Address);
            needDownload.Add(parser.ParseDocument(document).QuerySelectorAll("a"));


            while (needDownload.Count != 0)
            {
                foreach (var link in needDownload[0])
                {
                    var url = link.GetAttribute("href");
                    if (UrlIsOk(ref url))
                    {
                        Console.WriteLine(url);
                    }
                }
                needDownload.RemoveAt(0);
            }
        }

        private bool UrlIsOk(ref string url)
        {
            var parser = new HtmlParser();
            if (url != null && url != "" && url[0] != '#')
            {
                if (!urlsByCrawling.Keys.Contains(url) && url.Contains(Domain))
                {
                    try
                    {
                        var document = GetAndPutPageInfo(urlsByCrawling, url);
                        needDownload.Add(parser.ParseDocument(document).QuerySelectorAll("a"));
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (!url.Contains(Domain) || !url.Contains(Protocol))
                    {
                        if (url.Contains("//"))
                            url = Protocol + url;
                        if (url[0] == '/')
                            url = Protocol + "//" + Path + url;
                        else
                            url = Protocol + "//" + Path + "/" + url;
                        if (!urlsByCrawling.Keys.Contains(url) && url.Contains(Domain))
                        {
                            try
                            {
                                var document = GetAndPutPageInfo(urlsByCrawling, url);
                                needDownload.Add(parser.ParseDocument(document).QuerySelectorAll("a"));
                            }
                            catch
                            {
                                return false;
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            else return false;
        }

        private string GetAndPutPageInfo(SortedDictionary<string, long> urlAndTime ,string url)
        {
            var sw = Stopwatch.StartNew();
            var documentString = url.GetUrl();
            sw.Stop();
            urlAndTime.Add(url, sw.ElapsedMilliseconds);
            return documentString;
        }

        public bool SiteMapExist()
        {
            Uri uri = new Uri(Address + "sitemap.xml");
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch { return false; }
            return true;
        }


        public void ReadSiteMap()   // read data from sitemap.xml
        {
            Console.WriteLine("Urls FOUNDED IN SITEMAP.XML");
            var doc = XDocument.Load(Address + "sitemap.xml");
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var locs = doc.Descendants(ns + "loc").Select(e => e.Value).ToList();

            for (int i = 0; i < locs.Count; i++)
            {
                if (locs[i].Contains(".xml"))
                {
                    doc = XDocument.Load(locs[i]);
                    locs.AddRange(doc.Descendants(ns + "loc").Select(e => e.Value).ToList());
                    locs.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (!urlFromSiteMap.Keys.Contains(locs[i]))
                    {
                        GetAndPutPageInfo(urlFromSiteMap, locs[i]);
                    }
                    Console.WriteLine(locs[i]);
                    locs.RemoveAt(i);
                    i--;
                }
            }
           
        }


        public void CombineLists()  // Combine urls from sitemap.xml and url what we crowled
        {
            allUrl = urlsByCrawling.Keys.Union(urlFromSiteMap.Keys).ToList();
        }


        public void UniqeSiteMapUrl()     //Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site
        {
            Console.WriteLine("Uniqe Urls FOUNDED IN SITEMAP.XML");
            var uniqeSiteMapUrl  = urlFromSiteMap.Keys.Except(urlsByCrawling.Keys).ToList();
            if (uniqeSiteMapUrl.Any())
            {
                for (int i = 0; i < uniqeSiteMapUrl.Count; i++)
                {
                    Console.WriteLine("{0})   {1}", i+1, uniqeSiteMapUrl[i]);
                }
            }
            else
            {
                Console.WriteLine("No unique links found in sitemap.xml  ...");
            }
        }


        public void UniqeCrawledUrl()     //Urls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml
        {
            Console.WriteLine("Uniqe Urls FOUNDED BY CRAWLING");
            var uniqeCrawledUrl = urlsByCrawling.Keys.Except(urlFromSiteMap.Keys).ToList();

            if (uniqeCrawledUrl.Any())
            {
                for (int i = 0; i < uniqeCrawledUrl.Count; i++)
                {
                    Console.WriteLine("{0})   {1}", i + 1, uniqeCrawledUrl[i]);
                }
            }
            else
            {
                Console.WriteLine("No unique links found during crawling...");
            }
        }

        public void Timing()    // Find out the response time of the site and output ii on console (sorted)
        {
            Console.WriteLine("URL AND TIME EACH PAGE");
            var UrlAndTime = urlsByCrawling.Union(urlFromSiteMap).OrderBy(e => e.Value).ToList();
            
            Console.WriteLine("Url                    Timing");
            foreach(var item in UrlAndTime)
            {
                Console.WriteLine("{0}   {1}ms", item.Key, item.Value);
            }
        }
    }
}
