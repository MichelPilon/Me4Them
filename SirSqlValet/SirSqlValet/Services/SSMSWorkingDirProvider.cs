using SirSqlValetCommands.Data;

using SirSqlValetCore.App;
using System;
using System.IO;

namespace SirSqlValet.Services
{
    public class SSMSWorkingDirProvider : IWorkingDirProvider
    {
        private string _cachedWorkingDir = null;

        public string GetWorkingDir()
        {
            bool firstTime = string.IsNullOrWhiteSpace(_cachedWorkingDir);

            if (firstTime)
               _cachedWorkingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SirSqlValet");

            if (firstTime && !Directory.Exists(_cachedWorkingDir))
                Directory.CreateDirectory(_cachedWorkingDir);

            return _cachedWorkingDir;
        }
    }
}