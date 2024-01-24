using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

using TextCopy;

using static SirSqlValetCommands.Data.SVCGlobal;
using static SirSqlValetCommands.Data.GCSS;
using static SirSqlValetCommands.Data.Extensions;

namespace SirSqlValetCommands.Data
{
    public static class BD_Schema
    {
        private static string path { get; set; }

        private static string _fileNameFormat { get; set; } = "";
        private static string fileNameFormat
        { 
            get
            {   
                if (_fileNameFormat.isnws())
                {
                    Regex   rx      = new Regex(@"^(.*)_(?:views|vcolumns|tables|columns|fks)\.json$");
                    var     fis     = Directory.GetFiles(path).Select(_ => new FileInfo(_)).Select(_ => (fi:_, m: rx.Match(_.Name))).Where(_ => _.m.Success).ToList();
                    var     keys    = fis.GroupBy(_ => _.m.Groups[1].Value).Select(_ => (key: _.Key, dt:_.First().fi.LastWriteTime)).OrderByDescending(_ => _.dt).ToList();

                    _fileNameFormat = $"{(keys.Any() ? keys.First().key : "SirSqlValet")}_{{0}}.json";
                }
                return _fileNameFormat; 
            }
        }

        private static Dictionary<string, string> fileNames => new Dictionary<string, string>()
        {
            { nameof(views),    Path.Combine(path, string.Format(fileNameFormat, nameof(views))) },
            { nameof(vcolumns), Path.Combine(path, string.Format(fileNameFormat, nameof(vcolumns))) },
            { nameof(tables),   Path.Combine(path, string.Format(fileNameFormat, nameof(tables))) },
            { nameof(columns),  Path.Combine(path, string.Format(fileNameFormat, nameof(columns))) },
            { nameof(fks),      Path.Combine(path, string.Format(fileNameFormat, nameof(fks))) }
        };

        private static bool ReadFromFiles()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();

            list.Add(nameof(views),     File.ReadAllText(fileNames[nameof(views)]));
            list.Add(nameof(vcolumns),  File.ReadAllText(fileNames[nameof(vcolumns)]));
            list.Add(nameof(tables),    File.ReadAllText(fileNames[nameof(tables)]));
            list.Add(nameof(columns),   File.ReadAllText(fileNames[nameof(columns)]));
            list.Add(nameof(fks),       File.ReadAllText(fileNames[nameof(fks)]));

            FromJSON(list);

            return true;
        }

        public  static void WriteToFiles()
        {
            foreach (var d in ToJSON())
                File.WriteAllText(path: d.filename, contents: d.content);
        }

        public  static List<(string filename, string content)> ToJSON()
        {
            List<(string filename, string content)> returnValue = new List<(string filename, string content)>();

            returnValue.Add((fileNames[nameof(views)],      JsonSerializer.Serialize(views)));
            returnValue.Add((fileNames[nameof(vcolumns)],   JsonSerializer.Serialize(vcolumns)));
            returnValue.Add((fileNames[nameof(tables)],     JsonSerializer.Serialize(tables)));
            returnValue.Add((fileNames[nameof(columns)],    JsonSerializer.Serialize(columns)));
            returnValue.Add((fileNames[nameof(fks)],        JsonSerializer.Serialize(fks)));

            return returnValue;
        }

        public  static void FromJSON(Dictionary<string, string> list)
        {
            views       = JsonSerializer.Deserialize<List<VIEW>>(list[nameof(views)]);
            vcolumns    = JsonSerializer.Deserialize<List<VCOLUMN>>(list[nameof(vcolumns)]);
            tables      = JsonSerializer.Deserialize<List<TABLE>>(list[nameof(tables)]);
            columns     = JsonSerializer.Deserialize<List<COLUMN>>(list[nameof(columns)]);
            fks         = JsonSerializer.Deserialize<List<FK>>(list[nameof(fks)]);
        }

        public  static List<VIEW>    views       { get ; private set; } = new List<VIEW>();
        public  static List<VCOLUMN> vcolumns    { get; private set; }  = new List<VCOLUMN>();
        public  static List<TABLE>   tables      { get; private set; }  = new List<TABLE>  ();
        public  static List<COLUMN>  columns     { get; private set; }  = new List<COLUMN> ();
        public  static List<FK>      fks         { get; private set; }  = new List<FK>     ();

        private static int GetIntFromSQL(string sql, SqlConnection connection)
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(sql.Replace("DescriptionFrancais", "Description"), connection)))
                    da.Fill(dt);

                return int.Parse(dt.Rows[0][0].ToString());
            }
            catch
            {
                return 1;
            }
        }

        public static bool IsPopulated => tables.Any();

        public static bool HasToBePopulated => !tables.Any();

        public static bool Populate(string server, string database, string path, bool reset = false)
        {
            BD_Schema.path = path;

            if (tables.Count == 0)
            {
                if (fileNames.All(_ => File.Exists(_.Value)))
                    ReadFromFiles();
                else
                    foreach (var fn in fileNames)
                        if (File.Exists(fn.Value))
                            File.Delete(fn.Value);
            }

            if (tables.Count == 0)
            {
                SqlConnectionStringBuilder  sb                          = new SqlConnectionStringBuilder();
                                            sb.DataSource               = server;
                                            sb.InitialCatalog           = database;
                                            sb.IntegratedSecurity       = true;
                                            sb.ConnectTimeout           = 30;
                                            sb.Encrypt                  = SqlConnectionEncryptOption.Optional;
                                            sb.TrustServerCertificate   = false;
                                            sb.ApplicationIntent        = ApplicationIntent.ReadOnly;
                                            sb.MultiSubnetFailover      = false;

                using (SqlConnection connection = new SqlConnection(sb.ConnectionString))
                {
                    string sql = "";
                    sql = sql + " SELECT     FK.name    FK_NAME,";
                    sql = sql + "            TS.name    TABLE_NAME,";
                    sql = sql + "            ACS.name   COLUMN_NAME,";
                    sql = sql + "            TT.name    FK_TABLE_NAME,";
                    sql = sql + "            ACT.name   FK_COLUMN_NAME";
                    sql = sql + " FROM       " + database + ".sys.foreign_keys FK";
                    sql = sql + " INNER JOIN " + database + ".sys.foreign_key_columns FKC on FKC.constraint_object_id = FK.object_id";
                    sql = sql + " INNER JOIN " + database + ".sys.all_columns ACS on ACS.object_id = FKC.parent_object_id      AND ACS.column_id = FKC.parent_column_id";
                    sql = sql + " INNER JOIN " + database + ".sys.all_columns ACT on ACT.object_id = FKC.referenced_object_id  AND ACT.column_id = FKC.referenced_column_id";
                    sql = sql + " INNER JOIN " + database + ".sys.tables TS  on TS.object_id = FKC.parent_object_id";
                    sql = sql + " INNER JOIN " + database + ".sys.tables TT  on TT.object_id = FKC.referenced_object_id";

                    // Connect to the database then retrieve the schema information.
                    bool connected = false;
                    try
                    {
                        connection.Open();
                        connected = true;
                    }
                    catch (Exception)
                    {
                        throw new Exception($@"Cannot connect to {database} on {server}");
                    }

                    if (connected)
                    {
                        tables =   (from x in connection.GetSchema("Tables").AsEnumerable()
                                    select new TABLE { TABLE_NAME = x["TABLE_NAME"].ToString() }).ToList();

                        columns =  (from x in connection.GetSchema("AllColumns").AsEnumerable()
                                    select new COLUMN { TABLE_NAME          = x["TABLE_NAME"].ToString(),
                                                        COLUMN_NAME         = x["COLUMN_NAME"].ToString(),
                                                        COLUMN_TYPE         = x["DATA_TYPE"].ToString(),
                                                        ORDINAL_POSITION    = 0 }).ToList();

                        views =    (from x in connection.GetSchema("Views").AsEnumerable()
                                    select new VIEW {   VIEW_SCHEMA = x["TABLE_SCHEMA"].ToString(),
                                                        VIEW_NAME   = x["TABLE_NAME"].ToString() }).ToList();

                        vcolumns = (from x in connection.GetSchema("ViewColumns").AsEnumerable()
                                    select new VCOLUMN {    VIEW_SCHEMA         = x["VIEW_SCHEMA"].ToString(),
                                                            VIEW_NAME           = x["VIEW_NAME"].ToString(),
                                                            TABLE_SCHEMA        = x["TABLE_SCHEMA"].ToString(),
                                                            TABLE_NAME          = x["TABLE_NAME"].ToString(),
                                                            COLUMN_NAME         = x["COLUMN_NAME"].ToString(),
                                                            COLUMN_MAX_WIDTH    = GetIntFromSQL(string.Format("SELECT MAX(LEN({0})) FROM {1}.{2}", x["COLUMN_NAME"].ToString(), x["VIEW_SCHEMA"].ToString(), x["VIEW_NAME"].ToString()), connection)}).ToList();

                        DataTable dt;
                        foreach (TABLE t in tables)
                        {
                            try
                            {
                                dt = new DataTable();
                                using (SqlDataAdapter da = new SqlDataAdapter(new SqlCommand($"SELECT * FROM [{t.TABLE_NAME}] WHERE 0 <> 0", connection)))
                                {
                                    da.Fill(dt);
                                    for (int i = 0; i < dt.Columns.Count; i++)
                                        columns.Single(c => c.TABLE_NAME.ToUpper() == t.TABLE_NAME.ToUpper() && c.COLUMN_NAME.ToUpper() == dt.Columns[i].ColumnName.ToUpper()).ORDINAL_POSITION = i;
                                }
                            }
                            catch { }
                        }

                        dt = new DataTable();
                        SqlCommand cmd = new SqlCommand(sql, connection);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dt);

                        fks =  (from x in dt.AsEnumerable()
                                select new FK
                                {
                                    FK_NAME         = x["FK_NAME"].ToString(),
                                    TABLE_NAME      = x["TABLE_NAME"].ToString(),
                                    COLUMN_NAME     = x["COLUMN_NAME"].ToString(),
                                    FK_TABLE_NAME   = x["FK_TABLE_NAME"].ToString(),
                                    FK_COLUMN_NAME  = x["FK_COLUMN_NAME"].ToString()
                                }).ToList();
                    }
                }

                if (tables.Count > 0)
                    BD_Schema.WriteToFiles();
            }
            return tables.Count > 0;
        }
    }
}
