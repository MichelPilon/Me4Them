using SirSqlValetCommands.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSqlValetCommands
{
    public class CommandsPlugin
    {
        CommandsUI  documentUI;
        bool        isRegistred = false;

        public CommandsPlugin(CommandsUI documentUi)
        {
            documentUI = documentUi;
        }

        public void Register()
        {
            if (isRegistred)
                throw new Exception("CommandsPlugin is already registred");

            isRegistred = true;
            documentUI.Register();
        }
    }
}
