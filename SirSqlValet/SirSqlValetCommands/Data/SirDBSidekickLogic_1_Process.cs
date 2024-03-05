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
    public static partial class SirDBSidekickLogic
    {
        public static string ProcessScriptAndSelectedLine()
        {
            if (wd.scriptLines.Count < 2)
                return "Pas assez de lignes dans ce script";

            if (wd.scriptLines.Count(_ => _.notisnws()) < 2)
                return "Pas assez de lignes non vide dans ce script";

            // ==============================================================================================================================
            // S O R T I E   P O S S I B L E  -  non positionné
            // ==============================================================================================================================
            if (wd.numeroLigneCurseur == -1)
                return "Aucune ligne n'est présentement sélectionnée";

            // ==============================================================================================================================
            // S O R T I E   P O S S I B L E  -  positionné sur une ligne vide
            // ==============================================================================================================================
            if (wd.SafeGetLine(wd.numeroLigneCurseur).Trim().isnws())
                return "La ligne sélectionnée est vide";

            // ==============================================================================================================================
            // S O R T I E   P O S S I B L E  -  positionné sur une ligne vide
            // ==============================================================================================================================
            if (wd.SafeGetLine(wd.numeroLigneCurseur).Trim()[wd.SafeGetLine(wd.numeroLigneCurseur).Trim().Length - 1] == ';')
                return "La ligne se termine avec un ';'. Ce n'est pas supporté.";

            // ==============================================================================================================================
            // S O R T I E   P O S S I B L E  -  dans le cas où le curseur est dans un groupe de lignes où il n'y a pas d'énoncé "FROM ..."
            // ==============================================================================================================================
            if (wd.LineInsertSuperieur == -1)
                return "Le curseur est dans un groupe de lignes où il n'y a pas d'énoncé 'FROM ...' avant la ligne du curseur";

            // ==============================================================================================================================
            // S O R T I E   P O S S I B L E dans le cas où on pas identifié de table sur la ligne du curseur ( les vTypes.whatever comptent pas )
            // ==============================================================================================================================
            var words = wd.SafeGetLine(wd.numeroLigneCurseur).Split(new [] {' ', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(_ => _.Trim());
            try
            {
                wd.selectedTable = words.Take(3).First(_ => BD_Schema.tables.Any(t => t.TABLE_NAME.Equals(_, nocase)));
            }
            catch 
            {
                wd.selectedTable = "";
            }            
            if (wd.selectedTable == string.Empty)
                return "Incapable de repérer une table dans la ligne du curseur";

            // ------------------------------------------------------------------------------------------------------------------------------
            // on verifie que les majuscules/minuscules soient comme dans la vrai table
            // ------------------------------------------------------------------------------------------------------------------------------
            string realTableName = BD_Schema.tables.First(t => wd.selectedTable.Equals(t.TABLE_NAME, nocase)).TABLE_NAME;
            
            if (wd.selectedTable != realTableName)
            {
                wd.SafeSetLine(wd.numeroLigneCurseur, wd.SafeGetLine(wd.numeroLigneCurseur).Replace(wd.selectedTable, realTableName));
                wd.selectedTable = realTableName;
            }

            // ------------------------------------------------------------------------------------------------------------------------------
            // on identifie un  prefix si présent et un acronyme si présent
            // ------------------------------------------------------------------------------------------------------------------------------
            MatchCollection matchesAroundSelectedTable;
            if ((matchesAroundSelectedTable = (new Regex($@"^.*(?:FROM|JOIN)(?:\s|\t)+((?'prefix'\w+)\.)?{wd.selectedTable}(?:\s|\t)+(?'acronyme'\w+).*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)).Matches(wd.SafeGetLine(wd.numeroLigneCurseur))).Count == 1)
                foreach (Group group in matchesAroundSelectedTable[0].Groups)
                    if (group.Success)
                        if (group.Name == "prefix")
                            wd.selectedPrefixe = group.ToString();
                        else if (group.Name == "acronyme")
                            wd.selectedAcronyme = group.ToString();

            Func<string, string, string> BuildPossibleAcronyme = (prefix, table) => { return (prefix.notisnws() ? $"{prefix[0]}" : "") + table.Where(_ => $"{_}" == $"{_}".ToUpper()).Join(null); };
            string expectedAcronyme = BuildPossibleAcronyme(wd.selectedPrefixe, wd.selectedTable);

            // ==============================================================================================================================
            // ==============================================================================================================================
            //
            // S O R T I E   P O S S I B L E
            //
            //     dans le cas où on arrive pas à obtenir un acronyme
            //
            // ==============================================================================================================================
            if (expectedAcronyme == string.Empty || BuildPossibleAcronyme("", wd.selectedTable) == string.Empty)
                return "SirBDSidekick n'a pas été capable de repérer ou de contruire un acronyme valide à partir dans la ligne sélectionnée";

            // ------------------------------------------------------------------------------------------------------------------------------
            // on verifie et corrige la présence de l'acronyme dans le script
            //  -> !!! MODIFICATION DU SCRIPT !!!
            // ------------------------------------------------------------------------------------------------------------------------------
            if (wd.selectedAcronyme == string.Empty || (wd.selectedAcronyme != expectedAcronyme && (new Regex(expectedAcronyme + @"\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(wd.selectedAcronyme).Count) == 0))
            {
                string newLine;
                if (wd.selectedAcronyme == string.Empty)
                {
                    newLine = wd.SafeGetLine(wd.numeroLigneCurseur).Replace(" " + wd.selectedTable, " " + wd.selectedTable + " " + expectedAcronyme);
                    foreach (int j in RangeFromTo(wd.numeroLigneCurseur, wd.derniereLigneRequete))
                        wd.SafeSetLine(j, wd.SafeGetLine(j).Replace(" " + wd.selectedTable + ".", " " + expectedAcronyme + ".").Replace("[" + wd.selectedTable + "].", " " + expectedAcronyme + "."));
                }
                else
                {
                    newLine = wd.SafeGetLine(wd.numeroLigneCurseur).Replace(" " + wd.selectedAcronyme, " " + expectedAcronyme);
                    foreach (int j in RangeFromTo(wd.numeroLigneCurseur, wd.derniereLigneRequete))
                        wd.SafeSetLine(j, wd.SafeGetLine(j).Replace(" " + wd.selectedAcronyme + ".", " " + expectedAcronyme + "."));
                }

                wd.selectedAcronyme                = expectedAcronyme;
                wd.SafeSetLine(wd.numeroLigneCurseur, newLine);
            }

            // ==============================================================================================================================
            // Navigation vers la page de saisie de la table à ajouter
            // dans la page appelante, si on retourne ""
            // Quand on retourne du texte c'est le message d'erreur qui nous oblige à rester dans la page courante
            // ==============================================================================================================================
            return string.Empty;
        }

    } // fin class SirDBSidekickLogic
}

