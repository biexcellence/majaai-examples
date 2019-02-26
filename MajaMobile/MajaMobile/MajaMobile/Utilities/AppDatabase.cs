using MajaMobile.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MajaMobile.Utilities
{
    internal class AppDatabase : IDisposable
    {
        public const int DatabaseVersionNr = 1;
        private const string _databaseName = "majamobileappdatabase.db3";

        private SQLiteConnection _Connection;
        public SQLiteConnection Connection
        {
            get
            {
                if (_Connection == null)
                    _Connection = new SQLiteConnection(DependencyService.Get<IDeviceInfo>().getLocalFilePath(_databaseName));
                return _Connection;
            }
        }

        public static bool UpdateDatabase()
        {
            if (!DependencyService.Get<IDeviceInfo>().FileExists(_databaseName))
            {
                using (var database = new AppDatabase())
                {
                    CreateDatabase(database.Connection);
                }
            }
            else
            {
                using (var database = new AppDatabase())
                {
                    try
                    {
                        if (database.Connection.Table<DatabaseVersion>().First().VersionNr != DatabaseVersionNr)
                        {
                            IEnumerable<MajaTalentData> talents = null;
                            try
                            {
                                talents = database.Connection.Table<MajaTalentData>().ToList();
                                database.Connection.DropTable<MajaTalentData>();
                            }
                            catch (Exception) { }

                            database.Connection.DropTable<DatabaseVersion>();
                            CreateDatabase(database.Connection);

                            if (talents != null)
                                database.Connection.InsertAll(talents);
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Dispose()
        {
            if (_Connection != null)
            {
                _Connection.Dispose();
                _Connection = null;
            }
        }

        private static void CreateDatabase(SQLiteConnection connection)
        {
            connection.CreateTable<DatabaseVersion>();
            connection.CreateTable<MajaTalentData>();

            connection.Insert(new DatabaseVersion());
        }

        public IEnumerable<string> GetTalentIds()
        {
            return Connection.Table<MajaTalentData>().Select(t => t.Id);
        }

        public bool SetMajaTalentData(IEnumerable<string> talentIds)
        {
            try
            {
                Connection.DeleteAll<MajaTalentData>();
                foreach (var id in talentIds)
                {
                    Connection.Insert(new MajaTalentData(id));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    [Table("MajaTalent")]
    public class MajaTalentData
    {
        [PrimaryKey, Unique, Column("Id")]
        public string Id { get; set; }
        public MajaTalentData()
        {

        }
        public MajaTalentData(string id)
        {
            Id = id;
        }
    }

    [Table("DatabaseVersion")]
    internal class DatabaseVersion
    {
        [PrimaryKey, Unique, Column("Id")]
        public int VersionNr { get; set; }

        public DatabaseVersion()
        {
            VersionNr = AppDatabase.DatabaseVersionNr;
        }
    }

}