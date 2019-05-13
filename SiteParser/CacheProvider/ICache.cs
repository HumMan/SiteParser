using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteParser.CacheProvider
{
    interface ICache: IDisposable
    {
        HtmlDocument LoadPage(int id);
        HtmlDocument LoadComments(int gameId, int id, string threadId);
        HtmlDocument LoadGameDesc(int gameId);
        HtmlDocument LoadGameScreenshots(int gameId);
    }
}
