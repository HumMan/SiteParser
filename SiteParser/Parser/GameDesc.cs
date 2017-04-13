using HtmlAgilityPack;
using Shared.Model;
using SiteParser.CacheProvider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SiteParser.Parser
{
    class GameDesc
    {
        public static void GetGameDesc(GameInfo info)
        {
            //заполняем url скриншотов
            {
                var doc = Cache.LoadGameScreenshots(info.Id);

                var node = doc.DocumentNode.SelectNodes(".//div[@class='main-content']/div[@id='game_screens']/div[@id='screensarea']/ul[@class='gamescreens']/li[@class='game_screen']");
                if (node != null)
                {
                    foreach (var n in node)
                    {
                        var screenshot = n.SelectSingleNode("div/div[@class='screen_img_container']/a");
                        var thumb = screenshot.SelectSingleNode("img");
                        info.Screenshots.Add(new TScreenshot
                        {
                            ThumbUrl = Shared.DecodeHtml(thumb.GetAttributeValue("src", null)),
                            Url = Shared.DecodeHtml(screenshot.GetAttributeValue("href", null))
                        });
                    }
                }
            }
            {
                var doc = Cache.LoadGameDesc(info.Id);
                {
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']//div[@id='reviewtext']");
                    info.Desc = Shared.TrimHtml(node);
                }
                {
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']//span[@class='game_alt_names']");
                    if (node != null)
                        info.AltName = node.InnerText;
                }
                {
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/table[@class='gameinfo']//td[@class='game-cover']/div[@class='game_info_cover']/img");
                    info.CoverImageUrl = Shared.DecodeHtml(node.GetAttributeValue("src", null));
                    if (info.CoverImageUrl.Contains("nocover-rus.gif"))
                        info.CoverImageUrl = null;
                }

                {
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/table[@class='gameinfo']//span[@itemprop='aggregateRating']");
                    ParseGameUserStars(node.SelectSingleNode("img").GetAttributeValue("title", null), info);
                }
                {
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/div[@id='game_review']/div[@id='reviewarea']//ul[@class='game-groups']");
                    if (node != null)
                    {
                        foreach (var item in node.SelectNodes("li"))
                        {
                            var group = new TGameGroup();
                            info.GameGroups.Add(group);
                            group.Name = item.FirstChild.InnerText;
                            foreach (var value in item.SelectNodes("a"))
                            {
                                var newGroupValue = new TGameGroupValue();
                                group.Values.Add(newGroupValue);
                                newGroupValue.Href = value.GetAttributeValue("href", null);
                                newGroupValue.Title = Shared.TrimHtml(value.GetAttributeValue("title", null));
                                newGroupValue.Value = value.InnerText;
                            }
                        }
                    }
                }
                {
                    var nodes = doc.DocumentNode.SelectNodes(".//div[@class='bookmark-icon-block']/ul[@class='bookmark-icon-list']/li");
                    info.Favorites = int.Parse(nodes[0].SelectSingleNode("div[@class='bookmark-icon-count']").InnerText);
                    info.Completed = int.Parse(nodes[1].SelectSingleNode("div[@class='bookmark-icon-count']").InnerText);
                    info.Bookmarks = int.Parse(nodes[2].SelectSingleNode("div[@class='bookmark-icon-count']").InnerText);
                }
                {
                    var nodes = doc.DocumentNode.SelectNodes(".//div[@class='main-content']//table[@class='game_content_table']/tr");
                    foreach (var n in nodes)
                    {
                        var temp = n.SelectSingleNode("td/div[@class='game_description_col_text']");
                        if (temp != null && temp.InnerText.Contains("Рекомендуемые"))
                        {
                            var recomendNodes = n.SelectNodes("td/div[@class='middlesmall']/a[@class='game_recommended']");
                            if (recomendNodes != null)
                            {
                                foreach (var r in recomendNodes)
                                {
                                    info.Recomended.Add(new TGameRecomended
                                    {
                                        Id = Shared.GetGameIdFromUrl(r.GetAttributeValue("href", null)),
                                        Name = Shared.DecodeHtml(r.InnerText)
                                    });

                                }
                            }
                        }
                        else if (temp != null && temp.InnerText.Contains("Модификации"))
                        {
                            var recomendNodes = n.SelectNodes("td/div[@class='middlesmall']/a[@class='game_recommended']");
                            if (recomendNodes != null)
                            {

                                foreach (var r in recomendNodes)
                                {
                                    info.Mods.Add(new TGameMod
                                    {
                                        Href = r.GetAttributeValue("href", null),
                                        Name = Shared.DecodeHtml(r.InnerText)
                                    });

                                }
                            }
                        }
                    }
                }
                {
                    //количество страниц комментариев
                    var node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/div[@id='comments']//div[@class='game_comments_pager']");

                    //если несколько страниц комментариев
                    if (node != null)
                    {
                        string commentsPagesCountText = node.SelectSingleNode("span").InnerText;
                        var match = Regex.Match(commentsPagesCountText, @"Стр\. (\d+)/(\d+)");
                        int commentsPagesCount = int.Parse(match.Groups[2].Value);
                        ParseComments(doc, info.Comments);

                        string threadId = FindThreadId(doc);

                        for (int i = 2; i <= commentsPagesCount; i++)
                        {
                            var subDoc = Cache.LoadComments(info.Id, i, threadId);
                            ParseComments(subDoc, info.Comments);
                        }
                    }
                    else
                    //если комментарии без страниц
                    {
                        node = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/div[@id='comments']/table[@class='game_comments_table']");
                        if (node != null)
                            ParseComments(doc, info.Comments);
                    }
                }
            }
        }

        private static void ParseComments(HtmlDocument doc, List<TGameComment> comments)
        {
            var nodes = doc.DocumentNode.SelectNodes(".//div[@class='game_comments_container']/table[@class='game_comments_table']/tr");
            foreach (var n in nodes)
            {
                var node = n.SelectSingleNode("td[@class='game_comments_row']");
                if (node != null)
                {
                    var newComment = new TGameComment();
                    var userNode = node.SelectSingleNode("div[@class='middlesmall']/b/a");
                    if (userNode == null)
                    {
                        userNode = node.SelectSingleNode("div[@class='middlesmall']/b");
                        newComment.UserRef = null;
                        newComment.UserName = userNode.InnerText;
                    }
                    else
                    {
                        newComment.UserRef = userNode.GetAttributeValue("href", null);
                        newComment.UserName = userNode.InnerText;
                    }

                    var commentNode = n.SelectSingleNode("td[@class='game_comments_text']/div[@class='middlesmall']");
                    newComment.Comment = Shared.TrimHtml(commentNode);

                    userNode = node.SelectSingleNode("div[@class='red middlesmall']");
                    newComment.Date = DateTime.ParseExact(userNode.InnerText.Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                    userNode = node.SelectSingleNode("img");
                    newComment.Stars = ParseCommentUserStars(userNode.GetAttributeValue("title", null));

                    comments.Add(newComment);
                }
            }
        }

        private static void ParseGameUserStars(string value, GameInfo info)
        {
            var regex2 = new Regex(@"Оценка пользователей - ([\d\.]+) из 10. Всего голосов: (\d+)");
            var match = regex2.Match(value);
            info.UsersStars = Decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            info.UsersStarsSum = Int32.Parse(match.Groups[2].Value);
        }

        private static int ParseCommentUserStars(string value)
        {
            if (value == "Оценка отсутствует")
                return -1;

            var regex2 = new Regex(@"Оценка пользователя - (\d+) из 10");
            var match = regex2.Match(value);
            return Int32.Parse(match.Groups[1].Value);
        }

        private static string FindThreadId(HtmlDocument doc)
        {
            var scriptNode = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/script[@type='text/javascript']");
            var threadIdStart = scriptNode.InnerText.IndexOf("\"gamethreadid\":");

            var match = Regex.Match(scriptNode.InnerText, "\"gamethreadid\":\"(\\d+)\"");
            return match.Groups[1].Value;
        }
    }
}
