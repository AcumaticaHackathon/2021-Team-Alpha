//using Customization;
using PX.Common;
using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AcuChecker
{
    public static class extensionclass
    {
        private const BindingFlags everything = BindingFlags.Instance |
                           BindingFlags.Public |
                           BindingFlags.NonPublic |
                           BindingFlags.Static;

        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(everything);
            return properties;
        }
    }
    public class CustomizationChecker //: CustomizationPlugin
    {
        private static MethodInfo logger;
        private static MethodInfo Logger
        {
            get
            {
                if (logger == null)
                    logger = Assembly.Load("PX.Web.Customization").GetType("PX.Customization.CstPublishLog").GetMethod("WriteLog");
                return logger;
            }
        }
        public static void WriteLog(string msg, string css="")
        {
            //using reflection to get internal write log to gain access to controlling color of log text.
            //we have no choice here as this version of writelog is Internal and not otherwise accessible.
            Logger.Invoke(null, new object[] { msg, css });
        }
        private static string GetPath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }
        //public override void UpdateDatabase()
        //{
        //    var custDLLs = PXSelect<CustObject, Where<CustObject.name, Like<Required<CustObject.name>>, And<CustObject.type, Equal<Required<CustObject.type>>>>>.Select(new PXGraph(), "%.dll", "file").RowCast<CustObject>().ToList();
        //    AcuChecker.CustomizationChecker.WriteLog(string.Format("Testing {0}", Assembly.GetExecutingAssembly().GetName().Name));
        //    //Get all IBQL Tables to test SQL
        //    foreach (var custDLL in custDLLs)
        //        AcuChecker.CustomizationChecker.TestDLL(custDLL.Name);
                
        //}
        public static bool DllExists(string DLLName)
        {
            try
            {
                return System.IO.File.Exists(DLLName?.Replace(@"File#", GetPath()));
            }
            catch
            {
                return false;
            }
        }
        public static List<Tuple<bool, string>> TestDLL(string DLLName=null)
        {
            List<Tuple<bool, string>> retVal = new List<Tuple<bool, string>>();
            foreach (var row in GetInstances<IBqlTable>(DLLName?.Replace(@"File#", GetPath())))
            {
                if (TestDAC(row.Value, out string msg))
                {
                    WriteLog(msg, "color:green;");
                    retVal.Add(new Tuple<bool, string>(true, msg));// string.Format("{0}|{1}|{2}",DLLName,msg,true);
                }
                else
                {
                    WriteLog(msg, "color:red;");
                    retVal.Add(new Tuple<bool, string>(false, msg));//retVal += string.Format("{0}|{1}|{2}", DLLName, msg, false);
                }
                
            }
            return retVal;
        }
        public static bool TestDAC(string table, out string error)
        {
            var Table = Type.GetType(table);
            return TestDAC(Table, out error);
        }

        public static bool TestDAC(Type table, out string error)
        {
            try { 
            //Confirm via Reflection that the DAC is not a Virtual (Unbound) DAC
            var test1 = table.CustomAttributes.Where(c => c.AttributeType.Name.Contains("Virtual")).Count() == 0;
            //check that there are PXDB fields,e.g. PXDBString
            var test2 = table.GetProperties().Where(p => p.CustomAttributes.Where(t => t.AttributeType.Name.Contains("DB")).Count() > 0).Count() > 0;
            error = "";
            if (test1 && test2)
            {
                //log what table we are testing
                WriteLog(string.Format("Testing Table {0}", table.Name), "font-weight:bold");
                try
                {
                    //create dynamic PXSelect command
                    var command = BqlCommand.Compose(typeof(Select<>), table);
                    //creater view using PXSelect from above
                    var view = new PXView(new PXGraph(), true, BqlCommand.CreateInstance(command));
                    //Note: this test will only work if IsActive=true on all extensions being tested. You may need to publish, activate and publish again to fully test.
                    //Note: Alternative is to create methodology to activate extensions for the test. This would require refactoring that would make this testing engine not universal.
                    //Select first row of table.
                    var test = view.SelectSingle(new List<object>().ToArray());
                    //make sure extensions are tested.

                    ((IBqlTable)test).GetExtensions();

                    //log successful test
                    //WriteLog(string.Format("Testing of Table {0} Successful", table.GetType().Name), "color:green");
                    error = string.Format("Testing of Table {0} Successful", table.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    //if error is generated then log using Red Text
                    //WriteLog(string.Format("Testing of Table {0} Unsuccessful with error {1}", table.GetType().Name, ex.Message), "color:red;");
                    error = string.Format("Testing of Table {0} Unsuccessful with error {1}", table.Name, ex.Message);
                    return false;
                }
            }
            }
            catch
            {
                error = "";
            }
            return false;
        }

        private static Dictionary<string, Type> GetInstances<T>(string dll = null) 
        {
            var retVal = new Dictionary<string, Type>();
            var dlls = string.IsNullOrWhiteSpace(dll) ? Assembly.GetExecutingAssembly().GetTypes() : Assembly.LoadFile(dll).GetTypes();
            foreach (var item in dlls)
            {
                //test Type to see if it is same class as T
                if ((item.GetInterfaces().Contains(typeof(T)) && item.GetConstructor(Type.EmptyTypes) != null))
                {
                    //Create Instance of T
                    //var entry = (T)Activator.CreateInstance(item);
                    //if we don't already have one add to return 
                    if (!retVal.Keys.Contains(item.Name))//entry.GetType().Name))
                        retVal.Add(item.Name,item);// entry.GetType().Name, entry.GetType());
                }
                else
                {
                    //look to see if Type has a Base property and if that Base is same class a T
                    var BaseProperty = item.BaseType?.GetRuntimeProperties().FirstOrDefault(p => p.Name == "Base")?.PropertyType;
                    if ((BaseProperty?.GetInterfaces()?.Contains(typeof(T)) ?? false) && BaseProperty?.GetConstructor(Type.EmptyTypes) != null)
                    {
                        //Create Instance of T
                        //var entry = (T)Activator.CreateInstance(BaseProperty);
                        //if we don't already have one add to return 
                        if (!retVal.Keys.Contains(BaseProperty.Name))//entry.GetType().Name))
                            retVal.Add(BaseProperty.Name, BaseProperty);// entry.GetType().Name, entry.GetType());
                    }
                }

            }
            return retVal;
        }

    }
}
