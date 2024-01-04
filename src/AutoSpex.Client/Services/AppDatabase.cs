using System;

namespace AutoSpex.Client.Services;

public class AppDatabase : SQLiteConnector
{
    public AppDatabase(Func<string> pathFactory) : base(pathFactory)
    {
    }
}