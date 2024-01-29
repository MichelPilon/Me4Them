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
    public static class Command1005_SingleLine
    {
        public static string Execute(SirSqlValetCommands.CommandsUI commandUI)
        {
            string selection = Regex.Replace(commandUI.textSelectionString, Environment.NewLine, m => " ");

            string ligne = "";
            Func<string, string> shrink = (texte) => { return Regex.Replace(texte, @"\s+", m => " "); };

            bool Chaine = false;
            foreach (Match m in Regex.Matches(selection, @" (?<avantDouble>[^']*)(?<double>(?:'')+)(?<apresDouble>[^']*) | (?<avant>[^']*)'(?<apres>[^']*) | (?<total>[^']*) ", RegexOptions.IgnorePatternWhitespace))
            {
                if (m.Groups["total"].Length > 0)
                    ligne += shrink(m.Groups["total"].Value);

                else if (m.Groups["avant"].Length > 0 || m.Groups["apres"].Length > 0)
                {
                    if (Chaine)
                        ligne += m.Groups["avant"].Value +                      // brut  
                                    "'" +
                                    shrink(m.Groups["apres"].Value);            // compresser
                    else
                        ligne += shrink(m.Groups["avant"].Value) +              // compresser 
                                    "'" +
                                    m.Groups["apres"].Value;                    // brut

                    Chaine = !Chaine;
                }
                else if (m.Groups["avantDouble"].Length > 0 || m.Groups["double"].Length > 0 || m.Groups["apresDouble"].Length > 0)
                    if (Chaine)
                        ligne += m.Groups["avantDouble"].Value +                // brut  
                                    m.Groups["double"].Value +
                                    m.Groups["apresDouble"].Value;              // brut
                    else
                        ligne += shrink(m.Groups["avantDouble"].Value) +        // brut  
                                    m.Groups["double"].Value +
                                    shrink(m.Groups["apresDouble"].Value);      // brut
            };

            return ligne;
        }
    }
}