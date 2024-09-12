using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class ScriptCreator : IScriptCreator
{
    private readonly IConfiguration _config;

    public ScriptCreator(IConfiguration config)
    {
        _config = config;
    }

    public int Generate(string inFile, string outFile)
    {
        if (!string.IsNullOrEmpty(inFile) && !string.IsNullOrEmpty(outFile))
        {
            var inFileData = JsonSerializer.Deserialize<Rootobject>(File.ReadAllText(inFile));
            var sb = new StringBuilder();
            if (inFileData != null)
            {
                ParseRootObject(inFileData, sb);
                File.WriteAllText(outFile, sb.ToString());
                return 0;
            }
        }

        return -1;
    }

    private void ParseRootObject(Rootobject inFileData, StringBuilder sb)
    {
        sb.AppendLine("USE [master]");
        sb.AppendLine("GO\n");
        sb.AppendLine($"DROP DATABASE [{inFileData.DatabaseName}]");
        sb.AppendLine("GO\n");
        sb.AppendLine($"CREATE DATABASE [{inFileData.DatabaseName}]");
        sb.AppendLine("CONTAINMENT = NONE");
        sb.AppendLine("ON PRIMARY");
        sb.AppendLine($" ( NAME = N'{inFileData.DatabaseName}', FILENAME = N'C:\\Program Files\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\MSSQL\\DATA\\{inFileData.DatabaseName}.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )");
        sb.AppendLine(" LOG ON");
        sb.AppendLine($" ( NAME = N'{inFileData.DatabaseName}_log', FILENAME = N'C:\\Program Files\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\MSSQL\\DATA\\{inFileData.DatabaseName}_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )");
        sb.AppendLine("WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF");
        sb.AppendLine("GO\n");
        sb.AppendLine($"use [{inFileData.DatabaseName}]");
        sb.AppendLine("GO\n");

        foreach (var t in inFileData.Tables)
        {
            ParseTable(t, sb);
        }
    }

    private void ParseTable(Table t, StringBuilder sb)
    {
        sb.AppendLine($"IF EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{t.Name}]') AND type in (N'U'))");
        sb.AppendLine($"DROP TABLE [dbo].[{t.Name}]");
        sb.AppendLine("GO\n");

        sb.AppendLine($"create  table [{t.Name}] (");
        bool hasKey = false;
        foreach (var c in t.Columns)
        {
            if (c.PrimaryKey)
            {
                hasKey = true;
            }

            ParseColumn(c, sb);
        }
        if (hasKey)
        {
            sb.AppendLine($"CONSTRAINT[PK_{t.Name}] PRIMARY KEY CLUSTERED");
            sb.AppendLine("(");
            sb.AppendLine($"[{string.Join(',',(from c in t.Columns where c.PrimaryKey == true select c.Name ))}] ASC");
            sb.AppendLine(") WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]");
            sb.AppendLine(") ON[PRIMARY]");
        }
        sb.AppendLine("GO\n");

    }

    private void ParseColumn(Column c, StringBuilder sb)
    {
        sb.Append($"[{c.Name}] {c.DataType} ");
        if (c.Length > 0)
        {
            sb.Append($"({c.Length})");
        }
        if (c.PrimaryKey)
        {
            sb.Append(" IDENTITY(1,1) NOT NULL");
        }
        sb.AppendLine(",");
    }
}
