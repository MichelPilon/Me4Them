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
        public static void TraitementAvecAutreTable()
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
                    string line = ",'----- Individu_I -----' AS [----- Individu_I -----], I.numeroClient [I.numeroClient], ' ' [ ], I.nomFamille [I.nomFamille], I.prenom [I.prenom], I.secondPrenom [I.secondPrenom], I.personneResponsable [I.personneResponsable], CONVERT(NVARCHAR(16), vEM.Description) AS [I.EtatMatrimonial], CONVERT(NVARCHAR(8), vL.Description) AS [I.LangueCorrespondance], CONVERT(NVARCHAR(8), vS.Description) AS [I.Sexe], CONVERT(NVARCHAR(3), vSN.Description) AS [I.SuffixeNom], CONVERT(NVARCHAR(10), vT.Description) AS [I.Titre], CONVERT(NVARCHAR(21), vTD.Description) AS [I.TypeDelegation], I.UniqueId [I.UniqueId], I.dateDeces [I.dateDeces], I.dateNaissance [I.dateNaissance], I.estHorsCanada [I.estHorsCanada], I.estPrestataire [I.estPrestataire], I.aPourEtatMatrimonial [I.aPourEtatMatrimonial], I.aPourLangueCorrespondance [I.aPourLangueCorrespondance], I.aPourSexe [I.aPourSexe], I.aPourSuffixeNom [I.aPourSuffixeNom], I.aPourTitre [I.aPourTitre], I.aPourTypeDelegation [I.aPourTypeDelegation]";

                    // check and fix alias repeat number for I.
                    if ((m = Regex.Match(wd.groupeSuperieur[i], @"I(\d)\.UniqueId")).Success)
                        line = line.Replace("_I ", $"_I{m.Groups[1]} ").Replace("I.", $"I{m.Groups[1]}.");

                    // check and fix alias repeat number for these alias : "vEM", "vS", "vSN", "vT", "vTD"
                    foreach (string prefix in new string[] { "vEM", "vS", "vSN", "vT", "vTD", "vL" })
                        if ((m = Regex.Match(wd.groupeSuperieur[i], $@"{prefix}(\d).Description")).Success)
                            line = line.Replace($"{prefix}.", $"{prefix}{m.Groups[1]}.");

                    wd.groupeSuperieur[i] = line;
                }
            }

            // ------------------------------------------------------------------------------------------------------------------------------
            // On se debarasse de l'* si presente dans SELECT *
            // ------------------------------------------------------------------------------------------------------------------------------

            bool presenceEtoile = false;
            string debut = string.Empty;
            string reste = string.Empty;
            string espaces = string.Empty;

            foreach (int i in RangeFromTo(wd.premiereLigneRequete, wd.numeroLigneCurseur))
            {
                MatchCollection matchesForSelect;
                if ((matchesForSelect = (new Regex(@"(?'debut'^.*SELECT)(?'etoile'(?'espaces'(?:\s|\t)*)\*)?(?'reste'.*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)).Matches(wd.SafeGetLine(i))).Count == 1)
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

            string select = "SELECT";
            bool selectFinal = Regex.IsMatch(wd.SafeGetLine(wd.premiereLigneRequete), $@"^(?:\s|\t)*{select}(?:\s|\t)*'(?:\s|\t)*'(?:\s|\t)*\[(?:\s|\t)*\](?:\s|\t)*$");
            if (!selectFinal)
            {
                string premiereLigne = wd.SafeGetLine(wd.premiereLigneRequete);
                string selectFinalText = $"{select} '' [ ]";
                int pos = -1;
                if ((premiereLigne.ToUpper().Contains(select)) && (premiereLigne.Substring((pos = premiereLigne.ToUpper().IndexOf(select)), select.Length) != select))
                    premiereLigne = premiereLigne.Replace(premiereLigne.Substring(pos, select.Length), select);

                premiereLigne = premiereLigne.Substring(0, premiereLigne.IndexOf(select) + select.Length).Replace(select, selectFinalText);
                wd.SafeSetLine(wd.premiereLigneRequete, premiereLigne);
            }

            string equal5 = "=====";
            string titleElement = $",'{equal5} {otherTableWithSpaces} {equal5}' AS  [{equal5} {otherTableWithSpaces} {equal5}]";
            string deuxiemeLigne = wd.SafeGetLine(wd.premiereLigneRequete + 1);
            if (deuxiemeLigne.Trim() != titleElement)
            {
                string premiereLigne = wd.SafeGetLine(wd.premiereLigneRequete);
                bool alreadyATitleElement = Regex.IsMatch(deuxiemeLigne, $@",'{equal5}(?:\s|\t).*(?:\s|\t){equal5}'(?:\s|\t)+AS(?:\s|\t)+\[{equal5}(?:\s|\t).*(?:\s|\t){equal5}\]");

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
            int largeurMarge = (mctemp = (new Regex(@"^(?:\s|\t)*(.)", RegexOptions.Compiled | RegexOptions.Singleline)).Matches(wd.SafeGetLine(wd.numeroLigneCurseur))).Count == 1 ? mctemp[0].Groups[1].Index : 0;

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

            ListRegExSelectionLigne.Add(new Regex(@"[-=]\'(?:\s|\t)+(AS)(?:\s|\t)+\[[-=]", RegexOptions.Compiled | RegexOptions.IgnoreCase));
            ListRegExSelectionLigne.Add(new Regex(@"(?:JOIN|FROM)(?:\s|\t)+((\w+|\[\w+\])|(\w+|\[\w+\])\.(\w+|\[\w+\]))\s+\w+(?:(?:\s|\t)+|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase));

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
            string d = @"(a.|a.\.(\*|Description(Francais)?))";      // "d."
            string ta = @"(w.|f.)";                                   // "t."
            string a = @"w.";                                        // "a."
            string f = @"w.\.w.";                                    // "f."           
            string ao = @"(AND|OR)";                                  // "ao."
            string cd = @"(?<e>";                                     // "{"
            string cf = @")";                                         // "}"
            string o = @"[<=>]{1,2}";                                // "o."
            string w = @"(\w+|\[\w+\])";                             // "w."
            string wb = @"\[[-=]*_(\w|(?:\s|\t))+_[-=]*\]";           // "wb."
            string wp = @"[ ,]\'[-=]*_(\w|(?:\s|\t))+_[-=]*\'";       // "wp."
            string __ = @"(?:\s|\t)+";                                // "__"
            string _ = @"(?:\s|\t)*";                                // "_"

            // ------------------------------------------------------------------------------------------------------------------------------
            // regex pour l'alignement des lignes du SELECT
            // ------------------------------------------------------------------------------------------------------------------------------
            ListRegExReperage[0].Add(R(@"{wp.}__", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__{AS}__", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__{wb.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_{d.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__{AS}__", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__{wb.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_{d.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__{AS}__", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__{wb.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_{d.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_d.__{AS}__", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));
            ListRegExReperage[0].Add(R(@"wp.__AS__wb.,_d.__AS__wb.,_d.__AS__wb.,_d.__AS__{wb.}", new[] { ("d.", d), ("a.", a), ("{", cd), ("}", cf), ("w.", w), ("wb.", wb), ("wp.", wp), ("__", __), ("_", _) }));

            // ------------------------------------------------------------------------------------------------------------------------------
            // regex pour l'alignement des lignes de la zone FROM/JOIN
            // ------------------------------------------------------------------------------------------------------------------------------
            ListRegExReperage[1].Add(R(@"{(INNER|OUTTER|LEFT|RIGHT|FROM)}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"(INNER|OUTTER|LEFT|RIGHT)__{JOIN}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__{t.}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__{a.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__{ON}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__{f.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__{o.}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__{f.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__{ao.}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__{f.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__{o.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__{f.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__{ao.}__", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__{f.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__f.__{o.}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));
            ListRegExReperage[1].Add(R(@"((INNER|OUTTER|LEFT|RIGHT)__JOIN|FROM)__t.__a.__ON__f.__o.__f.__ao.__f.__o.__f.__ao.__f.__o.__{.*}", new[] { ("t.", ta), ("a.", a), ("f.", f), ("ao.", ao), ("{", cd), ("}", cf), ("o.", o), ("w.", w), ("__", __), ("_", _) }));

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
