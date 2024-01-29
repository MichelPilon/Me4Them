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
using System.Windows.Forms;


namespace SirSqlValetCommands
{
    public class CommandsUI
    {
        public enum enumNBase
        {
            enbZero = 0,
            enbOne  = 1
        }

        private bool                    isRegistered;

        private PackageProvider         packageProvider;
        private Document                document;

        private TextDocument            textDocumentObj;

        private string                 _textDocumentString;
        public  string                  textDocumentString => _textDocumentString;

        private IEnumerable<string>    _textDocumentLines;
        public  IEnumerable<string>     textDocumentLines => _textDocumentLines;

        private EnvDTE.TextSelection    textSelectionObj;
        private string                 _textSelectionString;
        public  string                  textSelectionString => _textSelectionString;

        private int                     TL1;
        private int                     TC1;
        private int                     BL1;
        private int                     BC1;
        public  int                     TL(enumNBase b) => TL1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int                     TC(enumNBase b) => TC1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int                     BL(enumNBase b) => BL1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int                     BC(enumNBase b) => BC1 - (b == enumNBase.enbZero ? 1 : 0);

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

            var handlers = new (EventHandler eh, int id)[]{ (Sir_Sql_Valet,     1001), 
                                                            (switch_comment,    1002), 
                                                            (exec_context,      1003), 
                                                            (quote_unquote,     1004), 
                                                            (single_line,       1005) };

            foreach (var handler in handlers)
                packageProvider.CommandService.AddCommand(new MenuCommand(handler.eh, new CommandID(MenuHelper.CommandSet, handler.id)));
        }

        private void GetQueryText()
        {
            document = packageProvider.Dte2.ActiveDocument;
            if (document == null)
                return;

            textDocumentObj     = (TextDocument)document.Object("TextDocument");
           _textDocumentString  = textDocumentObj.StartPoint.CreateEditPoint().GetText(textDocumentObj.EndPoint);
           _textDocumentLines   =_textDocumentString.SplitTextOnLines();
            
            textSelectionObj    = textDocumentObj.Selection;
           _textSelectionString = textSelectionObj.TopPoint.CreateEditPoint().GetText(textSelectionObj.BottomPoint);
            TL1                 = textSelectionObj.TopPoint.Line;
            TC1                 = textSelectionObj.TopPoint.LineCharOffset;
            BL1                 = textSelectionObj.BottomPoint.Line;
            BC1                 = textSelectionObj.BottomPoint.LineCharOffset;
        }
        private void SetQueryText_MergeNewLines(IEnumerable<string> newLines)
        {
            int index_top_both = -1;
            for (int i = 0; index_top_both == -1 && i < Math.Min(_textDocumentLines.Count(), newLines.Count()); i++)
                if (_textDocumentLines.ElementAt(i) != newLines.ElementAt(i))
                    index_top_both = i;

            if (index_top_both.Equals(-1))
                return;

            int index_end = -1;
            for (int i = 0; index_end == -1 && i < Math.Min(_textDocumentLines.Count(), newLines.Count()); i++)
                if (_textDocumentLines.ElementAt(_textDocumentLines.Count() - 1 - i) != newLines.ElementAt(newLines.Count() - 1 - i))
                    index_end = i;

            if (index_end.Equals(-1))
                return;

            int index_end_original  = _textDocumentLines.Count() - 1 - index_end;
            int index_end_new       =          newLines.Count() - 1 - index_end;

                   newLines         = newLines.Skip(index_top_both).Take(index_end_new - index_top_both + 1);
            string newText          = newLines.Join(Environment.NewLine, after: index_end_original < _textDocumentLines.Count() - 1);

            textSelectionObj.MoveToLineAndOffset(index_top_both + 1, 1);
            textSelectionObj.LineDown(Count: index_end_original - index_top_both + 1, Extend: true);
            textSelectionObj.Insert(newText);

            textSelectionObj.MoveToLineAndOffset(TL(enumNBase.enbOne), TC(enumNBase.enbOne));
            textSelectionObj.LineDown(Count: BL(enumNBase.enbOne) - TL(enumNBase.enbOne), Extend: true);
            textSelectionObj.CharRight(Count: BC(enumNBase.enbOne) - TC(enumNBase.enbOne), Extend: true);
        }

        private void InitializeBefore_SpecificCommandExecution() => GetQueryText();

        private void ReplaceCurrentSelection(string newSelection)
        {
            textDocumentObj.Selection.Insert(newSelection, vsInsertFlagsCollapseToEndValue: 0);
        }

        private void ShowMessage_ComingSoon(string texte)
        {
            F.MessageBox.Show($"{texte}\r\n\r\n...coming soon to an extension near you !", $"{texte}", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void ShowMessage_Error(string texte, Exception exception)
        {
            Func<string, Exception, string> AddToText = (s, e) => s + Environment.NewLine + Environment.NewLine + e.Message;

            string message = AddToText($"{texte} : ERREUR", exception);

            while ((exception = exception.InnerException) != null)
                message = AddToText(message, exception);

            F.MessageBox.Show(message, texte, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static string GetCurrentMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName;
        }

        private void Sir_Sql_Valet(object sender, EventArgs _e)
        {
            try
            {
                InitializeBefore_SpecificCommandExecution();
                SetQueryText_MergeNewLines(Command1001_SirSqlValet.Execute(this));
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
                SetQueryText_MergeNewLines(Command1002_SwitchComment.Execute(textDocumentLines, TL(enumNBase.enbZero)));
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
                SetQueryText_MergeNewLines(Command1003_RotateExecContext.Execute(this));
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
                ReplaceCurrentSelection(Command1004_QuoteUnquote.Execute(this));
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
                ReplaceCurrentSelection(Command1005_SingleLine.Execute(this));
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
    }
}
