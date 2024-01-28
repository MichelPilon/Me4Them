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
    public static class SVCGlobal
    {
        public  const   StringComparison        nocase              = StringComparison.OrdinalIgnoreCase;
        
        public  static  string                  statusText          = "";
        public  static  int                     MAX_CHAR_PER_LINE   = 100;

        public  static  readonly int?           EOF                 = null;
        public  static  readonly int            BOF                 = 0;

      //public  const   string                  MainWindowURL       = @"/";
      //public  const   string                  NextWindowURL       = @"/sidekick";
      //public  const   string                  Tailwind            = @"/tailwind";

        public  static  Stack<StackElement>     scriptStack         = new Stack<StackElement>(); 
        public  static  WorkData                wd                  = new WorkData();        
                
        static SVCGlobal()
        {
            if (BD_Schema.HasToBePopulated)
                BD_Schema.Populate("CCQSQL044170", "CCQ", @"C:\Temp");
        }

        public static void NewScriptStack(string rawText, int line)
        {
            if (scriptStack.Any())
                scriptStack.Clear();
            
            scriptStack.Push(new StackElement() { SelectedLine = line, RawText = rawText });

            UndoAll();
        }

        public static void AcceptAndLeave()
        {
            ClipboardService.SetText(string.Join(Environment.NewLine, wd.scriptLines));
        }

        public static void Quit()
        {
        }

        public static void UndoOne()
        {
            if (scriptStack.Count > 1)
            {
                scriptStack.Pop();
                DataInitialize();
            }
        }

        public static void UndoAll()
        {
            while (scriptStack.Count > 1) scriptStack.Pop();
            DataInitialize();
        }

        public static void PutOnStack()
        {
            scriptStack.Push(new StackElement() { SelectedLine = wd.numeroLigneCurseur, RawText = string.Join(Environment.NewLine, wd.scriptLines) });
            DataInitialize();
        }
        
        public static void UpdateStackTopWithLineNumber()
        {
            scriptStack.Peek().SelectedLine = wd.numeroLigneCurseur;
        }
        
        public static void DataInitialize()
        {
            wd = new WorkData() { numeroLigneCurseur = scriptStack.Peek().SelectedLine };

            if (scriptStack.Peek().RawText.Contains(Environment.NewLine))
                wd.scriptLines.AddRange(scriptStack.Peek().RawText.SplitTextOnLines());
            else
                wd.scriptLines.Add(scriptStack.Peek().RawText);
        }
    }
}
