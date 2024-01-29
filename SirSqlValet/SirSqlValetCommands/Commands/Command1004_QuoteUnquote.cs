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
    public static class Command1004_QuoteUnquote
    {
        public static string Execute(SirSqlValetCommands.CommandsUI commandUI)
        {
            if (commandUI.textSelectionString[0].ToString() == "'" && commandUI.textSelectionString[commandUI.textSelectionString.Length - 1].ToString() == "'")
                return commandUI.textSelectionString.Substring(1, commandUI.textSelectionString.Length - 2).Replace("''", "'");
            else if (commandUI.textSelectionString[0].ToString() == "'" || commandUI.textSelectionString[commandUI.textSelectionString.Length - 1].ToString() == "'")
                return commandUI.textSelectionString;
            else
                return "'" + commandUI.textSelectionString.Replace("'", "''") + "'";
        }
    }
}