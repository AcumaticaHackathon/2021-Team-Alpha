using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcuChecker.DACExt
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public sealed class CustProjectExt : PXCacheExtension<CustProject>
    {
        public class PK : PrimaryKeyOf<CustProject>.By<CustProject.projID>
        {
            public static CustProject Find(PXGraph graph, Guid? ProjectID)
             => FindBy(graph, ProjectID);
        }
    }
}
