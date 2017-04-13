using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SiteParser.Parser
{
    public class Shared
    {
        public static string DecodeHtml(string value)
        {
            var result = HttpUtility.HtmlDecode(value).Trim();
            return result;
        }

        public static string TrimHtml(string value)
        {
            var result = HttpUtility.HtmlDecode(value);
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            result = regex.Replace(result, " ");
            regex = new Regex("[\r\n\t]", options);
            result = regex.Replace(result, "").Trim();
            return result;
        }

        public static string ConvertTags(HtmlNode node, bool writeTags = true)
        {
            var sb = new StringBuilder();
            if (writeTags && node.NodeType == HtmlNodeType.Element)
                sb.Append("[" + node.Name + "]");
            if (!node.HasChildNodes)
            {
                sb.Append(node.InnerText);
            }
            else
            {
                foreach (var child in node.ChildNodes)
                    sb.Append(ConvertTags(child));
            }
            if (writeTags && node.NodeType == HtmlNodeType.Element && node.Closed && node.Name != "br")
                sb.Append("[/" + node.Name + "]");
            return sb.ToString();
        }

        public static string TrimHtml(HtmlNode value)
        {
            var result = TrimHtml(ConvertTags(value, false));
            return result;
        }

        public static int GetGameIdFromUrl(string url)
        {
            var match = Regex.Match(url, @"/game/(\d+)\.html");
            return int.Parse(match.Groups[1].Value);
        }
    }
}
