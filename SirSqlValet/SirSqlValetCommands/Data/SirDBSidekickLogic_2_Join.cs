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
        public static void Join(string userSelection, string joinType)
        {
            //  ===================================================================================================================================================================================================================================================
            //  ===================================================================================================================================================================================================================================================
            #region REG_process

            string GroupeSuperieur = string.Empty;
            string GroupeInferieur = string.Empty;

            string Champ = string.Empty;
            string[] Champs = new string[] { };
            string AutreTable = string.Empty;
            string AutreChamp = string.Empty;
            string[] AutresChamps = new string[] { };

            bool MultiSegement = false;

            Func<string, string, string> ExpandTablePointEtoile = (table, tableAcronyme) =>
            {
                string debuteTermine = "";
                string reste = "";

                foreach (COLUMN c in BD_Schema.columns.Where(c => c.TABLE_NAME.Equals(table, nocase)).OrderBy(c => c.ORDINAL_POSITION))
                {
                    string acrName = $"{tableAcronyme}.{c.COLUMN_NAME}";
                    string ajout = $"{acrName} [{acrName}]";

                    if (c.COLUMN_TYPE.ToUpper().Contains("MAX") && (c.COLUMN_TYPE.ToUpper().StartsWith("VARCHAR") || c.COLUMN_TYPE.ToUpper().StartsWith("NVARCHAR")))
                        ajout = $"CONVERT(NVARCHAR, {acrName}) [{acrName}]";

                    if (c.COLUMN_NAME.Equals("aDebute", nocase) || c.COLUMN_NAME.Equals("aTermine", nocase))
                        debuteTermine += (debuteTermine == "" ? "" : ", ") + ajout;
                    else
                        reste += (reste == "" ? "" : ", ") + ajout;
                }

                return debuteTermine + (debuteTermine == "" ? "" : ", ") + reste;
            };

            Func<string, string, string> BuildPossibleAcronyme = (prefix, table) => { return (prefix != string.Empty ? prefix[0].ToString() : "") + new string((from char c in table.ToCharArray() where c.ToString().ToUpper() == c.ToString() select c).ToArray()); };

            // --------------------------------------------------------------------------------------------------
            // Code de bonification
            // --------------------------------------------------------------------------------------------------
            Func<string, string, string, int, string> acronyme = (p, t, texteRecherche, max) =>
            {
                string PossibleAcronymeRoot = BuildPossibleAcronyme(p, t);
                string PossibleAcronyme = PossibleAcronymeRoot;
                int suffixe = 2;

                while (true)
                {
                    if (new Regex($@"FROM(?:\s|\t)+\w+(?:\s|\t)+{PossibleAcronyme}((?:\s|\t)|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
                        if (new Regex($@"JOIN(?:\s|\t)+(\w+\.)?\w+(?:\s|\t)+{PossibleAcronyme}(?:\s|\t)+ON(?:\s|\t).*$", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
                            break;

                    // --------------------------------------------------------------------------------------------------
                    // comme il ne peut pas y avoir plus de sequence d'un préfixe qu'il y a de lignes dans la requête
                    // le nombre de lignes de la requête est un bon critère de détection de loop enragée!!!
                    // --------------------------------------------------------------------------------------------------
                    if (suffixe > max)
                    {
                        PossibleAcronyme = PossibleAcronymeRoot + "_ERREUR";
                        break;
                    }

                    PossibleAcronyme = PossibleAcronymeRoot + (suffixe++).ToString();
                }

                return PossibleAcronyme;
            };

            // --------------------------------------------------------------------------------------------------
            // Code de bonification
            // --------------------------------------------------------------------------------------------------
            Func<bool, string, string, string, string, string, string, int, (string GSuperieur, string GInferieur)> BatirGroupeSuperieurEtBonification = (fBase, fTable, fAcronyme, fBaseTable, fTexteRequete, fGroupeSuperieur, fGroupeInferieur, fNbrLigneRequete) =>
            {
                string Insertion = string.Empty;

                List<COLUMN> fColsTypes = BD_Schema.columns.Where(c => { return (c.TABLE_NAME.Equals(fTable, nocase)) && (c.COLUMN_NAME.StartsWith("aPour", nocase)); }).ToList();

                if (fColsTypes.Count >= 1)
                {
                    foreach (COLUMN ColType in fColsTypes)
                    {
                        string ViewTypeName = ColType.COLUMN_NAME.Substring("aPour".Length);

                        VIEW v = BD_Schema.views.FirstOrDefault(view => { return view.VIEW_NAME.Equals(ViewTypeName, nocase); });
                        if ((v is null) && ViewTypeName.Equals("LangueCorrespondance", nocase))
                            v = BD_Schema.views.FirstOrDefault(view => { return view.VIEW_NAME.Equals("Langue"); });

                        if (!(v is null))
                        {
                            string A_VIEW_NAME = acronyme(v.VIEW_SCHEMA, v.VIEW_NAME, fTexteRequete + fGroupeInferieur, fNbrLigneRequete);
                            int size = BD_Schema.vcolumns.Single(vc => { return vc.VIEW_SCHEMA.Equals(v.VIEW_SCHEMA, nocase) && vc.VIEW_NAME.Equals(v.VIEW_NAME, nocase) && vc.COLUMN_NAME.Equals("DescriptionFrancais", nocase); }).COLUMN_MAX_WIDTH;
                            Insertion += string.Format(", CONVERT(NVARCHAR({2}), {0}.Description) AS [{1}]", A_VIEW_NAME, ViewTypeName, size);
                            if (fBase)
                                fGroupeInferieur = string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", v.VIEW_SCHEMA + "." + v.VIEW_NAME, A_VIEW_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME) + Environment.NewLine + fGroupeInferieur;
                            else
                                fGroupeInferieur += Environment.NewLine + string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", v.VIEW_SCHEMA + "." + v.VIEW_NAME, A_VIEW_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME);
                        }

                        else if (ViewTypeName.Equals("TypeDocument", nocase) || (ViewTypeName.Equals("Langue", nocase)))
                        {
                            TABLE t = BD_Schema.tables.FirstOrDefault(table => { return table.TABLE_NAME.Equals(ViewTypeName, nocase); });

                            COLUMN c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.Equals(ViewTypeName, nocase) && colonne.COLUMN_NAME.Equals("DescriptionFrancais", nocase); });
                            if (c is null)
                                c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.Equals(ViewTypeName, nocase) && colonne.COLUMN_NAME.Equals("Description", nocase); });

                            if (c is null)
                                c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.Equals(ViewTypeName, nocase) && Regex.IsMatch(colonne.COLUMN_TYPE, @"^N?(VAR)?CHAR.*", RegexOptions.IgnoreCase); });

                            if (!(t is null) && !(c is null))
                            {
                                string A_TABLE_NAME = acronyme("", t.TABLE_NAME, fTexteRequete + fGroupeInferieur, fNbrLigneRequete);
                                Insertion += string.Format(", CONVERT(NVARCHAR, {0}.{2}) AS [{1}]", A_TABLE_NAME, ViewTypeName, c.COLUMN_NAME);
                                if (fBase)
                                    fGroupeInferieur = string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", t.TABLE_NAME, A_TABLE_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME) + Environment.NewLine + fGroupeInferieur;
                                else
                                    fGroupeInferieur += Environment.NewLine + string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", t.TABLE_NAME, A_TABLE_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME);
                            }
                        }
                    }
                }

                fGroupeSuperieur = string.Format("{4}'----- {0}_{1} -----' AS [----- {0}_{1} -----]{3}, {2}", fTable, fAcronyme, ExpandTablePointEtoile(fTable, fAcronyme), Insertion, (fBase ? "," : ",")) + (fBase ? Environment.NewLine + fGroupeSuperieur : "");

                return (fGroupeSuperieur, fGroupeInferieur);
            };

            // -------------------------------------------------------------------------------
            // Extraire le champ de la table de base
            // et la table et le champ de la relation
            // -------------------------------------------------------------------------------
            MultiSegement = userSelection.Contains(',');
            foreach (string element in userSelection.Split("-=<>".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (element.Trim().Contains("."))
                {
                    string[] deuxMots = element.Trim().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    AutreTable = deuxMots[0];

                    if (MultiSegement)
                    {
                        AutresChamps = deuxMots[1].Replace('(', ' ').Replace(')', ' ').Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < AutresChamps.Count(); i++)
                            AutresChamps[i] = AutresChamps[i].Trim();

                        AutreChamp = AutresChamps[0].Trim();
                    }
                    else
                        AutreChamp = deuxMots[1].Trim();
                }
                else
                {
                    if (MultiSegement)
                    {
                        Champs = element.Replace('(', ' ').Replace(')', ' ').Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < Champs.Count(); i++)
                            Champs[i] = Champs[i].Trim();

                        Champ = Champs[0].Trim();
                    }
                    else
                        Champ = element.Trim();
                }
            };

            // --------------------------------------------------------------------------------
            // ajuster le JOIN type pour les relations de UniqueId à UniqueId (héritage)
            // --------------------------------------------------------------------------------
            if (joinType.Trim().ToUpper() == "LEFT")
                if (MultiSegement)
                {
                    bool Pareil = true;
                    for (int i = 0; i < Champs.Count(); i++)
                        if (Champs[i].ToUpper() != AutresChamps[i].ToUpper())
                        {
                            Pareil = false;
                            break;
                        }

                    if (Pareil)
                        joinType = "INNER";
                }
                else if (Champ.ToUpper() == AutreChamp.ToUpper())
                    joinType = "INNER";

            // --------------------------------------------------------------------------------------------------
            // bâtir une string contenant toutes les lignes de la requête
            // pour faciliter la recherche des acronymes
            // --------------------------------------------------------------------------------------------------
            //string TexteRequete = wd.scriptLines.Join(Environment.NewLine);
            string TexteRequete = wd.scriptLines.FromTo(wd.premiereLigneRequete, wd.derniereLigneRequete).Join();

            // --------------------------------------------------------------------------------------------------
            // calculer l'acronyme pour l'autre table
            // --------------------------------------------------------------------------------------------------
            string acronymeAutreTable = acronyme("", AutreTable, TexteRequete + GroupeInferieur, wd.derniereLigneRequete - wd.premiereLigneRequete + 1);

            // --------------------------------------------------------------------------------------------------
            // ajouter l'autre table dans la liste des nouveaux FROM/JOIN
            // --------------------------------------------------------------------------------------------------
            string AutreTableBrackets = AutreTable.ToUpper() == "Transaction".ToUpper() ? $"[{AutreTable}]" : AutreTable;

            if (MultiSegement)
            {
                GroupeInferieur = $"{joinType} JOIN  {AutreTableBrackets} {acronymeAutreTable} ON {acronymeAutreTable}.{AutresChamps[0]} = {wd.selectedAcronyme}.{Champs[0]}";
                for (int i = 1; i < Champs.Count(); i++)
                    GroupeInferieur += $" AND {acronymeAutreTable}.{AutresChamps[i]} = {wd.selectedAcronyme}.{Champs[i]}";
            }
            else
                GroupeInferieur = $"{joinType} JOIN  {AutreTableBrackets} {acronymeAutreTable} ON {acronymeAutreTable}.{AutreChamp} = {wd.selectedAcronyme}.{Champ}";

            // --------------------------------------------------------------------------------------------------
            // ajouter les tables pour le(s) type(s) de l'autre table dans la liste des nouveaux FROM/JOIN
            // --------------------------------------------------------------------------------------------------
            var Groupes = BatirGroupeSuperieurEtBonification(false, AutreTable, acronymeAutreTable, wd.selectedTable, TexteRequete, GroupeSuperieur, GroupeInferieur, wd.derniereLigneRequete - wd.premiereLigneRequete + 1);
            GroupeSuperieur = Groupes.GSuperieur;
            GroupeInferieur = Groupes.GInferieur;

            // --------------------------------------------------------------------------------------------------
            // verifier si la table de base ne fait pas déjà parti de la liste des SELECT
            // --------------------------------------------------------------------------------------------------
            Regex rxSelect = new Regex(string.Format(@"^.*\'-----(?:\s|\t){0}_{1}(?:\s|\t)-----\'(?:\s|\t)+AS(?:\s|\t)+\[-----(?:\s|\t){0}_{1}(?:\s|\t)-----\](?:\s|\t)*\,.*$", wd.selectedTable, wd.selectedAcronyme), RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (rxSelect.Matches(TexteRequete).Count == 0)
            {
                Groupes = BatirGroupeSuperieurEtBonification(true, wd.selectedTable, wd.selectedAcronyme, wd.selectedTable, TexteRequete, GroupeSuperieur, GroupeInferieur, wd.derniereLigneRequete - wd.premiereLigneRequete + 1);
                GroupeSuperieur = Groupes.GSuperieur;
                GroupeInferieur = Groupes.GInferieur;
            }

            #endregion
            //  ===================================================================================================================================================================================================================================================
            //  ===================================================================================================================================================================================================================================================

            Match m2;
            (from   Match m in new Regex(@"(\w+)(\.Description\)\s*AS\s*\[)(\w+\])").Matches(GroupeSuperieur) 
             select (mText:m.Groups[0].Value, sfx: m.Groups[1].Value, grp2: m.Groups[2].Value, grp3: m.Groups[3].Value))
                .ToList()
                .ForEach(   vTypesDesc => { 
                    if ((m2 = new Regex($@"{vTypesDesc.sfx}\s+ON\s+{vTypesDesc.sfx}\.UniqueId\s*=\s*(\w+)\.\w+").Match(GroupeInferieur)).Success)
                        GroupeSuperieur = GroupeSuperieur.Replace(vTypesDesc.mText, $"{vTypesDesc.sfx}{vTypesDesc.grp2}{m2.Groups[1].Value}.{vTypesDesc.grp3}"); });

            wd.groupeSuperieur  = GroupeSuperieur.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            wd.groupeInferieur  = GroupeInferieur.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            wd.otherTable       = AutreTable;
        }

    } // fin class SirDBSidekickLogic
}

