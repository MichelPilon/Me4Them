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
    internal static class Command1002_SwitchComment
    {
        public static IEnumerable<string> Execute(IEnumerable<string> scriptLines, int ligneCurseur)
        {
            string[]    lines   = scriptLines.ToArray(); 
            int         nl      = ligneCurseur;

            const int   dnlMax  = 4;

            // détermine la Distance (>= 0) entre Ligne (l) à la Ligne de référence (lr)
            Func<int, int, int> dll = (l, lr) => Math.Abs(l - lr);

            // détermine le Nombre de Caractères Distincts non blanc dans s
            Func<string, int> ncd = (s) => s.ToCharArray().GroupBy(c => c).Count(_ => _.Key != ' ');

            // détermine le nombre d'éléments identique dans les listes
            Func<IEnumerable<string>, IEnumerable<string>, int> matchCount = (list1, list2) => {
                int nombre      = Math.Min(list1.Count(), list2.Count());
                var subList1    = list1.Take(nombre).ToArray();
                var subList2    = list2.Take(nombre).ToArray();
                return subList1.WithIndex().Count(__ => __._.Equals(subList2[__.i]));
            };

            Func<IEnumerable<string>, int, int, bool, IEnumerable<(string s, IEnumerable<string> chunks, int i)>> toLCI = (ls, lr, dmax, not) => { 
                return lines.WithIndex<string>()
                            .Where(_=>  dll(_.i, lr) <= dmax /* Distance à la ligne lr */
                                    &&  _._.notisnws() /* non vide */
                                    &&  (not ? !_._.Trim().StartsWith("--") : _._.Trim().StartsWith("--")) /* premiers caractères non vide == "--" */
                                    &&  ncd(_._) > 1 /* ncd => Nombre Caractères Distincts */ ).AsEnumerable<(string _, int i)>()
                            .Select(si =>  (si._,
                                            si._.Trim().Substring(2).Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).AsEnumerable<string>(),
                                            si.i));
            };

            // liste de < Lignes, Chuncks de ligne, Index ) >
            // si ligne non vide, commençant par -- ayant +1 caractères différents à l'intérieur de dnlMax lignes de distance
            var sources = toLCI(lines, nl, dnlMax, false);

            if (!sources.Any())
                return new List<string>().AsEnumerable();

            // sts = (Source & Targets)S
            var sts = sources
                .Select(_s =>  (source:_s, targets:toLCI(lines, _s.i, dnlMax + 1, true) .Select(_ => (s:_.s, chunks:_.chunks, i:_.i, n:matchCount(_s.chunks, _.chunks)))
                                                                                        .OrderByDescending(_ => _.n)
                                                                                        .AsEnumerable()))
                .Where              (_ => _.targets.ElementAt(0).n > 0)
                .OrderBy            (_ => dll(_.source.i, nl))
                .ThenByDescending   (_ => _.targets.ElementAt(0).n).AsEnumerable();

            if (!sts.Any())
                return new List<string>().AsEnumerable();

            var     stso            = sts.ElementAt(0).source;
            var     stt             = sts.ElementAt(0).targets.ElementAt(0);

            int     commentLine     = stso.i; 
            string  comment         = stso.s;

            int     codeLine        = stt.i;  
            string  code            = stt.s;

            int     commentIndex    = comment.IndexOf("--");
            int     codeIndex       =    code.IndexOf(code.Trim()[0]);

            if (codeIndex == commentIndex)
            {
                lines[commentLine]  = comment.Substring(0, commentIndex) +          comment.Substring(commentIndex + 2);
                lines[codeLine]     =    code.Substring(0, codeIndex)    + "--" +      code.Substring(codeIndex);
            }
            else if (codeIndex <= commentIndex + 2)
            {
                lines[commentLine]  = (commentIndex == 0 ? "" : comment.Substring(0, commentIndex))     + "  " + comment.Substring(commentIndex + 2);
                lines[codeLine]     = (codeIndex    <= 2 ? "" :    code.Substring(0, codeIndex - 2))    + "--" +    code.Substring(codeIndex);
            }
            else
            {
                lines[commentLine]  = (commentIndex == 0 ? "" : comment.Substring(0, commentIndex)) + "  " + comment.Substring(commentIndex + 2);
                lines[codeLine]     =                              code.Substring(0, commentIndex)  + "--" +    code.Substring(commentIndex + 2);
            }

            return lines;
        }
    }
}
