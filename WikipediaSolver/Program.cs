using System;
using System.Net;
using HtmlAgilityPack;

class Program
{

    static HttpClient httpClient = new HttpClient();

    public static async Task<List<string>> GetWikipediaLinks(string url)
    {
        HttpResponseMessage response = await httpClient.GetAsync(url);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(await response.Content.ReadAsStringAsync());
        List<string> allLinks = getAllLinks(document.DocumentNode.SelectSingleNode("//body"));
        //Console.WriteLine("got " + allLinks.Count.ToString().PadRight(5) + " from " + url + " ");
        return allLinks;
    }

    public static List<string> getAllLinks(HtmlNode node)
    {
        List<string> urls = new List<string>();
        if (node.Name.ToLower() == "a")
            urls = node.Attributes.AttributesWithName("href").Select(x => x.Value).Where(x => x.StartsWith("/wiki/")).ToList();
        foreach (HtmlNode n in node.ChildNodes)
            urls.AddRange(getAllLinks(n));
        return urls;
    }

    public static Dictionary<string, string?> visitedUrls = new Dictionary<string, string>();

    public static async Task<List<string>> getNextLinks(string link)
    {
        return (await GetWikipediaLinks(link)).Select(x => "https://en.wikipedia.org" + x).Distinct().Where(x => !visitedUrls.ContainsKey(x)).ToList();
    }

    public static async Task Main()
    {
        while (true)
        {
            Console.Write("Enter start url: ");
            string startUrl = Console.ReadLine();
            Console.Write("Enter target url: ");
            string targetUrl = Console.ReadLine();
            visitedUrls = new Dictionary<string, string> { { startUrl, null } };
            List<string> links = new List<string> { startUrl };
            List<string> nextLinks = new List<string>();
            while (!links.Contains(targetUrl))
            {
                Console.WriteLine("NEW GENERATION: " + links.Count + " LINKS TO SEARCH--------------------");
                foreach (string link in links)
                {
                    List<string> newLinks = await getNextLinks(link);
                    foreach (string s in newLinks)
                        visitedUrls.Add(s, link);
                    nextLinks.AddRange(newLinks);
                    if (nextLinks.Contains(targetUrl))
                        break;
                }
                Console.WriteLine("copying " + nextLinks.Count + " urls");
                links = nextLinks.ToList();
                nextLinks.Clear();
                Console.WriteLine("finished generation");
            }
            Console.WriteLine("found path");
            List<string> path = new List<string> { targetUrl };
            while (targetUrl != null)
            {
                targetUrl = visitedUrls[targetUrl];
                path.Add(targetUrl);
            }
            path.Reverse();
            foreach (string s in path)
                Console.WriteLine(s);
        }
    }
}

