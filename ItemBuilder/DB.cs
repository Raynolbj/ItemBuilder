using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ItemBuilder;

public class DB
{
    private static string DBName = "log.db";
    public static SQLiteConnection conn;

    public static void OpenConnection()
    {
        string libFolder = FileSystem.AppDataDirectory;
        string fname = System.IO.Path.Combine(libFolder, DBName);
        conn = new SQLiteConnection(fname);

        conn.CreateTable<Build>();
        conn.CreateTable<Champion>(); // Create Champion table
        conn.CreateTable<Item>(); // Create Item tables
        conn.CreateTable<BuildItem>();

    }

}
