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

using static SirSqlValetCommands.Data.GCSS;
using static SirSqlValetCommands.Data.Extensions;
using EnvDTE;
using static SirSqlValetCommands.CommandsUI;

namespace SirSqlValetCommands.Commands
{
    internal static class Command1001_SirSqlValet
    {
        public static IEnumerable<string> Execute(CommandsUI commandUI)
        {
            FScript f = new FScript(commandUI.textDocumentString, commandUI.TL(enumNBase.enbZero));
            var lines = f.MyShowDialog();
            f.Close();
            f.Dispose();

            return lines;
        }
    }
}
