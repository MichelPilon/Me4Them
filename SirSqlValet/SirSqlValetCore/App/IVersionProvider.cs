using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSqlValetCore.App
{
    public interface IVersionProvider
    {
        int GetBuild();
        int[] GetBuildAndRevision();
    }
}
