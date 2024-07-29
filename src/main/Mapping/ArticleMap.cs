using FluentNHibernate.Mapping;
using iLib.src.main.Model;
using NHibernate.Type;

namespace iLib.src.main.Mapping
{
    public class ArticleMap : SubclassMap<Article>
    {
        public ArticleMap()
        {
            Abstract();
            Map(x => x.Location);
            Map(x => x.Title);
            Map(x => x.YearEdition);
            Map(x => x.Publisher);
            Map(x => x.Genre);
            Map(x => x.Description);
            Map(x => x.State).CustomType<EnumStringType<ArticleState>>().Column("State");
        }
    }
}
