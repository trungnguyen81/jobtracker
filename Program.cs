using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Globalization;
using CsvHelper;


namespace Jobtracker
{
    class Program
        {
            static ScrapingBrowser _scrapingBrowser = new ScrapingBrowser();
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a search term");
            var searchTerm = Console.ReadLine();
            var mainLinks = GetMainPageLinks("https://ca.indeed.com/Software-Development-jobs-in-Saskatoon,-SK?from=breadcrumbs");
            var lstPageDetails = GetPageDetails(mainLinks, searchTerm);
            exportGigsToCSV(lstPageDetails, searchTerm);
        }

        static List<string> GetMainPageLinks(string url) {
            var homePageLinks = new List<string>();
            var html = GetHtml(url);
            var links = html.CssSelect("a");
            
            foreach (var link in links) {
                if(link.Attributes["href"].Value.Contains(".html")){
                    homePageLinks.Add(link.Attributes["href"].Value);
                }

            }

            return homePageLinks;
        }

        static List<PageDetails> GetPageDetails(List<string> urls, string searchTerm) {
            var lstPageDetails = new List<PageDetails>();
            foreach(var url in urls){
                var htmlNode = GetHtml(url);
                var pageDetails = new PageDetails();

                pageDetails.Title = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/head/title").InnerText;
                var descriptions = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/section/section/").InnerText;
                pageDetails.Url = url;

                var searchTermInTitle = pageDetails.Title.ToLower().Contains(searchTerm.ToLower());
                var searchTermInDescription = pageDetails.Descriptions.ToLower().Contains(searchTerm.ToLower());

                if(searchTermInTitle || searchTermInDescription){
                    lstPageDetails.Add(pageDetails);

                }

                
            }
            return lstPageDetails;
        }

        static void exportGigsToCSV(List<PageDetails> lstPageDetails, string searchTerm){
            using( var writer = new StreamWriter($@"/Users/Thuy LE/Desktop/{searchTerm}_{DateTime.Now.ToFileTime()}.csv"))
            using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
                csv.WriteRecord(lstPageDetails);
            }


        }

        static HtmlNode GetHtml(string url) {
            WebPage webpage = _scrapingBrowser.NavigateToPage(new Uri(url));
            return webpage.Html;
        }
    }

    public class  PageDetails {
        public string Title {get; set; }

        public string Descriptions {get; set; }

        public string Url {get; set; }
    }
}
