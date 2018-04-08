using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Slidable.Questions.Data;

namespace Slidable.Questions.Migrate
{
    public class DesignTimeQuestionContextFactory : IDesignTimeDbContextFactory<QuestionContext>
    {
        public const string LocalPostgres = "Host=localhost;Database=questions;Username=slidable;Password=SecretSquirrel";

        public static readonly string MigrationAssemblyName =
            typeof(DesignTimeQuestionContextFactory).Assembly.GetName().Name;

        public QuestionContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<QuestionContext>()
                .UseNpgsql(args.FirstOrDefault() ?? LocalPostgres, b => b.MigrationsAssembly(MigrationAssemblyName));
            return new QuestionContext(builder.Options);
        }
    }
}