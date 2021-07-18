using System;
using PX.Objects;
using PX.Data;
using AcuChecker;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Web;

namespace PX.SM
{
    public class ProjectList_Extension : PXGraphExtension<ProjectList>
    {
        #region Event Handlers
        
        public PXAction<CustProject> ValidateSqlExists;
        [PXButton]
        [PXUIField(DisplayName = "Validate database (Runtime Validation)")]
        protected IEnumerable validateSqlExists(PXAdapter adapter)
        {
            PXResultset<CustObject> custDLLs = PXSelect<CustObject, Where<CustObject.content, Like<Required<CustObject.content>>>>.Select(Base, "%.DLL%");
            PXResultset<CustObject> custGraphs = PXSelect<CustObject, Where<CustObject.type, Like<Required<CustObject.type>>>>.Select(Base, "Graph");

            PXResultset<CustObject> custDACs = PXSelect<CustObject, Where<CustObject.type, Like<Required<CustObject.type>>>>.Select(Base, "DAC");
            PXResultset<CustProject> custProjects = PXSelect<CustProject>.Select(Base);

            PXLongOperation.StartOperation(this, delegate
            {
                var messages = new List<string> { "Checking the customizations for missing SQL create statments\n" };
                foreach (CustObject d in custDLLs)
                {
                    string pName = "";
                    {
                        foreach (CustProject cproj in custProjects)
                        {
                            if (cproj?.ProjID == d.ProjectID)
                            {
                                pName = cproj.Name;
                                break;
                            }
                        }
                        //= custProjects.FirstOrDefault(p => p.ProjID == d.ProjectID)?.Name;
                    }
                    messages.Add($"{pName}");
                    var testDllResults = CustomizationChecker.TestDLL(d.Name);
                    var errors = testDllResults.Where(t => t.Item1 == false).Select(t => t.Item2).ToList();
                    if (errors.Count() > 0)
                    {
                        errors.ForEach(e =>
                        {
                            messages.Add($"{e}");
                        });
                    }
                    else
                    {
                        messages.Add($"Everything seems Ok in {d.Name}\n");
                    }
                }
                PXLongOperation.ClearStatus(this);
                foreach (CustObject d in custDACs)
                {
                    string message;
                    if(!CustomizationChecker.TestDAC(d.Name, out message))
                    {
                        messages.Add($"{ d.Name} : {message}");
                    }
                    
                }
                var r = Base.ViewValidateExtensions.Current;
                if (messages.Any())
                    //r.Messages = string.Join("\n", messages.Select(HttpUtility.HtmlEncode));
                    Base.ViewValidateExtensions.AskExt(
                            delegate { r.Messages = string.Join("\n", messages.Select(HttpUtility.HtmlEncode)); });
            });
            //foreach (CustObject d in custDACs)
            //{
            //  d.
            //}
            //CustomizationChecker.TestDAC()

            //var cstDocuments = GetPublishedDocument();

            //PXLongOperation.StartOperation(this, delegate
            //{
            //  var messages = new RuntimeValidationManager().ValidateLookupsDefinitions();
            //  var r = ViewValidateExtensions.Current;
            //  if (messages.Any())
            //      ViewValidateExtensions.AskExt(
            //          delegate { r.Messages = string.Join("\n", messages.Select(HttpUtility.HtmlEncode)); });
            //});
            return adapter.Get();
        }
        
        #endregion
    }
}