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
using EnvDTE;

namespace SirSqlValetCommands.Commands
{
    internal static class Command1003_RotateExecContext
    {
        public static IEnumerable<string> Execute(CommandsUI commandUI)
        {
            List<string> lines   = commandUI.textDocumentLines.ToList();

            Regex   rxDEV       = new Regex(@"^(?:\s|\t)*(--)?(?:\s|\t)*DECLARE(?:\s|\t)+@DEV(?:\s|\t)+BIT(?:\s|\t)*=(?:\s|\t)*(0|1)(?:\s|\t)*;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var     DEVLines    = lines.WithIndex().Where(_ => rxDEV.IsMatch(_._)).Select(_ => (_:_._, i:_.i, m: rxDEV.Match(_._))).ToList();

            if (DEVLines.Count <= 1)
                return lines;

            var     DEV0Info    = DEVLines.First(_ => _.m.Groups[1].ToString().isnws());
            int     DEV0Line    = DEV0Info.i;
            bool    DEV0        = DEV0Info.m.Groups[2].ToString().Equals("0");

            Regex   rxTRANS     = new Regex(@"^(?:\s|\t)*(--)?(?:\s|\t)*(ROLLBACK|COMMIT)(?:\s|\t)+TRANS(?:ACTION)?(?:\s|\t)*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var     TRANSLines  = lines.WithIndex().Where(_ => rxTRANS.IsMatch(_._)).Select(_ => (_: _._, i: _.i, m: rxTRANS.Match(_._))).ToList();

            if (TRANSLines.Count <= 1)
                return lines;

            var     TRANSInfo   = TRANSLines.First(_ => _.m.Groups[1].ToString().isnws());
            int     TRANSLine   = TRANSInfo.i;
            bool    COMMIT      = TRANSInfo.m.Groups[2].ToString().ToUpper().Equals("COMMIT");

            if (DEV0 && !COMMIT)
                return Command1002_SwitchComment.Execute(lines, TRANSLine);

            else if (DEV0)
                return Command1002_SwitchComment.Execute(Command1002_SwitchComment.Execute(lines, DEV0Line), TRANSLine);

            else if (COMMIT)
                return Command1002_SwitchComment.Execute(Command1002_SwitchComment.Execute(lines, DEV0Line), TRANSLine);

            else
                return Command1002_SwitchComment.Execute(lines, DEV0Line);
        }
    }
}
