using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSqlValetCore.Integration.ObjectExplorer
{
    public class ObjectExplorerInteraction : IObjectExplorerInteraction
    {
        PackageProvider         _packageProvider;
        IObjectExplorerService  _objectExplorer;

        public ObjectExplorerInteraction(PackageProvider packageProvider)
        {
            _packageProvider = packageProvider;
        }

        public async System.Threading.Tasks.Task SelectNodeAsync(string server, string dbName, IReadOnlyCollection<string> itemPath)
        {
            var objectExplorer = (await _packageProvider.AsyncPackage.GetServiceAsync(typeof(IObjectExplorerService))) as IObjectExplorerService;
            var objNode = ObjectExplorerHelper.GetObjectHierarchyNode(objectExplorer, server, dbName, itemPath);
            ObjectExplorerHelper.SelectNode(objectExplorer, objNode);
        }

        public async void ConnectServer(string server)
        {
            if (_objectExplorer is null)
                _objectExplorer = (await _packageProvider.AsyncPackage.GetServiceAsync(typeof(IObjectExplorerService))) as IObjectExplorerService;

            UIConnectionInfo ci     = new UIConnectionInfo();
            ci.ServerName           = server;
            ci.ServerType           = new Guid("8c91a03d-f9b4-46c0-a305-b5dcc79ff907");
            ci.AuthenticationType   = 0;
            ci.DisplayName          = server;

           _objectExplorer.ConnectToServer(ci);
        }
    }
}
