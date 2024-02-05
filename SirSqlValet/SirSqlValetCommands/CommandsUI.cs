using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using F = System.Windows.Forms;

using EnvDTE;

using SirSqlValetCommands.Commands;
using SirSqlValetCommands.Data;
using SirSqlValetCore.Integration;
using SirSqlValetCore.Integration.ObjectExplorer;
using SirSqlValetCore.App;

using System.Windows.Forms;

using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Threading.Tasks;
using SirSqlValetCommands.Forms;

namespace SirSqlValetCommands
{
    public class CommandsUI
    {
        public enum enumNBase
        {
            enbZero = 0,
            enbOne  = 1
        }

        private bool                            isRegistered;

        private PackageProvider                _packageProvider;
        private IObjectExplorerInteraction     _objectExplorerInteraction;
        private IWorkingDirProvider            _ssmsWorkingDirProvider;
        private IObjectExplorerService         _objectExplorer;

        private Document                        document;
        private TextDocument                    textDocumentObj;
                private string                 _textDocumentString;
                public  string                  textDocumentString => _textDocumentString;
                private IEnumerable<string>    _textDocumentLines;
                public  IEnumerable<string>     textDocumentLines => _textDocumentLines;

        private TextSelection                   textSelectionObj;
                private string                 _textSelectionString;
                public  string                  textSelectionString => _textSelectionString;

        private int TL1;
        private int TC1;
        private int BL1;
        private int BC1;
        public  int TL(enumNBase b) => TL1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int TC(enumNBase b) => TC1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int BL(enumNBase b) => BL1 - (b == enumNBase.enbZero ? 1 : 0);
        public  int BC(enumNBase b) => BC1 - (b == enumNBase.enbZero ? 1 : 0);

        private List<string> lastSelectedKeywords = new List<string>();

        public CommandsUI(PackageProvider packageProvider, IObjectExplorerInteraction objectExplorerInteraction, IWorkingDirProvider ssmsWorkingDirProvider)
        {
            _packageProvider            = packageProvider;
            _objectExplorerInteraction  = objectExplorerInteraction;
            _ssmsWorkingDirProvider     = ssmsWorkingDirProvider;

            SVCGlobal.statusText = "";
        }

        public void Register()
        {
            if (isRegistered)
                throw new Exception("DocumentUi is already registred");

            isRegistered = true;

            var handlers = new (EventHandler eh, int id)[]  {   (Sir_Sql_Valet,                 1001), 
                                                                (switch_comment,                1002), 
                                                                (exec_context,                  1003), 
                                                                (quote_unquote,                 1004),
                                                                (single_line,                   1005),
                                                                (connection_manager,            1006) 
                                                            };

            foreach (var handler in handlers)
                _packageProvider.CommandService.AddCommand(new MenuCommand(handler.eh, new CommandID(MenuHelper.CommandSet, handler.id)));
        }

        private void GetQueryText()
        {
            document = _packageProvider.Dte2.ActiveDocument;
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

        private void InitializeBeforeCommandExecution() => GetQueryText();

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
                InitializeBeforeCommandExecution();
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
                InitializeBeforeCommandExecution();
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
                InitializeBeforeCommandExecution();
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
                InitializeBeforeCommandExecution();
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
                InitializeBeforeCommandExecution();
                ReplaceCurrentSelection(Command1005_SingleLine.Execute(this));
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }
        private async void connection_manager(object sender, EventArgs eventArg)
        {
            if (_objectExplorer is null)
                _objectExplorer = (await _packageProvider.AsyncPackage.GetServiceAsync(typeof(IObjectExplorerService))) as IObjectExplorerService;

            try
            {
                // if no server definition, attempt to load from .json
                if (!CMInfos.cminfos().Any())
                { 
                    CMInfos.Load(_ssmsWorkingDirProvider.GetWorkingDir());
                    if (!CMInfos.cminfos().Any())
                        return; // if STILL no server definition, get the fuck out
                }

                // aller chercher la selection du user
                FKeywords fKeywords = new FKeywords(CMInfos.keyWords);
                List<string> selected = fKeywords.MyShowDialog(lastSelectedKeywords).ToList();
                fKeywords.Dispose();
                fKeywords = null;

                // pas de selection
                if (!selected.Any())
                    return; // ou on termine

                // disconnect all connections
                INodeInformation[] ini;
                _objectExplorer.GetSelectedNodes(out int arraySize, out ini);
                while (arraySize > 0)
                {
                    _objectExplorer.DisconnectSelectedServer();
                    _objectExplorer.GetSelectedNodes(out arraySize, out ini);
                }

                lastSelectedKeywords.Clear();
                lastSelectedKeywords.AddRange(selected);

                var servers = CMInfos.cminfos() .Where  (_ => selected.All(s => _.KEYWORDS.Contains(s)))
                                                .OrderBy(_ => _.NSTAR()).ThenBy(_ => _.FriendlyName());

                foreach (var _ in servers)
                    ConnectAndDoEvents(_);

                foreach (var i in Enumerable.Range(1, 10))
                {
                    Application.DoEvents();
                    Task.Delay(20).Wait();
                }

                foreach (var _ in servers)
                {
                    ObjectExplorerServer snode = ObjectExplorerHelper.GetServerHierarchyNode(_objectExplorer, _.SERVER);
                    ObjectExplorerHelper.SelectNode(_objectExplorer, snode.Root);
                    foreach (var i in Enumerable.Range(1, 10))
                    {
                        Application.DoEvents();
                        Task.Delay(10).Wait();
                    }

                    if (_.STAR() || servers.Count() == 1)
                    {
                        SendKeys.SendWait("{RIGHT}");
                        Application.DoEvents();
                        Task.Delay(100).Wait();
                        SendKeys.SendWait("{RIGHT}");
                        Application.DoEvents();
                        Task.Delay(100).Wait();
                    }
                    else
                    {
                        SendKeys.SendWait("{LEFT}");
                        Application.DoEvents();
                        Task.Delay(100).Wait();
                    }
                }

                //IReadOnlyCollection<ObjectExplorerServer> x = ObjectExplorerHelper.GetServersConnection(_objectExplorer);
                //foreach (ObjectExplorerServer xx in x)
                //{ 
                //    if (xx.Root.IsExpanded)
                //    { 
                //        xx.Root.Collapse();
                //        Application.DoEvents();
                //    }
                //
                //    Application.DoEvents();
                //}
            }
            catch (Exception exception)
            {
                ShowMessage_Error(GetCurrentMethodName(), exception);
            }
        }

        private void ConnectAndDoEvents(CMInfo cmi)
        {
            _objectExplorerInteraction.ConnectServer(cmi.SERVER, cmi.FriendlyName());
            foreach (var i in Enumerable.Range(1, 10))
            {
                Application.DoEvents();
                Task.Delay(20).Wait();
            }
        }

        //private void SendKeysWaitDoEvents(string text) => SendKeysWaitDoEvents(new[] { text });
        //private void SendKeysWaitDoEvents(IEnumerable<string> keys)
        //{
        //    foreach (var key in keys) 
        //    {
        //        SendKeys.SendWait(key);
        //        Application.DoEvents();
        //        System.Threading.Thread.Sleep(200);
        //    }
        //}
    }
}
