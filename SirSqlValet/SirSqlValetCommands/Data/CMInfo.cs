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
using SirSqlValetCore.App;

namespace SirSqlValetCommands.Data
{
    public class CMInfo
    {
        private IEnumerable<string> _keyWords   = new List<string>();
        private string              _DISPLAY    = "";

        public IEnumerable<string>   KEYWORDS => _keyWords;

        public string SERVER { get; set; }
        public string DISPLAY 
        { 
            get => _DISPLAY;
            set
            { 
                _DISPLAY = value;
                _keyWords = _DISPLAY.ToUpper().Split(new [] {' ' }, StringSplitOptions.RemoveEmptyEntries).Where(_ => !_.Contains("*")).Distinct();
            }
        }
    }

    public static class CMInfos
    {
        private static string fileNameOnly = "SirSqlValet_CMInfo.json";

        public static IEnumerable<string> keyWords => _cminfos.SelectMany(_ => _.KEYWORDS).Distinct().OrderBy(_ => _);

        public static string FriendlyName(this CMInfo cmi)
        {
            string cleanDisplayName = cmi.DISPLAY;
            while (cleanDisplayName.notisnws() && " *-".Contains(cleanDisplayName[0]))
                cleanDisplayName = (cleanDisplayName.Length > 1 ? cleanDisplayName.Substring(1) : string.Empty);
            while (cleanDisplayName.notisnws() && " *-".Contains(cleanDisplayName[cleanDisplayName.Length - 1]))
                cleanDisplayName = (cleanDisplayName.Length > 1 ? cleanDisplayName.Substring(0, cleanDisplayName.Length - 1) : string.Empty);

            return !string.IsNullOrWhiteSpace(cleanDisplayName) ? cleanDisplayName.ToUpper().Equals(cmi.SERVER.ToUpper()) ? cleanDisplayName : $"{cleanDisplayName} [{cmi.SERVER}]" : cmi.SERVER;
        }
        public static bool STAR(this CMInfo cmi) => cmi.DISPLAY.Contains('*');
        public static int NSTAR(this CMInfo cmi) => cmi.DISPLAY.Where(_ => _.Equals('*')).Count();

        private static string fileName { get; set; } = string.Empty;
        private static List<CMInfo> _cminfos { get; set; } = new List<CMInfo>();


        public static List<CMInfo> cminfos() => _cminfos;

        public static void Load(string folder)
        {
            if (folder.notisnws() && Directory.Exists(folder))
            {
                CMInfos.fileName = Path.Combine(folder, fileNameOnly);
                if (!File.Exists(Path.Combine(folder, fileNameOnly)))
                { 
                    string defaultConfig = @"[{""DISPLAY"":""DEV 04 EX RH RF "",""SERVER"":""DEV04EX-SQL""},{""DISPLAY"":""DEV 04 IN RH"",""SERVER"":""DEV04IN-SQL""},{""DISPLAY"":""DEV 04 SIR *"",""SERVER"":""CCQSQL044170""},{""DISPLAY"":""DEV 04 COURRIELS"",""SERVER"":""CCQSQL044113""},{""DISPLAY"":""INT 04 IN"",""SERVER"":""INT04IN-SQL""},{""DISPLAY"":""INT 04 EX RF"",""SERVER"":""INT04EX-SQL""},{""DISPLAY"":""INT 04 COURRIELS"",""SERVER"":""CCQSQL044112""},{""DISPLAY"":""INT 04 SIR"",""SERVER"":""CCQSQL044180""},{""DISPLAY"":""INT 04 SIR OCVC"",""SERVER"":""CCQSIR044163""},{""DISPLAY"":""ACC 04 EX RF RH"",""SERVER"":""ACC04EX-SQL""},{""DISPLAY"":""ACC 04 IN RH"",""SERVER"":""ACC04IN-SQL""},{""DISPLAY"":""ACC 04 SIR"",""SERVER"":""CCQSQL044190""},{""DISPLAY"":""ACC 04 SIR OCVC"",""SERVER"":""CCQSIR045163""},{""DISPLAY"":""ACC 04 COURRIELS"",""SERVER"":""CCQSQL045027""},{""DISPLAY"":""PROD IN RH"",""SERVER"":""PRODIN-SQL""},{""DISPLAY"":""PROD EX RF RH"",""SERVER"":""PRODEX-SQL""},{""DISPLAY"":""PROD COURRIELS"",""SERVER"":""CCQSQL047041""},{""DISPLAY"":""PROD SIR"",""SERVER"":""CCQSQL046039""},{""DISPLAY"":""PROD SIR OCVC"",""SERVER"":""CCQSIR046163""},{""DISPLAY"":""PROD SIR ACTU-R"",""SERVER"":""CCQSQL046128""},{""DISPLAY"":""PREP01 EX RF RH"",""SERVER"":""PREP01EX-SQL""},{""DISPLAY"":""PREP01 IN RH"",""SERVER"":""PREP01IN-SQL""},{""DISPLAY"":""PREP01 SIR"",""SERVER"":""CCQSQL045190""}]";
                    File.WriteAllText(Path.Combine(folder, fileNameOnly), defaultConfig );
                }
            }

            _cminfos = JsonSerializer.Deserialize<List<CMInfo>>(File.ReadAllText(CMInfos.fileName)).ToList();

            bool update = false;
            if (!_cminfos.All(_ => _.SERVER.Equals(_.SERVER.ToUpper())))
            {
                _cminfos.ForEach(_ => _.SERVER = _.SERVER.ToUpper());
                update = true;
            }

            if (!_cminfos.All(_ => _.DISPLAY.Equals(_.DISPLAY.ToUpper())))
            {
                _cminfos.ForEach(_ => _.DISPLAY = _.DISPLAY.ToUpper());
                update = true;
            }

            if (update)
                Save();
        }

        public static void Save()
        { 
            if (!_cminfos.Any())
                return;

            BakFileRename(CMInfos.fileName);
            File.WriteAllText(CMInfos.fileName, JsonSerializer.Serialize(_cminfos));
        }

        private static void BakFileRename(string f, int maxGeneration = 10, int currentLevel = 0)
        {
            FileInfo fi;
            if (!(fi = new FileInfo(f)).Exists)
                return;

            currentLevel++;

            FileInfo fiNew;
            if ((fiNew = new FileInfo(Path.Combine(fi.DirectoryName, Path.GetFileNameWithoutExtension(f) + ".bak" + Path.GetExtension(f)))).Exists)
                if (currentLevel > maxGeneration)
                    fiNew.Delete();
                else
                    BakFileRename(fiNew.FullName, maxGeneration, currentLevel);

            fi.MoveTo(fiNew.FullName);
        }
    }
}

