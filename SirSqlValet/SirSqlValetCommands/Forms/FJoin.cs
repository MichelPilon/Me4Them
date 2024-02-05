using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using System.Windows.Forms;

using SirSqlValetCommands;
using SirSqlValetCommands.Data;

using static SirSqlValetCommands.Data.SVCGlobal;
using static SirSqlValetCommands.Data.GCSS;
using static SirSqlValetCommands.Data.Extensions;

namespace SirSqlValetCommands
{
    public partial class FJoin : Form
    {
        private bool throughMyShowDialog = false;

        int[] sel123 = new[] { -1, -1, -1 };
        List<List<string>> list123;
        List<ListBox> listbox123;

        List<string> list1 = new List<string> ();
        List<string> list2 = new List<string> ();
        List<string> list3 = new List<string> ();
        List<string> list4 = new List<string> ();
        List<string> list5 = new List<string> ();

        List<int> listSelectedIndexList4 = new List<int> ();
        List<int> listSelectedIndexList5 = new List<int> ();

        string titreGauche = "";
        string titreDroite = "";

        int nvh1 = 25;
        int nvh2 = 30;
        int nvh3 = 35;

        public FJoin()
        {
            InitializeComponent();

            splMain.Panel1.BackColor    = GCSS.SteelBlue_Dark.ToColor();

            bJoin.ForeColor             = GCSS.SteelBlue_Light.ToColor();
            bCancel.ForeColor           = GCSS.SteelBlue_Light.ToColor();

            bJoin.BackColor             = GCSS.SteelBlue.ToColor();
            bCancel.BackColor           = GCSS.SteelBlue.ToColor();

            listBox1.ForeColor          = GCSS.SteelBlue_Light.ToColor();
            listBox2.ForeColor          = GCSS.SteelBlue_Light.ToColor();
            listBox3.ForeColor          = GCSS.SteelBlue_Light.ToColor();
            listBox4.ForeColor          = Color.Black;
            listBox5.ForeColor          = Color.Black;

            listBox1.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox2.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox3.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox4.BackColor          = GCSS.SteelBlue.ToColor();
            listBox5.BackColor          = GCSS.SteelBlue.ToColor();

            listBox1.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox2.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox3.BackColor          = GCSS.SteelBlue_Dark.ToColor();
            listBox4.BackColor          = GCSS.SteelBlue.ToColor();
            listBox5.BackColor          = GCSS.SteelBlue.ToColor();

            list123 = new[] { list1, list2, list3 }.ToList();
            listbox123 = new[] { listBox1, listBox2, listBox3 }.ToList();

            #region REG_init
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
                string PossibleAcronymeRoot = BuildPossibleAcronyme(p, t);
                string PossibleAcronyme = PossibleAcronymeRoot;
                int suffixe = 2;

                while (true)
                {
                    if (new Regex(string.Format(@"FROM\s+\w+\s+{0}(\s|$)", PossibleAcronyme), RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
                        if (new Regex(string.Format(@"JOIN\s+(\w+\.)?\w+\s+{0}\s+ON\s.*$", PossibleAcronyme), RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(texteRecherche).Count == 0)
                            break;

                    // --------------------------------------------------------------------------------------------------
                    // comme il ne peut pas y avoir plus de sequence d'un préfixe qi'il y a de lignes dans la requête
                    // le nombre de lignes de la requête est un bon critère de détection le loop enragée!!!
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
            // on affiche le nom de la table de base
            // -------------------------------------------------------------------------------
            titreGauche = wd.selectedTable; // table

            bool PeuplerListbox3 = false;

            // -------------------------------------------------------------------------------
            // remplir le ListBox de gauche
            // dépend juste de la table sélectionnée
            // -------------------------------------------------------------------------------
            foreach (COLUMN c in BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()).OrderBy(c => c.ORDINAL_POSITION))
            {
                list4.Add(c.COLUMN_NAME + " (" + c.COLUMN_TYPE + ")");
                if (c.COLUMN_NAME.ToUpper() == "UniqueId".ToUpper())
                    PeuplerListbox3 = true;
            }

            // -------------------------------------------------------------------------------
            // remplir le ListBox central du haut pour les FK
            // qui partent de la table sélectionnée vers d'autres tables
            //
            // à partir de toutes les FK,
            // prendre seulement celles qui concernent la table sélectionnée
            // on évite aussi certains cas techniques
            // en triant ça par la position de la colonne
            // pour afficher dans le même ordre que dans l'object browser de SSMS
            // -------------------------------------------------------------------------------

            // -------------------------------------------------------------------------------
            // chercher les FK pertinentes
            // et trier par FK_NAME pour regrouper ensemble les FK multi-segement
            // -------------------------------------------------------------------------------
            var fks = from fk
                      in BD_Schema.fks
                      join c in BD_Schema.columns on fk.TABLE_NAME + fk.COLUMN_NAME equals c.TABLE_NAME + c.COLUMN_NAME
                      where fk.TABLE_NAME.ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()
                              && !(fk.FK_TABLE_NAME.ToUpper().StartsWith("xValueType".ToUpper()))
                              && !(fk.COLUMN_NAME.ToUpper().StartsWith("aPourType".ToUpper()))
                              && !(fk.FK_TABLE_NAME.ToUpper().StartsWith("Auditable".ToUpper()))
                      orderby fk.FK_NAME, c.ORDINAL_POSITION
                      select fk;

            // -------------------------------------------------------------------------------
            // obtenir la liste des FK_NAME unique
            // -------------------------------------------------------------------------------
            var ufks = from fk
                       in fks.ToList().Distinct()
                       select fk.FK_NAME;

            // -------------------------------------------------------------------------------
            // batir petite liste temporaire des insertions pour comparaison
            // avec les FK 'molles'
            // -------------------------------------------------------------------------------
            var ElementsAjoutes = new List<string>();

            // -------------------------------------------------------------------------------
            // pour chaque FK_NAME unique
            // -------------------------------------------------------------------------------
            foreach (string fkname in ufks)
            {
                // -------------------------------------------------------------------------------
                // cas d'une FK simple segment
                // -------------------------------------------------------------------------------
                if (fks.Count(fk => fk.FK_NAME == fkname) == 1)
                {
                    FK fk = fks.First(fk2 => fk2.FK_NAME == fkname);
                    list1.Add(fk.COLUMN_NAME + " -> " + fk.FK_TABLE_NAME + "." + fk.FK_COLUMN_NAME);
                    ElementsAjoutes.Add(fk.COLUMN_NAME + " -> " + fk.FK_TABLE_NAME + "." + fk.FK_COLUMN_NAME);
                }
                // -------------------------------------------------------------------------------
                // cas d'une FK multi-segement
                // -------------------------------------------------------------------------------
                else
                {
                    string gauche = "";
                    string droite = "";
                    string fktable = "";
                    foreach (FK fk in fks.Where(fk2 => fk2.FK_NAME == fkname))
                    {
                        gauche += ((gauche == string.Empty) ? "(" : ", ") + fk.COLUMN_NAME;
                        droite += ((droite == string.Empty) ? "(" : ", ") + fk.FK_COLUMN_NAME;
                        fktable = (fktable == string.Empty) ? fk.FK_TABLE_NAME : fktable;
                    }
                    gauche += ")";
                    droite += ")";
                    list1.Add(gauche + " -> " + fktable + "." + droite);
                    ElementsAjoutes.Add(gauche + " -> " + fktable + "." + droite);
                }
            }

            // -------------------------------------------------------------------------------
            // aller chercher la liste des FK 'molles' et, pour chaque...
            // -------------------------------------------------------------------------------
            //string possibleTableName = "";
            //foreach (COLUMN c in BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() == Table.ToUpper() && c.COLUMN_NAME.ToUpper().StartsWith("aPour".ToUpper()) && !c.COLUMN_NAME.ToUpper().StartsWith("aPourType".ToUpper())).OrderBy(c => c.ORDINAL_POSITION))
            //    if (BD_Schema.tables.Any(t => t.TABLE_NAME.ToUpper() == (possibleTableName = c.COLUMN_NAME.Substring("aPour".Length)).ToUpper()))
            //        if (!(ElementsAjoutes.Any(s => s.Trim() == c.COLUMN_NAME + " -> " + possibleTableName + ".UniqueId")))  // si la FK 'molle' est pas déjà là, on l'ajoute
            //            listBox1.Items.Add(c.COLUMN_NAME + " => " + possibleTableName + ".UniqueId");


            string AuditRecordIdFK = "AuditRecordId -> Audit.UniqueId";
            if (BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()).Any(c => c.COLUMN_NAME.ToUpper() == "AuditRecordId".ToUpper()))
                if (!(ElementsAjoutes.Any(s => s.Trim() == AuditRecordIdFK)))
                {
                    list1.Add(AuditRecordIdFK);
                    ElementsAjoutes.Add(AuditRecordIdFK);
                }

            string UIDFK = "UID -> UserProfile.username";
            if (BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()).Any(c => c.COLUMN_NAME.ToUpper() == "UID"))
                if (!(ElementsAjoutes.Any(s => s.Trim() == UIDFK)))
                {
                    list1.Add(UIDFK);
                    ElementsAjoutes.Add(UIDFK);
                }

            // -------------------------------------------------------------------------------
            // remplir le ListBox central du milieu pour les FK
            // qui partent d'autre tables vers la table sélectionnée
            // -------------------------------------------------------------------------------
            // -------------------------------------------------------------------------------
            // fonction d'affichage et de tri
            // -------------------------------------------------------------------------------
            Func<FK, string> AffichageEtTriFK = fk => { return fk.FK_COLUMN_NAME + " <- " + fk.TABLE_NAME + "." + fk.COLUMN_NAME; };

            // -------------------------------------------------------------------------------
            // chercher les FK pertinentes
            // et trier par FK_NAME pour regrouper ensemble les FK multi-segement
            // -------------------------------------------------------------------------------
            fks = from fk
                  in BD_Schema.fks
                  join c in BD_Schema.columns on fk.TABLE_NAME + fk.COLUMN_NAME equals c.TABLE_NAME + c.COLUMN_NAME
                  where fk.FK_TABLE_NAME.ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()
                  orderby fk.FK_NAME, AffichageEtTriFK(fk)
                  select fk;

            // -------------------------------------------------------------------------------
            // obtenir la liste des FK_NAME unique
            // -------------------------------------------------------------------------------
            ufks = from fk
                   in fks.ToList().Distinct()
                   select fk.FK_NAME;

            // -------------------------------------------------------------------------------
            // batir petite liste temporaire des insertions pour comparaison
            // avec les FK 'molles'
            // -------------------------------------------------------------------------------
            ElementsAjoutes = new List<string>();

            // -------------------------------------------------------------------------------
            // pour chaque FK_NAME unique
            // -------------------------------------------------------------------------------
            foreach (string fkname in ufks)
            {
                // -------------------------------------------------------------------------------
                // cas d'une FK simple segment
                // -------------------------------------------------------------------------------
                if (fks.Count(fk => fk.FK_NAME == fkname) == 1)
                {
                    FK fk = fks.First(fk2 => fk2.FK_NAME == fkname);
                    list2.Add(AffichageEtTriFK(fk));
                    ElementsAjoutes.Add(AffichageEtTriFK(fk));
                }
                // -------------------------------------------------------------------------------
                // cas d'une FK multi-segement
                // -------------------------------------------------------------------------------
                else
                {
                    string gauche = "";
                    string droite = "";
                    string fktable = "";
                    foreach (FK fk in fks.Where(fk2 => fk2.FK_NAME == fkname))
                    {
                        gauche += ((gauche == string.Empty) ? "(" : ", ") + fk.FK_COLUMN_NAME;
                        droite += ((droite == string.Empty) ? "(" : ", ") + fk.COLUMN_NAME;
                        fktable = (fktable == string.Empty) ? fk.TABLE_NAME : fktable;
                    }
                    gauche += ")";
                    droite += ")";
                    list2.Add(gauche + " <- " + fktable + "." + droite);
                }
            }

            // -------------------------------------------------------------------------------
            // aller chercher la liste des FK 'molles' et, pour chaque...
            // -------------------------------------------------------------------------------
            foreach (COLUMN c in BD_Schema.columns.Where(c => c.TABLE_NAME.ToUpper() != SVCGlobal.wd.selectedTable.ToUpper()
                                                             && c.COLUMN_NAME.ToUpper().StartsWith("aPour".ToUpper())
                                                             && !c.COLUMN_NAME.ToUpper().StartsWith("aPourType".ToUpper())
                                                             && c.COLUMN_NAME.Substring("aPour".Length).ToUpper() == SVCGlobal.wd.selectedTable.ToUpper()
                                                        ).OrderBy(c => c.ORDINAL_POSITION))
                if (!(ElementsAjoutes.Any(s => s.Trim() == "UniqueId <- " + c.TABLE_NAME + "." + c.COLUMN_NAME))) // si la FK 'molle' est pas déjà là, on l'ajoute
                    list2.Add("UniqueId <= " + c.TABLE_NAME + "." + c.COLUMN_NAME);

            // -------------------------------------------------------------------------------
            // remplir le ListBox central du bas pour les FK
            // qui partent d'autre tables vers la table sélectionnée
            // -------------------------------------------------------------------------------
            if (PeuplerListbox3)
            {
                // -------------------------------------------------------------------------------
                // fonction d'affichage et de tri
                // -------------------------------------------------------------------------------
                Func<COLUMN, string> AffichageEtTriCO = fk => { return "UniqueId <- " + fk.TABLE_NAME + "." + fk.COLUMN_NAME; };


                list3.AddRange(BD_Schema.columns
                                .Where(c => c.COLUMN_NAME.ToUpper() == "aPourAuditable".ToUpper())
                                .Select(c => AffichageEtTriCO(c))
                                .OrderBy(_ => _).ToList());
            }

            int total = 100;
            int sum = list1.Count + list2.Count + list3.Count;
            nvh3 = (int)(total * list3.Count / (double)sum);
            nvh2 = (int)(total * list2.Count / (double)sum);
            nvh1 = total - nvh3 - nvh2;

            foreach (var s in list1) listBox1.Items.Add(s);
            foreach (var s in list2) listBox2.Items.Add(s);
            foreach (var s in list3) listBox3.Items.Add(s);

            listBox4.Items.Add(titreGauche);
            listBox4.Items.Add(new string('-', titreGauche.Length));
            foreach (var s in list4) listBox4.Items.Add(s);

            if (titreDroite.notisnws())
            {
                listBox5.Items.Add(titreDroite);
                listBox5.Items.Add(new string('-', titreDroite.Length));
                foreach (var s in list5) listBox5.Items.Add(s);
            }

            #endregion

            throughMyShowDialog = false;
        }

        public void MyShowDialog()
        {
            throughMyShowDialog = true;
            this.ShowDialog();
            throughMyShowDialog = false;
        }


        private void FJoin_Shown(object sender, EventArgs e)
        {
            if (!throughMyShowDialog)
            {
                MessageBox.Show("Use MyShowDialog to display this window", "Sir Sql Valet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
                return;
            }

            splMain.Tag = splMain.SplitterDistance.ToString();
            FJoin_Resize();

            try
            {
                ListBox lb = new ListBox[] { listBox2, listBox1, listBox3 }.FirstOrDefault(_ => _.Items.Count > 0);
                lb.SelectedIndex = 0;
                lb.Focus();
            }
            catch
            {
            }
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            DataInitialize();
            this.Close();
        }

        private void FJoin_Resize(object sender, EventArgs e)
        {
            FJoin_Resize();
        }
        private void FJoin_Resize()
        {
            if (splMain.Tag != null && int.TryParse(splMain.Tag.ToString(), out int newValue))
                splMain.SplitterDistance = newValue;

            AdjustMajorSplitters(25, 50, nvh1, nvh2);
        }

        private void AdjustMajorSplitters(int pctLeft, int pctMiddle, int pctMiddleTop, int pctMiddleMiddle)
        {
            int w = splMain.Width - 2;
            splLeft.SplitterDistance = w * pctLeft / 100;
            splMiddle.SplitterDistance = (w * pctMiddle / 100);

            int h = splMain.Height - 1 - splMain.SplitterDistance;
            splMiddleTop.SplitterDistance = (h * pctMiddleTop / 100);
            splMiddleMiddle.SplitterDistance = (h * pctMiddleMiddle / 100);
        }

        private void List123Click(List<string> list, int listIndex, int index)
        {
            StringSplitOptions ree = StringSplitOptions.RemoveEmptyEntries;

            if (index == -1) return;

            sel123 = new[] { -1, -1, -1 };
            sel123[listIndex] = index;

            List<string> Champs         = new List<string>();
            List<string> AutresChamps   = new List<string>();
            string AutreTable = string.Empty;

            bool MultiSegment = false;

            // -------------------------------------------------------------------------------
            // briser la ligne sélectionnée en parties
            // -------------------------------------------------------------------------------
            MultiSegment = list[index].Contains(',');
            foreach (string element in list[index].Split("-=<>".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (element.Trim().Contains("."))
                {
                    string[] deuxMots = element.Trim().Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    AutreTable = deuxMots[0];

                    if (MultiSegment)
                        AutresChamps.AddRange(deuxMots[1].Replace('(', ' ').Replace(')', ' ').Trim().Split(new char[] { ',' }, ree).Select(_ => _.Trim()));
                    else
                        AutresChamps.Add(deuxMots[1].Trim());
                }
                else
                {
                    if (MultiSegment)
                        Champs.AddRange(element.Replace('(', ' ').Replace(')', ' ').Trim().Split(new char[] { ',' }, ree).Select(_ => _.Trim()));
                    else
                        Champs.Add(element.Trim());
                }
            };

            // -------------------------------------------------------------------------------
            // sélectionner la(les) colonne(s) dans le listbox de gauche
            // -------------------------------------------------------------------------------
            listSelectedIndexList4 = new List<int>();
            foreach (var el in list4.Select((_, i) => (_: _, i: i)).OrderBy(_ => _.i))
                if (Champs.Any(c => (list4[el.i].ToUpper().Trim().StartsWith(c.ToUpper() + " ("))))
                    listSelectedIndexList4.Add(el.i);

            // -------------------------------------------------------------------------------
            // sélectionner la(les) colonne(s) dans le listbox de droite
            // -------------------------------------------------------------------------------
            listSelectedIndexList5  = new List<int>();
            list5                   = new List<string>();
            titreDroite             = AutreTable;
            BD_Schema.columns.Where(col => col.TABLE_NAME.ToUpper() == AutreTable.ToUpper()).OrderByDescending(col => col.ORDINAL_POSITION).ToList().ForEach(col =>
            {
                if (!list5.Any())
                    list5.Add(col.COLUMN_NAME + " (" + col.COLUMN_TYPE + ")");
                else
                    list5.Insert(0, col.COLUMN_NAME + " (" + col.COLUMN_TYPE + ")");

                if (list5.Any() && AutresChamps.Any(c => (list5[0].ToUpper().Trim().StartsWith(c.ToUpper() + " ("))))
                    listSelectedIndexList5.Add(0);
            });

            listBox5.Items.Clear();
            if (titreDroite.notisnws())
            {
                listBox5.Items.Add(titreDroite);
                listBox5.Items.Add(new string('-', titreDroite.Length));
                foreach (var s in list5) listBox5.Items.Add(s);
            }

            sel123.ToList().Select((_, i) => (_, i)).Where(_ => _.i != listIndex && listbox123[_.i].SelectedIndex != -1).ToList().ForEach(_ => listbox123[_.i].SelectedIndex = -1);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            List123Click(list1, 0, listBox1.SelectedIndex);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            List123Click(list2, 1, listBox2.SelectedIndex);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            List123Click(list3, 2, listBox3.SelectedIndex);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (new[] { Keys.Up, Keys.Left }.Contains(e.KeyCode) && listBox1.SelectedIndex == 0)
            {
                if (listBox3.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox3.SelectedIndex = listBox3.Items.Count - 1;
                    listBox3.Focus();
                }
                else if (listBox2.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                    listBox2.Focus();
                }
            }
            else if (new[] { Keys.Down, Keys.Right }.Contains(e.KeyCode) && listBox1.SelectedIndex == listBox1.Items.Count - 1)
            {
                if (listBox2.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox2.SelectedIndex = 0;
                    listBox2.Focus();
                }
                else if (listBox3.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox3.SelectedIndex = 0;
                    listBox3.Focus();
                }
            }
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (new[] { Keys.Up, Keys.Left }.Contains(e.KeyCode) && listBox2.SelectedIndex == 0)
            {
                if (listBox1.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    listBox1.Focus();
                }
                else if (listBox3.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox3.SelectedIndex = listBox3.Items.Count - 1;
                    listBox3.Focus();
                }
            }
            if (new[] { Keys.Down, Keys.Right }.Contains(e.KeyCode) && listBox2.SelectedIndex == listBox2.Items.Count - 1)
            {
                if (listBox3.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox3.SelectedIndex = 0;
                    listBox3.Focus();
                }
                else if (listBox1.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox1.SelectedIndex = 0;
                    listBox1.Focus();
                }
            }
        }

        private void listBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (new[] { Keys.Up, Keys.Left }.Contains(e.KeyCode) && listBox3.SelectedIndex == 0)
            {
                if (listBox2.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                    listBox2.Focus();
                }
                else if (listBox1.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    listBox1.Focus();
                }
            }
            if (new[] { Keys.Down, Keys.Right }.Contains(e.KeyCode) && listBox3.SelectedIndex == listBox3.Items.Count - 1)
            {
                if (listBox1.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox1.SelectedIndex = 0;
                    listBox1.Focus();
                }
                else if (listBox2.Items.Count > 0)
                {
                    e.Handled = true;
                    listBox2.SelectedIndex = 0;
                    listBox2.Focus();
                }
            }
        }

        private void bJoin_Click(object sender = null, EventArgs e = null)
        {
            (int, int) infoUserSelection;
            try
            {
                infoUserSelection = sel123.Select((_, i) => (_, i)).First(_ => _.Item1 != -1);
            }
            catch (Exception)
            {
                infoUserSelection = (0, -1);
            }

            if (infoUserSelection.Item2 == -1)
                return;

            string userSelection = list123[infoUserSelection.Item2][infoUserSelection.Item1];
            string joinType = new[] { "INNER", "LEFT", "LEFT" }.ToArray()[infoUserSelection.Item2];

            SirDBSidekickLogic.Join(userSelection, joinType);
            SirDBSidekickLogic.TraitementAvecAutreTable();
            PutOnStack();
            this.Close();
        }

        private void listBox123_DoubleClick(object sender, EventArgs e)
        {
            bJoin_Click();
        }
    }
}
