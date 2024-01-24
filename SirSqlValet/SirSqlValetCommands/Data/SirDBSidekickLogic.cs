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
    public static class SirDBSidekickLogic
    {
        public  static  string  ProcessScriptAndSelectedLine()
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
            if ((matchesAroundSelectedTable = (new Regex($@"^.*(?:FROM|JOIN)\s+((?'prefix'\w+)\.)?{wd.selectedTable}\s+(?'acronyme'\w+).*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)).Matches(wd.SafeGetLine(wd.numeroLigneCurseur))).Count == 1)
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

        public  static  void    Join(string userSelection, string joinType)
        {
            //  ===================================================================================================================================================================================================================================================
            //  ===================================================================================================================================================================================================================================================
            #region REG_process

            string      GroupeSuperieur = string.Empty;
            string      GroupeInferieur = string.Empty;

            string      Champ           = string.Empty;
            string[]    Champs          = new string[] { };
            string      AutreTable      = string.Empty;
            string      AutreChamp      = string.Empty;
            string[]    AutresChamps    = new string[] { };

            bool        MultiSegement   = false;

            Func<string, string, string> ExpandTablePointEtoile = (table, tableAcronyme) =>
            {
                string debuteTermine = "";
                string reste = "";

                foreach (COLUMN c in BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() == table.ToUpper()).OrderBy(c => c.ORDINAL_POSITION))
                {
                    string ajout = tableAcronyme + "." + c.COLUMN_NAME;
                    if (c.COLUMN_TYPE.ToUpper().Contains("MAX") && (c.COLUMN_TYPE.ToUpper().StartsWith("VARCHAR") || c.COLUMN_TYPE.ToUpper().StartsWith("NVARCHAR")))
                        ajout = $"CONVERT(NVARCHAR, {ajout}) AS [{c.COLUMN_NAME}]";

                    if ((c.COLUMN_NAME.ToUpper() == "ADEBUTE") || (c.COLUMN_NAME.ToUpper() == "ATERMINE"))
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
                string  PossibleAcronymeRoot    = BuildPossibleAcronyme(p, t);
                string  PossibleAcronyme        = PossibleAcronymeRoot;
                int     suffixe                 = 2;

                while (true)
                {
                    if (new Regex($@"FROM\s+\w+\s+{PossibleAcronyme}(\s|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
                        if (new Regex($@"JOIN\s+(\w+\.)?\w+\s+{PossibleAcronyme}\s+ON\s.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
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

                List<COLUMN> fColsTypes = BD_Schema.columns.Where(c => { return (c.TABLE_NAME.ToUpper() == fTable.ToUpper()) && (c.COLUMN_NAME.ToUpper().StartsWith("aPour".ToUpper())); }).ToList();
                if (fColsTypes.Count >= 1)
                {
                    foreach (COLUMN ColType in fColsTypes)
                    {
                        string ViewTypeName = ColType.COLUMN_NAME.Substring("aPour".Length);
                        bool ViewsOnly = !(ColType.COLUMN_NAME.ToUpper().StartsWith("aPourType")) && !(ViewTypeName.ToUpper() == "TypeDocument".ToUpper() && !(ViewTypeName.ToUpper() == "Langue".ToUpper()));

                        VIEW v = BD_Schema.views.FirstOrDefault(view => { return view.VIEW_NAME.ToUpper() == ViewTypeName.ToUpper(); });
                        if (!(v is null))
                        {
                            string A_VIEW_NAME = acronyme(v.VIEW_SCHEMA, v.VIEW_NAME, fTexteRequete + fGroupeInferieur, fNbrLigneRequete);
                            int size = BD_Schema.vcolumns.Single(vc => { return vc.VIEW_SCHEMA.ToUpper() == v.VIEW_SCHEMA.ToUpper() && vc.VIEW_NAME.ToUpper() == v.VIEW_NAME.ToUpper() && vc.COLUMN_NAME.ToUpper() == "DESCRIPTIONFRANCAIS"; }).COLUMN_MAX_WIDTH;
                            Insertion += string.Format(", CONVERT(NVARCHAR({2}), {0}.Description) AS [{1}]", A_VIEW_NAME, ViewTypeName, size);
                            if (fBase)
                                fGroupeInferieur = string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", v.VIEW_SCHEMA + "." + v.VIEW_NAME, A_VIEW_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME) + Environment.NewLine + fGroupeInferieur;
                            else
                                fGroupeInferieur += Environment.NewLine + string.Format("{0} JOIN  {1} {2} ON {2}.{3} = {4}.{5}", "LEFT ", v.VIEW_SCHEMA + "." + v.VIEW_NAME, A_VIEW_NAME, "UniqueId", fAcronyme, ColType.COLUMN_NAME);
                        }
                        else if (!ViewsOnly)
                        {
                            TABLE t = BD_Schema.tables.FirstOrDefault(table => { return table.TABLE_NAME.ToUpper() == ViewTypeName.ToUpper(); });

                            COLUMN c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.ToUpper() == ViewTypeName.ToUpper() && colonne.COLUMN_NAME.ToUpper() == "DescriptionFrancais".ToUpper(); });
                            if (c is null)
                                c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.ToUpper() == ViewTypeName.ToUpper() && colonne.COLUMN_NAME.ToUpper() == "Description".ToUpper(); });

                            if (c is null)
                                c = BD_Schema.columns.FirstOrDefault(colonne => { return colonne.TABLE_NAME.ToUpper() == ViewTypeName.ToUpper() && (colonne.COLUMN_TYPE.ToUpper().StartsWith("VARCHAR") || colonne.COLUMN_TYPE.ToUpper().StartsWith("NVARCHAR") || colonne.COLUMN_TYPE.ToUpper().StartsWith("CHAR") || colonne.COLUMN_TYPE.ToUpper().StartsWith("NCHAR")); });

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
            Regex rxSelect = new Regex(string.Format(@"^.*\'-----\s{0}_{1}\s-----\'\s+AS\s+\[-----\s{0}_{1}\s-----\]\s*\,.*$", wd.selectedTable, wd.selectedAcronyme), RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (rxSelect.Matches(TexteRequete).Count == 0)
            {
                Groupes = BatirGroupeSuperieurEtBonification(true, wd.selectedTable, wd.selectedAcronyme, wd.selectedTable, TexteRequete, GroupeSuperieur, GroupeInferieur, wd.derniereLigneRequete - wd.premiereLigneRequete + 1);
                GroupeSuperieur = Groupes.GSuperieur;
                GroupeInferieur = Groupes.GInferieur;
            }

            #endregion
            //  ===================================================================================================================================================================================================================================================
            //  ===================================================================================================================================================================================================================================================

            wd.groupeSuperieur  = GroupeSuperieur.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            wd.groupeInferieur  = GroupeInferieur.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            wd.otherTable       = AutreTable;
        }

        public  static  void    TraitementAvecAutreTable()
        {
            // ==============================================================================================================================
            // ==============================================================================================================================
            //
            // S O R T I E   P O S S I B L E
            //
            //     s'il n'y a pas de code à la sortie du dialogue
            //     donc si aucune sélection n'ait été faite
            //
            // ==============================================================================================================================
            // ==============================================================================================================================
            if (wd.groupeSuperieur.Count == 0 || wd.groupeInferieur.Count == 0)
                throw new Exception("Aucune sélection ne semble avoir été faite");

            Func<List<string>, int, List<string>> AjouteMarge = (ls, largeur) =>
            {
                string marge = new string(' ', largeur);
                for (int i = 0; i < ls.Count; i++)
                    ls[i] = marge + ls[i];

                return ls;
            };

            for (int i = 0; i < wd.groupeSuperieur.Count; i++)
            {
                if (wd.groupeSuperieur[i].Contains("Individu_I"))
                {
                    Match m;

                    // line for Individu if no alias repeat
                    string line = ",'----- Individu_I -----' AS [----- Individu_I -----], I.numeroClient, ' ' [ ], I.nomFamille, I.prenom, I.secondPrenom, I.personneResponsable, CONVERT(NVARCHAR(16), vEM.Description) AS [EtatMatrimonial], CONVERT(NVARCHAR(8), vS.Description) AS [Sexe], CONVERT(NVARCHAR(3), vSN.Description) AS [SuffixeNom], CONVERT(NVARCHAR(10), vT.Description) AS [Titre], CONVERT(NVARCHAR(21), vTD.Description) AS [TypeDelegation], I.UniqueId, I.aPourEtatMatrimonial, I.aPourLangueCorrespondance, I.aPourSexe, I.aPourSuffixeNom, I.aPourTitre, I.aPourTypeDelegation, I.dateDeces, I.dateNaissance, I.estHorsCanada, I.estPrestataire";

                    // check and fix alias repeat number for I.
                    if ((m = Regex.Match(wd.groupeSuperieur[i], @"I(\d)\.UniqueId")).Success)
                        line = line.Replace("_I ", $"_I{m.Groups[1]} ").Replace("I.", $"I{m.Groups[1]}.");

                    // check and fix alias repeat number for these alias : "vEM", "vS", "vSN", "vT", "vTD"
                    foreach (string prefix in new string[] { "vEM", "vS", "vSN", "vT", "vTD" })
                        if ((m = Regex.Match(wd.groupeSuperieur[i], $@"{prefix}(\d).Description")).Success)
                            line = line.Replace($"{prefix}.", $"{prefix}{m.Groups[1]}.");

                    wd.groupeSuperieur[i] = line;
                }
            }

            // ------------------------------------------------------------------------------------------------------------------------------
            // On se debarasse de l'* si presente dans SELECT *
            // ------------------------------------------------------------------------------------------------------------------------------

            bool    presenceEtoile  = false;
            string  debut           = string.Empty;
            string  reste           = string.Empty;
            string  espaces         = string.Empty;

            foreach (int i in RangeFromTo(wd.premiereLigneRequete, wd.numeroLigneCurseur))
            {
                MatchCollection matchesForSelect;
                if ((matchesForSelect = (new Regex(@"(?'debut'^.*SELECT)(?'etoile'(?'espaces'\s*)\*)?(?'reste'.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)).Matches(wd.SafeGetLine(i))).Count == 1)
                    foreach (Group group in matchesForSelect[0].Groups)
                        if (group.Success)
                            if (group.Name == "etoile")
                                presenceEtoile = true;
                            else if (group.Name == "reste")
                                reste = group.ToString();
                            else if (group.Name == "debut")
                                debut = group.ToString();
                            else if (group.Name == "espaces")
                                espaces = group.ToString();

                if (presenceEtoile)
                {
                    wd.SafeSetLine(i, debut + espaces + reste);
                    break;
                }
            }

            //  ------------------------------------------------------------------------------------------------------------------------------
            //  On ajoute le decorateur principal sur la ligne du select
            //      mais seulement si yé pas déjà là 
            //   -> !!! MODIFICATION DU SCRIPT !!!
            //  ------------------------------------------------------------------------------------------------------------------------------

            string otherTableWithSpaces = "";
            for (int index = 0; index < wd.otherTable.Length; index++)
            {
                if (index > 0 && wd.otherTable[index].ToString() == wd.otherTable[index].ToString().ToUpper())
                    otherTableWithSpaces += " ";

                otherTableWithSpaces += wd.otherTable[index].ToString().ToUpper();
            }

            string  select          = "SELECT";
            bool    selectFinal     = Regex.IsMatch(wd.SafeGetLine(wd.premiereLigneRequete), $@"^(\s|\t)*{select}(\s|\t)*'(\s|\t)*'(\s|\t)*\[(\s|\t)*\](\s|\t)*$");
            if (!selectFinal)
            {
                string  premiereLigne   = wd.SafeGetLine(wd.premiereLigneRequete);
                string  selectFinalText = $"{select} '' [ ]";
                int     pos             = -1;
                if ((premiereLigne.ToUpper().Contains(select)) && (premiereLigne.Substring((pos = premiereLigne.ToUpper().IndexOf(select)), select.Length) != select))
                    premiereLigne = premiereLigne.Replace(premiereLigne.Substring(pos, select.Length), select);

                premiereLigne = premiereLigne.Substring(0, premiereLigne.IndexOf(select) + select.Length).Replace(select, selectFinalText);
                wd.SafeSetLine(wd.premiereLigneRequete, premiereLigne);
            }

            string  equal5          = "=====";
            string  titleElement    = $",'{equal5} {otherTableWithSpaces} {equal5}' AS  [{equal5} {otherTableWithSpaces} {equal5}]";
            string  deuxiemeLigne   = wd.SafeGetLine(wd.premiereLigneRequete + 1);
            if (deuxiemeLigne.Trim() != titleElement)
            {
                string  premiereLigne           = wd.SafeGetLine(wd.premiereLigneRequete);
                bool    alreadyATitleElement    = Regex.IsMatch(deuxiemeLigne, $@",'{equal5}\s.*\s{equal5}'(\s|\t)+AS(\s|\t)+\[{equal5}\s.*\s{equal5}\]");
                
                deuxiemeLigne = premiereLigne.Substring(0, premiereLigne.IndexOf(select)) + titleElement;
                
                if (alreadyATitleElement)
                    wd.SafeSetLine(wd.premiereLigneRequete + 1, deuxiemeLigne);
                else
                {
                    wd.scriptLines.Insert(wd.premiereLigneRequete + 1, deuxiemeLigne);
                    wd.numeroLigneCurseur++;
                }
            }

            // ------------------------------------------------------------------------------------------------------------------------------
            // insertion des lignes de code dans les FROM/JOIN
            // ------------------------------------------------------------------------------------------------------------------------------
            MatchCollection mctemp;
            int largeurMarge = (mctemp = (new Regex(@"^\s*(.)", RegexOptions.Compiled | RegexOptions.Singleline)).Matches(wd.SafeGetLine(wd.numeroLigneCurseur))).Count == 1 ? mctemp[0].Groups[1].Index : 0;
            
            if (wd.LineInsertInferieur <= wd.derniereLigneRequete)
                wd.scriptLines.InsertRange(wd.LineInsertInferieur, AjouteMarge(wd.groupeInferieur, largeurMarge));
            else if (wd.derniereLigneRequete <= wd.scriptLines.Count - 1)
                wd.scriptLines.InsertRange(wd.LineInsertInferieur, AjouteMarge(wd.groupeInferieur, largeurMarge));
            else
                wd.scriptLines.AddRange(AjouteMarge(wd.groupeInferieur, largeurMarge));

            // ------------------------------------------------------------------------------------------------------------------------------
            // insertion des lignes de code dans le SELECT
            // ajustement de la position initiale du curseur en fonction du nombre de lignes insérées dans le SELECT
            // ------------------------------------------------------------------------------------------------------------------------------
            wd.scriptLines.InsertRange(wd.LineInsertSuperieur, AjouteMarge(wd.groupeSuperieur, largeurMarge));
            wd.numeroLigneCurseur += wd.groupeSuperieur.Count;

            // ------------------------------------------------------------------------------------------------------------------------------
            // ------------------------------------------------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------------------------------------------------
            // extraction des lignes du SELECT et des lignes FROM/JOIN
            // ------------------------------------------------------------------------------------------------------------------------------
            List<List<Tuple<string, int, int>>> SuperListeAlignement = new List<List<Tuple<string, int, int>>>();
            List<Regex> ListRegExSelectionLigne = new List<Regex>();

            SuperListeAlignement.Add(new List<Tuple<string, int, int>>());
            SuperListeAlignement.Add(new List<Tuple<string, int, int>>());

            ListRegExSelectionLigne.Add(new Regex(@"[-=]\'\s+(AS)\s+\[[-=]", RegexOptions.Compiled | RegexOptions.IgnoreCase));
            ListRegExSelectionLigne.Add(new Regex(@"(?:JOIN|FROM)\s+((\w+|\[\w+\])|(\w+|\[\w+\])\.(\w+|\[\w+\]))\s+\w+(?:\s+|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase));

            foreach (int ligne in RangeFromTo(wd.premiereLigneRequete, wd.derniereLigneRequete))
                foreach (int i in RangeFromTo(0, 1))
                    if (ListRegExSelectionLigne[i].Matches(wd.SafeGetLine(ligne)).Count > 0)
                        SuperListeAlignement[i].Add(new Tuple<string, int, int>(wd.SafeGetLine(ligne), ligne, int.MaxValue));

            List<List<string>> ListRegExReperage = new List<List<string>>();
            ListRegExReperage.Add(new List<string>());
            ListRegExReperage.Add(new List<string>());

            // ------------------------------------------------------------------------------------------------------------------------------
            // regex pour l'alignement des lignes FROM/JOIN
            // ------------------------------------------------------------------------------------------------------------------------------
            Func<string, IEnumerable<(string pattern, string value)>, string> R = (targetContainer, findReplace) =>
            {
                findReplace.ToList().ForEach(pair => targetContainer = targetContainer.Replace(pair.pattern, pair.value));
                return targetContainer;
            };

            // ------------------------------------------------------------------------------------------------------------------------------
            // token regex (d,ta,a,...) qui contiennent le regex à utiliser
            //     ex : les occurences de "d." seront remplacées par le contenu de la variable d, donc par @"(a.|a.\.(\*|Description(Francais)?))"
            //
            // l'ordre doit rester le même que celui-ci pcq
            //     ex : "d." est remplacé par @"(a.|a.\.(\*|Description(Francais)?))" 
            //          qui lui-même contient des "a."
            //     ex : "a." est remplacé par @"w." 
            //          qui contient un "a."
            // ------------------------------------------------------------------------------------------------------------------------------
            string d    = @"(a.|a.\.(\*|Description(Francais)?))";      // "d."
            string ta   = @"(w.|f.)";                                   // "t."
            string a    = @"w.";                                        // "a."
            string f    = @"w.\.w.";                                    // "f."           
            string ao   = @"(AND|OR)";                                  // "ao."
            string cd   = @"(?<e>";                                     // "{"
            string cf   = @")";                                         // "}"
            string o    = @"[<=>]{1,2}";                                // "o."
            string w    = @"(\w+|\[\w+\])";                             // "w."
            string wb   = @"\[[-=]*_(\w|\s)+_[-=]*\]";                  // "wb."
            string wp   = @"[ ,]\'[-=]*_(\w|\s)+_[-=]*\'";              // "wp."
            string __   = @"\s+";                                       // "__"
            string _    = @"\s*";                                       // "_"

            // ------------------------------------------------------------------------------------------------------------------------------
            // regex pour l'alignement des lignes du SELECT
            // ------------------------------------------------------------------------------------------------------------------------------
            ListRegExReperage[0].Add(R(@"{wp.}__",                                                                                              new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__{AS}__",                                                                                          new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__{wb.}",                                                                                       new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_{d.}",                                                                                   new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__{AS}__",                                                                             new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__{wb.}",                                                                          new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_{d.}",                                                                      new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__{AS}__",                                                                new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__{wb.}",                                                             new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_{d.}",                                                         new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_d.__{AS}__",                                                   new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_d.__AS__{wb.}",                                                new[] { ("d.", d),             ("a.", a),                         ("{", cd), ("}", cf),            ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));

            // ------------------------------------------------------------------------------------------------------------------------------
            // regex pour l'alignement des lignes de la zone FROM/JOIN
            // ------------------------------------------------------------------------------------------------------------------------------
            ListRegExReperage[1].Add(R(@"{(INNER|OUTTER|LEFT|RIGHT|FROM)}__",                                                                   new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R( @"(INNER|OUTTER|LEFT|RIGHT)__{JOIN}__",                                                                 new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__{t.}__",                                                       new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__{a.}",                                                     new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__{ON}__",                                               new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__{f.}",                                             new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__{o.}__",                                       new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__{f.}",                                     new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__{ao.}__",                              new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__{f.}",                            new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__{o.}",                        new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__{f.}",                    new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__{ao.}__",             new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__{f.}",           new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__f.__{o.}",       new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__f.__o.__{.*}",   new[] {            ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w),                           ("__", __), ("_", _) }));

            // ------------------------------------------------------------------------------------------------------------------------------
            // alignement proprement dit
            // ------------------------------------------------------------------------------------------------------------------------------
            foreach (int i in new int[] { 0, 1 })
            {
                foreach (string srx in ListRegExReperage[i])
                {
                    List<Tuple<string, int, int>> temp = new List<Tuple<string, int, int>>();

                    Regex rx = new Regex(srx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    int Pos = int.MinValue;
                    int Count = 0;

                    foreach (Tuple<string, int, int> T in SuperListeAlignement[i])
                    {
                        MatchCollection mc;
                        Group g;

                        if ((mc = rx.Matches(T.Item1)).Count == 1 && !((g = mc[0].Groups["e"]) is null))
                        {
                            if (g.Index > Pos)
                                Pos = g.Index;

                            temp.Add(new Tuple<string, int, int>(T.Item1, T.Item2, g.Index));

                            Count++;
                        }
                        else
                            temp.Add(T);
                    }

                    if (Count > 1)
                    {
                        while ((Pos % 4) != 0)
                            Pos++;

                        SuperListeAlignement[i].Clear();
                        foreach (Tuple<string, int, int> T in temp)
                            SuperListeAlignement[i].Add(new Tuple<string, int, int>(T.Item3 >= Pos ? T.Item1 : T.Item1.Insert(T.Item3, (new string(' ', Pos - T.Item3))), T.Item2, int.MaxValue));
                    }
                }

                foreach (Tuple<string, int, int> T in SuperListeAlignement[i])
                    wd.SafeSetLine(T.Item2, T.Item1);
            }

            return;
        }   // fin ProcessScriptAndSelectedLine(...)

    } // fin class SirDBSidekickLogic
}

