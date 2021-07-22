using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcuChecker
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class CompanyMaintExt : PXGraphExtension<CompanyMaint>
    {
        public PXSelect<Acucheckpref> AcucheckSetup;
    }
}
