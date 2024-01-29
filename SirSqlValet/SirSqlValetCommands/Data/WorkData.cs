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
    public class WorkData
    {
        public  string          otherTable                                  = string.Empty;
        
        public  string          selectedPrefixe             { get; set; }   = string.Empty;
        public  string          selectedTable               { get; set; }   = string.Empty;
        public  string          selectedAcronyme            { get; set; }   = string.Empty;

        private	List<string>   _scriptLines                 { get; set; }   = new List<string>();
        public	List<string>    scriptLines
        {
            get => _scriptLines;
            set
            {
                _scriptLines = new List<string>();
                _scriptLines.AddRange(value ?? new List<string>());
            }
        }
        
        public  string  SafeGetLine (int numligne)
        {
            string returnValue = string.Empty;
            if (numligne > -1 && numligne < scriptLines.Count)
                returnValue = scriptLines[numligne];
            else
                Console.WriteLine($@"WARNING : call to SafeGetLine({numligne}) while having {scriptLines.Count} lines");
            
            return returnValue;
        }
        public  void    SafeSetLine (int numligne, string s)
        {
            if (numligne > -1 && numligne < scriptLines.Count)
                scriptLines[numligne] = s;
            else
                Console.WriteLine($@"WARNING : while having {scriptLines.Count} lines, call to SafeSetLine({numligne}, ""...{s.Trim()}..."")");
        }
        
        
        public int              numeroLigneCurseur          { get; set; }   = -1;
        public int              premiereLigneRequete
        {
            get
            {
                Regex selectRX = new Regex(@"^(?:\s|\t)*SELECT(?:\s|\t)", RegexOptions.IgnoreCase);

                var BeforeCurseurVides = scriptLines.FromToIdx(BOF, numeroLigneCurseur).Where(_ => _._.isnws()).OrderBy(_ => _.i);
                int line = !BeforeCurseurVides.Any() ? 0 : BeforeCurseurVides.Max(_ => _.i) + 1;
                while (line < scriptLines.Count && !selectRX.IsMatch(SafeGetLine(line)))
                    line++;

                if (!selectRX.IsMatch(SafeGetLine(line)))
                    throw new Exception();

                return line;
            }
        }
        public  int             derniereLigneRequete
        {
            get
            {
                var AfterCurseurVides = scriptLines.FromToIdx(numeroLigneCurseur, EOF).Where(_ => _._.isnws());
                return !AfterCurseurVides.Any() ? scriptLines.Count - 1 : AfterCurseurVides.Min(_ => _.i) - 1;
            }
        }

        private List<string>   _groupeSuperieur             { get; set; }   = new List<string>();
        public  List<string>    groupeSuperieur
        {
            get => _groupeSuperieur;
            set
            {
                _groupeSuperieur = new List<string>();
                _groupeSuperieur.AddRange(value ?? new List<string>());
            }
        }

        private List<string>   _groupeInferieur             { get; set; }   = new List<string>();
        public  List<string>    groupeInferieur
        {
            get =>  _groupeInferieur;
            set
            {
                _groupeInferieur = new List<string>();
                _groupeInferieur.AddRange(value ?? new List<string>());
            }
        }

        public  int             LineInsertInferieur
        {
            get
            {
                int line = numeroLigneCurseur;

                Group gAcronyme;
                Match match = (new Regex(@"(?:FROM|JOIN)(?:\s|\t)+(?:\w+\.\w+|\w+)(?:\s|\t)+(?'acronyme'\w+)(?:(?:\s|\t)|$)", RegexOptions.IgnoreCase)).Match(SafeGetLine(line));
                
                if (match.Success && ((gAcronyme = match.Groups["acronyme"]) != null))
                {
                    line++;
                    Regex rxFromJoin    = new Regex($@"JOIN(?:\s|\t)+(?:\w+\.\w+)(?:\s|\t).*(?:\s|\t){gAcronyme}\.", RegexOptions.IgnoreCase);
                    try
                    {
                        line = scriptLines.FromToIdx(line, EOF).First(_ => !rxFromJoin.Match(_._).Success).i;
                    }
                    catch
                    {
                        line = wd.derniereLigneRequete + 1;
                    }                
                }
                return line;
            }
        }
        
        public  int             LineInsertSuperieur
        {
            get
            {
                Regex rx = new Regex(@"^(?:\s|\t)*FROM(?:\s|\t).*$", RegexOptions.IgnoreCase);
                int i = -1;
                try
                {
                    i = scriptLines.FromToIdx(premiereLigneRequete, numeroLigneCurseur).First(_ => rx.IsMatch(_._)).i;
                }
                catch 
                {
                    i = -1;
                }
                return i;
            }
        }
    }
}

