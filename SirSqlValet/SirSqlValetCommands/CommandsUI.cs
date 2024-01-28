using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using F = System.Windows.Forms;

using EnvDTE;

using SirSqlValetCommands.Commands;
using SirSqlValetCommands.Data;

using SirSqlValetCore.Integration;

using static SirSqlValetCommands.Data.SVCGlobal;
using System.Windows;
using System.Windows.Forms;


namespace SirSqlValetCommands
{
    public class CommandsUI
    {
        private enum enumNBase
        {
            enbZero = 0,
            enbOne  = 1
        }

        private bool                    isRegistered;

        private PackageProvider         packageProvider;
        private Document                document;

        private TextDocument            textDocumentObj;
        private string                  textDocumentString;
        private IEnumerable<string>     textDocumentLines;

        private EnvDTE.TextSelection    textSelectionObj;
        private string                  textSelectionString;

        private int                     TL1;
        private int                     TC1;
        private int                     BL1;
        private int                     BC1;
        private int                     TL(enumNBase b) => TL1 - (b == enumNBase.enbZero ? 1 : 0);
        private int                     TC(enumNBase b) => TC1 - (b == enumNBase.enbZero ? 1 : 0);
        private int                     BL(enumNBase b) => BL1 - (b == enumNBase.enbZero ? 1 : 0);
        private int                     BC(enumNBase b) => BC1 - (b == enumNBase.enbZero ? 1 : 0);

        public CommandsUI(PackageProvider packageProvider)
        {
            this.packageProvider = packageProvider;
            SVCGlobal.statusText = "";
          //F.MessageBox.Show($"Coucou ! ;-)", $"", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void Register()
        {
            if (isRegistered)
                throw new Exception("DocumentUi is already registred");

            isRegistered = true;

            var handlers = new (EventHandler eh, int id)[] { (Sir_Sql_Valet, 1001), (switch_comment, 1002), (exec_context, 1003), (quote_unquote, 1004), (single_line, 1005) };

            foreach (var handler in handlers)
                packageProvider.CommandService.AddCommand(new MenuCommand(handler.eh, new CommandID(MenuHelper.CommandSet, handler.id)));
        }

        private IEnumerable<string> GetQueryText()
        {
            document = packageProvider.Dte2.ActiveDocument;
            if (document == null)
                return new List<string>().AsEnumerable();

            textDocumentObj     = (TextDocument)document.Object("TextDocument");
            textDocumentString  = textDocumentObj.StartPoint.CreateEditPoint().GetText(textDocumentObj.EndPoint);
            textDocumentLines   = textDocumentString.SplitTextOnLines();
            
            textSelectionObj    = textDocumentObj.Selection;
            textSelectionString = textSelectionObj.TopPoint.CreateEditPoint().GetText(textSelectionObj.BottomPoint);
            TL1                 = textSelectionObj.TopPoint.Line;
            TC1                 = textSelectionObj.TopPoint.LineCharOffset;
            BL1                 = textSelectionObj.BottomPoint.Line;
            BC1                 = textSelectionObj.BottomPoint.LineCharOffset;

            return textDocumentLines;
        }
        private void SetQueryText_MergeNewText(string newText) => SetQueryText_MergeNewLines(newText.SplitTextOnLines());
        private void SetQueryText_MergeNewLines(IEnumerable<string> newLines)
        {
            int index_top_both = -1;
            for (int i = 0; index_top_both == -1 && i < Math.Min(textDocumentLines.Count(), newLines.Count()); i++)
                if (textDocumentLines.ElementAt(i) != newLines.ElementAt(i))
                    index_top_both = i;

            if (index_top_both.Equals(-1))
                return;

            int index_end = -1;
            for (int i = 0; index_end == -1 && i < Math.Min(textDocumentLines.Count(), newLines.Count()); i++)
                if (textDocumentLines.ElementAt(textDocumentLines.Count() - 1 - i) != newLines.ElementAt(newLines.Count() - 1 - i))
                    index_end = i;

            if (index_end.Equals(-1))
                return;

            int index_end_original  = textDocumentLines.Count() - 1 - index_end;
            int index_end_new       =          newLines.Count() - 1 - index_end;

                   newLines         = newLines.Skip(index_top_both).Take(index_end_new - index_top_both + 1);
            string newText          = newLines.Join(Environment.NewLine, after: index_end_original < textDocumentLines.Count() - 1);

            textSelectionObj.MoveToLineAndOffset(index_top_both + 1, 1);
            textSelectionObj.LineDown(Count: index_end_original - index_top_both + 1, Extend: true);
            textSelectionObj.Insert(newText);

            textSelectionObj.MoveToLineAndOffset(TL(enumNBase.enbOne), TC(enumNBase.enbOne));
            textSelectionObj.LineDown(Count: BL(enumNBase.enbOne) - TL(enumNBase.enbOne), Extend: true);
            textSelectionObj.CharRight(Count: BC(enumNBase.enbOne) - TC(enumNBase.enbOne), Extend: true);
        }

        private void InitializeBefore_SpecificCommandExecution() => GetQueryText();

        private void MergeNewScriptInCurrentScript(IEnumerable<string> newLines)
        {
            SetQueryText_MergeNewLines(newLines);

            textDocumentLines   = newLines;
            textDocumentString  = textDocumentLines.Join();
        }

        private void ShowMessage_ComingSoon(string texte)
        {
            F.MessageBox.Show($"{texte}\r\n\r\n...coming soon to an extension near you !", $"{texte}", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void ShowMessage_Error(string texte, Exception exception)
        {
            texte = $"{texte} : ERREUR";

            while ((exception = exception.InnerException) != null)
                texte += Environment.NewLine + Environment.NewLine + exception.Message;

            F.MessageBox.Show(texte, texte.Substring(0, texte.IndexOf(' ')), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static string GetCurrentMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName; // This will return "Main"
        }

        private void Sir_Sql_Valet(object sender, EventArgs _e)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                MergeNewScriptInCurrentScript(new FScript(textDocumentString, TL(enumNBase.enbZero)).MyShowDialog());
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
        private void switch_comment(object sender, EventArgs eventArg)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                MergeNewScriptInCurrentScript(Command1002_SwitchComment.Execute(textDocumentLines, TL(enumNBase.enbZero)));
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
        private void exec_context(object sender, EventArgs eventArg)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                ShowMessage_ComingSoon(GetCurrentMethodName());
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
        private void quote_unquote(object sender, EventArgs eventArg)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                ShowMessage_ComingSoon(GetCurrentMethodName());
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
        private void single_line(object sender, EventArgs eventArg)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                ShowMessage_ComingSoon(GetCurrentMethodName());
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }

    }
}
