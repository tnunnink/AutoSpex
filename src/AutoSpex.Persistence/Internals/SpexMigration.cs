using FluentMigrator;

namespace AutoSpex.Persistence;

[Tags(Database.Spex)]
internal abstract class SpexMigration : Migration;

[Tags(Database.Spex)]
internal abstract class SpexAutoReverseMigration : AutoReversingMigration;