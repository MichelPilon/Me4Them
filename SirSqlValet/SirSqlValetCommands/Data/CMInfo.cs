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
using System.Windows.Forms;
using System.Windows;

namespace SirSqlValetCommands.Data
{
    public class CMInfo
    {
        public string SERVER_NAME { get; set; }
        public string DISPLAY_NAME { get; set; }
        public string GROUP_NAME { get; set; }
    }

    public static class CMInfos
    {
        public static string FriendlyName (this CMInfo cmi)
        { 
            string cleanDisplayName = cmi.DISPLAY_NAME;
            while (cleanDisplayName.notisnws() && " *-".Contains(cleanDisplayName[0]))
                cleanDisplayName = (cleanDisplayName.Length > 1 ? cleanDisplayName.Substring(1) : string.Empty);
            while (cleanDisplayName.notisnws() && " *-".Contains(cleanDisplayName[cleanDisplayName.Length - 1]))
                cleanDisplayName = (cleanDisplayName.Length > 1 ? cleanDisplayName.Substring(0, cleanDisplayName.Length - 1) : string.Empty);

            return !string.IsNullOrWhiteSpace(cleanDisplayName) ? cleanDisplayName.ToUpper().Equals(cmi.SERVER_NAME.ToUpper()) ? cleanDisplayName : $"{cleanDisplayName} [{cmi.SERVER_NAME}]" : cmi.SERVER_NAME;
        }

        private static string fileName { get; set; } = @"C:\Users\P67\AppData\Local\SirSqlValet\SirSqlValet_CMInfo.json";
        private static List<CMInfo> _cminfos { get; set; } = new List<CMInfo>();
        public static List<CMInfo> cminfos
        { 
            get
            {
                Load();
                return _cminfos;
            }
        }

        public static void Load(string filename = "")
        {
            if (filename.notisnws() && File.Exists(filename))
                CMInfos.fileName = filename;

            _cminfos = JsonSerializer.Deserialize<List<CMInfo>>(File.ReadAllText(CMInfos.fileName)).OrderBy(_ => _.GROUP_NAME).ThenBy(_ => _.SERVER_NAME).ToList();

            bool update = false;
            if (!_cminfos.All(_ => _.SERVER_NAME.Equals(_.SERVER_NAME.ToUpper())))
            {
                _cminfos.ForEach(_ => _.SERVER_NAME = _.SERVER_NAME.ToUpper());
                update = true;
            }

            if (!_cminfos.All(_ => _.DISPLAY_NAME.Equals(_.DISPLAY_NAME.ToUpper())))
            {
                _cminfos.ForEach(_ => _.DISPLAY_NAME = _.DISPLAY_NAME.ToUpper());
                update = true;
            }

            if (update)
                Save();
        }

        public static void Save()
        { 
            if (!_cminfos.Any())
                return;

            if (File.Exists(CMInfos.fileName))
            { 
                string backupFileName = $"{CMInfos.fileName}.bak";

                if (File.Exists(backupFileName))
                    File.Delete(backupFileName);

                File.Move(CMInfos.fileName, backupFileName);
            }

            File.WriteAllText(CMInfos.fileName, JsonSerializer.Serialize(_cminfos));
        }
    }
}

