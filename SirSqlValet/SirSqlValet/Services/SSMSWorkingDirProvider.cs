using SirSqlValetCore.App;
using System;
using System.IO;

namespace SirSqlValet.Services
{
    public class SSMSWorkingDirProvider : IWorkingDirProvider
    {
        private string _cachedWorkingDir = string.Empty;

        public string GetWorkingDir()
        {
            if (string.IsNullOrEmpty(_cachedWorkingDir))
            {
                _cachedWorkingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sir Sql Valet");
                Directory.CreateDirectory(_cachedWorkingDir);
            }

            return _cachedWorkingDir;
        }
    }
}