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

        public  static  Stack<StackElement>     theStack            = new Stack<StackElement>(); 
        public  static  WorkData                wd                  = new WorkData();        
                
        static SVCGlobal()
        {
            if (BD_Schema.HasToBePopulated)
                BD_Schema.Populate("CCQSQL044170", "CCQ", @"C:\Temp");
        }

        public static void NewClipboard(string rawText, int line)
        {
            if (theStack.Any())
                theStack.Clear();
            
            theStack.Push(new StackElement() { SelectedLine = line, RawText = rawText });

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
            if (theStack.Count > 1)
            {
                theStack.Pop();
                DataInitialize();
            }
        }

        public static void UndoAll()
        {
            while (theStack.Count > 1) theStack.Pop();
            DataInitialize();
        }

        public static void PutOnStack()
        {
            theStack.Push(new StackElement() { SelectedLine = wd.numeroLigneCurseur, RawText = string.Join(Environment.NewLine, wd.scriptLines) });
            DataInitialize();
        }
        
        public static void UpdateStackTopWithLineNumber()
        {
            theStack.Peek().SelectedLine = wd.numeroLigneCurseur;
        }
        
        public static void DataInitialize()
        {
            wd = new WorkData() { numeroLigneCurseur = theStack.Peek().SelectedLine };

            if (theStack.Peek().RawText.Contains(Environment.NewLine))
                wd.scriptLines.AddRange(theStack.Peek().RawText.SplitTextOnLines());
            else
                wd.scriptLines.Add(theStack.Peek().RawText);
        }
    }
}
