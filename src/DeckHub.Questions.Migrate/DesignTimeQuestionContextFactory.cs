using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DeckHub.Questions.Data;

namespace DeckHub.Questions.Migrate
{
    public class DesignTimeQuestionContextFactory : IDesignTimeDbContextFactory<QuestionContext>
    {
        public const string LocalPostgres = "Host=localhost;Database=questions;Username=deckhub;Password=SecretSquirrel";

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