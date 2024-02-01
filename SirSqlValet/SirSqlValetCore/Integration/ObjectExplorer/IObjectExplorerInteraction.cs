using System.Collections.Generic;
using System.Threading.Tasks;

namespace SirSqlValetCore.Integration.ObjectExplorer
{
    public interface IObjectExplorerInteraction
    {
        Task SelectNodeAsync(string server, string dbName, IReadOnlyCollection<string> itemPath);
        void ConnectServer(string server);
    }
}