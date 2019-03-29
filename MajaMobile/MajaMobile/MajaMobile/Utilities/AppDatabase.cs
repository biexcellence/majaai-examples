using BiExcellence.OpenBi.Api.Commands.MajaAi;
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
        public const int DatabaseVersionNr = 2;
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

        public bool InsertMajaTalentData(IMajaTalent talent)
        {
            try
            {
                Connection.Insert(new MajaTalentData(talent));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteMajaTalentData(IMajaTalent talent)
        {
            try
            {
                Connection.Delete<MajaTalentData>(talent.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<string> DeletePrivateTalentData()
        {
            try
            {
                Connection.Table<MajaTalentData>().Delete(t => !t.IsPublic);
            }
            catch (Exception)
            {
                try
                {
                    //delete everything
                    Connection.DeleteAll<MajaTalentData>();
                }
                catch (Exception) { }
            }
            return GetTalentIds();
        }
    }

    [Table("MajaTalent")]
    public class MajaTalentData
    {
        [PrimaryKey, Unique, Column("Id")]
        public string Id { get; set; }
        public bool IsPublic { get; set; }
        public MajaTalentData()
        {

        }
        public MajaTalentData(IMajaTalent talent)
        {
            Id = talent.Id;
            IsPublic = talent.IsPublic;
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