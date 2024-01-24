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
    public class TABLE
    {
        public string TABLE_NAME { get; set; }
    }
    public class COLUMN
    {
        public string   TABLE_NAME { get; set; }
        public string   COLUMN_NAME { get; set; }
        public string   COLUMN_TYPE { get; set; }
        public int      ORDINAL_POSITION { get; set; }
    }
    public class FK : IEquatable<FK>
    {
        public string FK_NAME { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string FK_TABLE_NAME { get; set; }
        public string FK_COLUMN_NAME { get; set; }

        public bool Equals(FK other)
        {
            return FK_NAME == other.FK_NAME;
        }

        public override int GetHashCode()
        {
            return FK_NAME.GetHashCode();
        }
    }

    public class VIEW
    {
        public string VIEW_SCHEMA { get; set; }
        public string VIEW_NAME { get; set; }
    }

    public class VCOLUMN
    {
        public string VIEW_SCHEMA { get; set; }
        public string VIEW_NAME { get; set; }
        public string TABLE_SCHEMA { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public int COLUMN_MAX_WIDTH { get; set; }
    }
}
