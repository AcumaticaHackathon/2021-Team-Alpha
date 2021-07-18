using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcuChecker
{
    public class CompanyMaintExt : PXGraphExtension<CompanyMaint>
    {
        public PXSelect<Acucheckpref> AcucheckSetup;
    }
}
