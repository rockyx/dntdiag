using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

#if __ANDROID__
using System.IO;
using Android.Content;
using Android.Database.Sqlite;
using Connection = Mono.Data.Sqlite.SqliteConnection;
using Command = Mono.Data.Sqlite.SqliteCommand;
using ConnectionStringBuilder = Mono.Data.Sqlite.SqliteConnectionStringBuilder;
using Parameter = Mono.Data.Sqlite.SqliteParameter;

#else
using System.Data.SQLite;

using Connection = SQLiteConnection;
using Command = SQLiteCommand;
using ConnectionStringBuilder = SQLiteConnectionStringBuilder;
using Parameter = SQLiteParameter;

#endif
using DNT.Diag.Data;

namespace DNT.Diag.DB
{
    public sealed class VehicleDB
    {
        private Dictionary<string, byte[]> encryptMap;
        private Dictionary<string, string> decryptMap;
        private Dictionary<string, byte[]> commandMap;
        private Connection conn;
        private Command cmdCommand;
        private Command textCommand;
        private Command troubleCodeCommand;
        private Command liveDataCommand;

        static VehicleDB()
        {
        }

        public VehicleDB(string absoluteFilePath)
        {
            encryptMap = new Dictionary<string, byte[]>();
            decryptMap = new Dictionary<string, string>();
            commandMap = new Dictionary<string, byte[]>();
            Open(absoluteFilePath);
        }

        public VehicleDB(string filePath, string dbName)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (filePath.EndsWith("/") || filePath.EndsWith("\\"))
                {
                    sb.AppendFormat("{0}{1}.db", filePath, dbName);
                }
                else
                {
                    sb.AppendFormat("{0}/{1}.db", filePath, dbName);
                }
                Open(sb.ToString());
            }
            catch (Exception ex)
            {
                Close();
                throw new DatabaseException(
                    String.Format("Cannot open vehicle database! file path = \"{0}\", database name = \"{1}\", error message: {2}", 
                        filePath, dbName, ex.Message));
            }
        }

        public void Open(string absoluteFilePath)
        {
            try
            {
                ConnectionStringBuilder connstr = new ConnectionStringBuilder();
                connstr.DataSource = absoluteFilePath;
                conn = new Connection();
                conn.ConnectionString = connstr.ToString();
                conn.Open();

                cmdCommand = new Command(
                    "SELECT [Command] FROM [Command] WHERE [Name]=:name AND [Class]=:class", 
                    conn);
                cmdCommand.Parameters.Add(":name", DbType.Binary);
                cmdCommand.Parameters.Add(":class", DbType.Binary);

                textCommand = new Command(
                    "SELECT [Content] FROM [Text] WHERE [Name]=:name AND [Language]=:language AND [Class]=:class",
                    conn);
                textCommand.Parameters.Add(":name", DbType.Binary);
                textCommand.Parameters.Add(":language", DbType.Binary);
                textCommand.Parameters.Add(":class", DbType.Binary);

                troubleCodeCommand = new Command(
                    "SELECT [Content], [Description] FROM [TroubleCode] WHERE [Code]=:code AND [Language]=:language AND [Class]=:class",
                    conn);
                troubleCodeCommand.Parameters.Add(":code", DbType.Binary);
                troubleCodeCommand.Parameters.Add(":language", DbType.Binary);
                troubleCodeCommand.Parameters.Add(":class", DbType.Binary);

                liveDataCommand = new Command(
                    "SELECT [ShortName], [Content], [Unit], [DefaultValue], [CommandName], [CommandClass], [Description], [Index] FROM [LiveData] WHERE [Language]=:language AND [Class]=:class",
                    conn);
                liveDataCommand.Parameters.Add(":language", DbType.Binary);
                liveDataCommand.Parameters.Add(":class", DbType.Binary);
            }
            catch (Exception ex)
            {
                Close();
                throw new DatabaseException(
                    String.Format("Cannot open vehicle database! file path = \"{0}\", error message: {1}", 
                        absoluteFilePath, ex.Message));
            }
        }

        public void Close()
        {
            liveDataCommand.Dispose();
            troubleCodeCommand.Dispose();
            textCommand.Dispose();
            cmdCommand.Dispose();
            conn.Close();
        }

        private byte[] Encrypt(string plain)
        {
            if (!encryptMap.ContainsKey(plain))
            {
                encryptMap.Add(plain, DBCrypto.Encrypt(plain));
            }
            return encryptMap[plain];
        }

        private byte[] EncryptLang
        {
            get { return Encrypt(Settings.LanguageText); }
        }

        public byte[] QueryCommand(string name, string cls)
        {
            Action ThrowException = () =>
            {
                throw new DatabaseException(String.Format("Query command fail by name = {0}, class = {1}",
                    name, cls));
            };

            string key = String.Format("{0}_{1}", name, cls);

            if (!commandMap.ContainsKey(key))
            {
                try
                {
                    cmdCommand.Prepare();
                    cmdCommand.Parameters[0].Value = Encrypt(name);
                    cmdCommand.Parameters[1].Value = Encrypt(cls);
                    using (var reader = cmdCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            commandMap.Add(key, DBCrypto.DecryptToBytes(reader.GetFieldValue<byte[]>(0)));
                        }
                    }
                }
                catch
                {
                    ThrowException();
                }
            }

            return commandMap[key];
        }

        public string QueryText(string name, string cls)
        {
            Action ThrowException = () =>
            {
                throw new DatabaseException(String.Format("Query text fail by name = {0}, class = {1}",
                    name, cls));
            };

            string key = String.Format("{0}_{1}_{2}", name, Settings.LanguageText, cls);

            if (!decryptMap.ContainsKey(key))
            {

                try
                {
                    textCommand.Prepare();

                    textCommand.Parameters[0].Value = Encrypt(name);
                    textCommand.Parameters[1].Value = EncryptLang;
                    textCommand.Parameters[2].Value = Encrypt(cls);

                    using (var reader = textCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decryptMap.Add(key, DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(0)));
                        }
                        else
                        {
                            ThrowException();
                        }
                    }
                }
                catch (DatabaseException)
                {
                    throw;
                }
                catch
                {
                    ThrowException();
                }
            }
            return decryptMap[key];
        }

        public TroubleCodeItem QueryTroubleCode(string code, string cls)
        {
            Action ThrowException = () =>
            {
                throw new DatabaseException(String.Format("Query trouble code fail by code = {0}, class = {1}",
                    code, cls));
            };

            try
            {
                troubleCodeCommand.Prepare();

                troubleCodeCommand.Parameters[0].Value = Encrypt(code);
                troubleCodeCommand.Parameters[1].Value = EncryptLang;
                troubleCodeCommand.Parameters[2].Value = Encrypt(cls);

                using (var reader = troubleCodeCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        TroubleCodeItem item = new TroubleCodeItem();
                        item.Code = code;
                        item.Content = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(0));

                        if (!reader.IsDBNull(1))
                            item.Description = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(1));
                        return item;
                    }
                }
            }
            catch
            {
                ThrowException();
            }

            ThrowException();
            return null;
        }

        public LiveDataList QueryLiveData(string cls)
        {
            Action ThrowException = () =>
            {
                throw new DatabaseException(String.Format("Query live data fail by class = {0}", cls));
            };

            try
            {
                liveDataCommand.Prepare();

                liveDataCommand.Parameters[0].Value = EncryptLang;
                liveDataCommand.Parameters[1].Value = Encrypt(cls);

                using (var reader = liveDataCommand.ExecuteReader())
                {
                    LiveDataList list = new LiveDataList();
                    while (reader.Read())
                    {
                        LiveDataItem item = new LiveDataItem();

                        item.ShortName = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(0));

                        item.Content = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(1));

                        item.Unit = reader.IsDBNull(2) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(2));

                        item.DefaultValue = reader.IsDBNull(3) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(3));

                        if (!reader.IsDBNull(4) && !reader.IsDBNull(5))
                        {
                            item.CmdName = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(4));
                            item.CmdClass = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(5));
                            item.Command = QueryCommand(item.CmdName, item.CmdClass);
                        }

                        item.Description = reader.IsDBNull(6) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(6));
                        byte[] indexArray = DBCrypto.DecryptToBytes(reader.GetFieldValue<byte[]>(7));
                        int index = ((indexArray[3] & 0xFF) << 24) +
                        ((indexArray[2] & 0xFF) << 16) +
                        ((indexArray[1] & 0xFF) << 8) +
                        (indexArray[0] & 0xFF);

                        item.IndexForSort = index;

                        list.Add(item);
                    }

                    return list;
                } 
            }
            catch
            {
                ThrowException();
            }

            ThrowException();
            return null;
        }
        #if __ANDROID__
        public static void CopyDatabase(Context context, string dbName)
        {
            using (Stream istream = context.Assets.Open(dbName))
            {
                using (SQLiteDatabase db = context.OpenOrCreateDatabase(dbName, FileCreationMode.Private, null))
                {
                }
                using (Stream ostream = new FileStream(context.GetDatabasePath(dbName).AbsolutePath, FileMode.Open))
                {
                    byte[] buffer = new byte[1024];
                    int length = 0;
                    while ((length = istream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ostream.Write(buffer, 0, length);
                    }
                }
            }
        }
        #endif
    }
}

