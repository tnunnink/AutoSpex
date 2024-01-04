using System;

namespace AutoSpex.Client.Services;

public class ProjectDatabase : SQLiteConnector
{
    public ProjectDatabase(Func<string> pathFactory) : base(pathFactory)
    {
    }
}