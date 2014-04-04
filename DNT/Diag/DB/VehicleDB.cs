using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

#if __ANDROID__
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
		private Connection conn;
		private Command cmdCommand;
		private Command textCommand;
		private Command troubleCodeCommand;
		private Command liveDataCommand;

		static VehicleDB ()
		{
		}

		public VehicleDB (string filePath, string dbName)
		{
			try {
				ConnectionStringBuilder connstr = new ConnectionStringBuilder ();
				StringBuilder sb = new StringBuilder ();
				if (filePath.EndsWith ("/") || filePath.EndsWith ("\\")) {
					sb.AppendFormat ("{0}{1}.db", filePath, dbName);
				} else {
					sb.AppendFormat ("{0}/{1}.db", filePath, dbName);
				}

				connstr.DataSource = sb.ToString ();
				conn = new Connection ();
				conn.ConnectionString = connstr.ToString ();
				conn.Open ();

				cmdCommand = new Command (
					"SELECT [Command] FROM [Command] WHERE [Name]=:name AND [Class]=:class", 
					conn);
				cmdCommand.Parameters.Add (":name", DbType.Binary);
				cmdCommand.Parameters.Add (":class", DbType.Binary);

				textCommand = new Command (
					"SELECT [Content] FROM [Text] WHERE [Name]=:name AND [Language]=:language AND [Class]=:class",
					conn);
				textCommand.Parameters.Add (":name", DbType.Binary);
				textCommand.Parameters.Add (":language", DbType.Binary);
				textCommand.Parameters.Add (":class", DbType.Binary);

				troubleCodeCommand = new Command (
					"SELECT [Content], [Description] FROM [TroubleCode] WHERE [Code]=:code AND [Language]=:language AND [Class]=:class",
					conn);
				troubleCodeCommand.Parameters.Add (":code", DbType.Binary);
				troubleCodeCommand.Parameters.Add (":language", DbType.Binary);
				troubleCodeCommand.Parameters.Add (":class", DbType.Binary);

				liveDataCommand = new Command (
					"SELECT [ShortName], [Content], [Unit], [DefaultValue], [CommandName], [CommandClass], [Description], [Index] FROM [LiveData] WHERE [Language]=:language AND [Class]=:class",
					conn);
				liveDataCommand.Parameters.Add (":language", DbType.Binary);
				liveDataCommand.Parameters.Add (":class", DbType.Binary);

			} catch (Exception ex) {
				Close ();
				throw new DatabaseException (
					String.Format ("Cannot open vehicle database! file path = \"{0}\", database name = \"{1}\", error message: {2}", 
						filePath, dbName, ex.Message));
			}
		}

		public void Close ()
		{
			liveDataCommand.Dispose ();
			troubleCodeCommand.Dispose ();
			textCommand.Dispose ();
			cmdCommand.Dispose ();
			conn.Close ();
		}

		public byte[] queryCommand (string name, string cls)
		{
			Action ThrowException = () => {
				throw new DatabaseException (String.Format ("Query command fail by name = {0}, class = {1}",
					name, cls));
			};

			cmdCommand.Prepare ();
			byte[] enName = null;
			byte[] enCls = null;

			try {
				enName = DBCrypto.Encrypt (name);
				enCls = DBCrypto.Encrypt (cls);
			} catch {
				ThrowException ();
			}

			cmdCommand.Parameters [0].Value = enName;
			cmdCommand.Parameters [1].Value = enCls;

			try {
				using (var reader = cmdCommand.ExecuteReader ()) {
					if (reader.Read ()) {
						DBCrypto.DecryptToBytes (reader.GetFieldValue<byte[]> (0));
					}
				}
			} catch {
				ThrowException ();
			}

			ThrowException ();
			return null;
		}

		public string queryText (string name, string cls)
		{
			Action ThrowException = () => {
				throw new DatabaseException (String.Format ("Query text fail by name = {0}, class = {1}",
					name, cls));
			};

			textCommand.Prepare ();

			byte[] enName = null;
			byte[] enCls = null;

			try {
				enName = DBCrypto.Encrypt (name);
				enCls = DBCrypto.Encrypt (cls);
			} catch {
				ThrowException ();
			}
				
			textCommand.Parameters [0].Value = enName;
			textCommand.Parameters [1].Value = DBCrypto.Language;
			textCommand.Parameters [2].Value = enCls;

			try {
				using (var reader = textCommand.ExecuteReader ()) {
					if (reader.Read ()) {
						return DBCrypto.Decrypt (name, cls, reader.GetFieldValue<byte[]> (0));
					}
				}
			} catch {
				ThrowException ();
			}

			ThrowException ();
			return null;
		}

		public TroubleCodeItem queryTroubleCode (string code, string cls)
		{
			Action ThrowException = () => {
				throw new DatabaseException (String.Format("Query trouble code fail by code = {0}, class = {1}",
					code, cls));
			};

			troubleCodeCommand.Prepare ();

			byte[] enCode = null;
			byte[] enCls = null;

			try {
				enCode = DBCrypto.Encrypt (code);
				enCls = DBCrypto.Encrypt (cls);
			} catch {
				ThrowException ();
			}

			troubleCodeCommand.Parameters [0].Value = enCode;
			troubleCodeCommand.Parameters [1].Value = DBCrypto.Language;
			troubleCodeCommand.Parameters [2].Value = enCls;

			try {
				using (var reader = troubleCodeCommand.ExecuteReader ()) {
					if (reader.Read ()) {
						TroubleCodeItem item = new TroubleCodeItem ();
						item.Code = code;
						item.Content = DBCrypto.DecryptToString (reader.GetFieldValue<byte[]> (0));

						if (!reader.IsDBNull(1))
							item.Description = DBCrypto.DecryptToString (reader.GetFieldValue<byte[]> (1));
						return item;
					}
				}
			} catch {
				ThrowException ();
			}

			ThrowException ();
			return null;
		}

		public LiveDataList queryLiveData (string cls)
		{
			Action ThrowException = () => {
				throw new DatabaseException (String.Format("Query live data fail by class = {0}", cls));
			};

			liveDataCommand.Prepare ();

			byte[] enCls = null;

			try {
				enCls = DBCrypto.Encrypt (cls);
			} catch {
				ThrowException ();
			}

			liveDataCommand.Parameters [0].Value = DBCrypto.Language;
			liveDataCommand.Parameters [1].Value = enCls;

			try {
				using (var reader = liveDataCommand.ExecuteReader ()) {
					LiveDataList list = new LiveDataList ();
					while (reader.Read ()) {
						LiveDataItem item = new LiveDataItem ();

						item.ShortName = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]> (0));

						item.Content = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]> (1));

						item.Unit = reader.IsDBNull(2) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]> (2));

						item.DefaultValue = reader.IsDBNull(3) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]> (3));

						if (!reader.IsDBNull(4) && !reader.IsDBNull(5)) {
							item.CmdName = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(4));
							item.CmdClass = DBCrypto.DecryptToString(reader.GetFieldValue<byte[]>(5));
							item.Command = queryCommand(item.CmdName, item.CmdClass);
						}

						item.Description = reader.IsDBNull(6) ? "" : DBCrypto.DecryptToString(reader.GetFieldValue<byte[]> (6));
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
			} catch {
				ThrowException ();
			}

			ThrowException ();
			return null;
		}
	}
}

