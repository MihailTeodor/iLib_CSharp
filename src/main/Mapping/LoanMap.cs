using FluentNHibernate.Mapping;
using iLib.src.main.Model;
using NHibernate.Type;

namespace iLib.src.main.Mapping
{
    public class LoanMap : SubclassMap<Loan>
    {
        public LoanMap()
        {
            Map(x => x.LoanDate);
            Map(x => x.DueDate);
            Map(x => x.Renewed);
            Map(x => x.State).CustomType<EnumStringType<LoanState>>().Column("State");
            References(x => x.LoaningUser).Fetch.Join().Not.Nullable().Column("userId");
            References(x => x.ArticleOnLoan).Fetch.Join().Not.Nullable().Column("articleId");
            Table("loans");
        }
    }
}
