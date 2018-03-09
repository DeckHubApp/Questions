using Microsoft.Extensions.Logging;

namespace Slidable.Questions.Migrate
{
    public static class EventIds
    {
        public static readonly EventId MigrationTestConnectFailed = 1001;
        public static readonly EventId MigrationFailed = 1002;
    }
}