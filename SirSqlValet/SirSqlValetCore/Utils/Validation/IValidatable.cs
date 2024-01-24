using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirSqlValetCore.Utils.Validation
{
    public interface IValidatable<T>
    {
        T Validate();
    }
}
