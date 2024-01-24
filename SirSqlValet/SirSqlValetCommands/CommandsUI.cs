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
        }

        public void Register()
        {
            if (isRegistered)
                throw new Exception("DocumentUi is already registred");

            isRegistered = true;

            foreach (var info in new int[] { 1001, 1002, 1003, 1004, 1005 }) 
            {
                EventHandler           eh = null;

                if      (info == 1001) eh = this.ExecuteFromMenu1001; 
                else if (info == 1002) eh = this.ExecuteFromMenu1002; 
                else if (info == 1003) eh = this.ExecuteFromMenu1003; 
                else if (info == 1004) eh = this.ExecuteFromMenu1004; 
                else if (info == 1005) eh = this.ExecuteFromMenu1005; 

                if (eh != null)
                    packageProvider.CommandService.AddCommand(new MenuCommand(eh, new CommandID(MenuHelper.CommandSet, info)));
            }
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
        private void SetQueryText(string newText) => SetQueryText(newText.SplitTextOnLines());
        private void SetQueryText(IEnumerable<string> newLines)
        {
            int index_top_both = -1;
            for (int i = 0; index_top_both == -1 && i < Math.Min(textDocumentLines.Count(), newLines.Count()); i++)
                if (textDocumentLines.ElementAt(i) != newLines.ElementAt(i))
                    index_top_both = i;

            if (index_top_both == -1)
                return;

            int index_end;
            for (index_end = 0; index_end <= 0 && index_end < Math.Min(textDocumentLines.Count(), newLines.Count()); index_end++)
                if (textDocumentLines.ElementAt(textDocumentLines.Count() - 1 + index_end) != newLines.ElementAt(newLines.Count() - 1 + index_end))
                    index_end*=-1;

            if (index_end <= 0)
                return;

            int index_end_original  =    textDocumentLines.Count() - 1 - index_end;
            int index_end_new       = newLines.Count() - 1 - index_end;

                   newLines         = newLines.Skip(index_top_both).Take(index_end_new - index_top_both + 1);
            string newText          = newLines.Join(Environment.NewLine, after: index_end_original < textDocumentLines.Count() - 1);

            textSelectionObj.MoveToLineAndOffset(index_top_both + 1, 1);
            textSelectionObj.LineDown(Count: index_end_original - index_top_both + 1, Extend: true);
            textSelectionObj.Insert(newText);

            textSelectionObj.MoveToLineAndOffset(TL(enumNBase.enbOne), TC(enumNBase.enbOne));
            textSelectionObj.LineDown(Count: BL(enumNBase.enbOne) - TL(enumNBase.enbOne), Extend: true);
            textSelectionObj.CharRight(Count: BC(enumNBase.enbOne) - TC(enumNBase.enbOne), Extend: true);
        }

        private void ExecuteFromMenu1001(object sender, EventArgs _e)
        {
            try
            {
                GetQueryText();
                NewClipboard(textDocumentString, TL(enumNBase.enbZero));
                FScript f = new FScript();
                SetQueryText(f.MyShowDialog());
                f.Dispose();
                f = null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Switch Comment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExecuteFromMenu1002(object sender, EventArgs _e)
        {
            try
            {
                SetQueryText(Command1002_SwitchComment.Execute(GetQueryText(), TL(enumNBase.enbZero)));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Switch Comment", MessageBoxButton.OK, MessageBoxImage.Error);
            }   
        }
        private void ExecuteFromMenu1003(object sender, EventArgs e)
        {
            F.MessageBox.Show("rotate transaction/dev context");
        }
        private void ExecuteFromMenu1004(object sender, EventArgs e)
        {
            F.MessageBox.Show("[un] stringyfy");
        }
        private void ExecuteFromMenu1005(object sender, EventArgs e)
        {
            F.MessageBox.Show("sqwish to single line");
        }
    }
}
