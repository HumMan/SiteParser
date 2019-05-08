using System;
using System.Collections.Generic;

namespace Shared.Model
{
    public class TScreenshot
    {
        public string ThumbUrl { get; set; }
        public string Url { get; set; }
    }

    public class TGameGroupValue
    {
        public string Href { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }

    public class TGameGroup
    {
        public TGameGroup()
        {
            Values = new List<TGameGroupValue>();
        }
        public string Name { get; set; }
        public List<TGameGroupValue> Values { get; set; }
    }

    public class TGameComment
    {
        public string UserRef { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; }
    }

    public class TGameRecomended
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class TGameMod
    {
        public string Name { get; set; }
        public string Href { get; set; }
    }

    public class GameInfo
    {
        public GameInfo()
        {
            Screenshots = new List<TScreenshot>();
            GameGroups = new List<TGameGroup>();
            Comments = new List<TGameComment>();
            Recomended = new List<TGameRecomended>();
            Mods = new List<TGameMod>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string AltName { get; set; }
        public string CoverImageUrl { get; set; }
        public string Genre { get; set; }
        public string Year { get; set; }
        public string Platform { get; set; }
        public string Publisher { get; set; }
        public int Stars { get; set; }

        public string Desc { get; set; }

        public decimal UsersStars { get; set; }
        public int UsersStarsSum { get; set; }

        public int Favorites { get; set; }
        public int Completed { get; set; }
        public int Bookmarks { get; set; }

        public List<TGameRecomended> Recomended { get; set; }
        public List<TGameMod> Mods { get; set; }

        public List<TGameGroup> GameGroups { get; set; }

        public List<TGameComment> Comments { get; set; }
        public string CommentsThreadId { get; set; }

        public List<TScreenshot> Screenshots { get; set; }
    }
}
