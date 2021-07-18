using Customization;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcuChecker
{
    public class CustomPlugin: CustomizationPlugin
    {
        public override void UpdateDatabase()
        {
            try
            {
                PXDatabase.Insert<Acucheckpref>(new PXDataFieldAssign<Acucheckpref.enableTests>(true));
            }catch
            {

            }

        }
    }
}
