using Customization;
using PX.Caching;
using PX.Common;
using PX.Customization;
using PX.Data;
using PX.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Customization
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
    public class CustomizationChecker : CustomizationPlugin
    {
        public void WriteLog(string msg,string css)
        {
            //using reflection to get internal write log to gain access to controlling color of log text.
            //we have no choice here as this version of writelog is Internal and not otherwise accessible.
            Assembly.Load("PX.Web.Customization").GetType("PX.Customization.CstPublishLog").GetMethod("WriteLog").Invoke(null, new object[] { msg, css });
        }
        [InjectDependency]
        protected ICacheControl<PageCache> PageCacheControl { get; set; }
        [InjectDependency]
        protected IScreenInfoCacheControl ScreenInfoCacheControl { get; set; }
        public override void UpdateDatabase()
        {

            WriteLog(string.Format("Testing {0}", Assembly.GetExecutingAssembly().GetName().Name));
            //Get all IBQL Tables to test SQL
            //PXDatabase.ResetSlots();
            //Database.setslot.SetSlot<bool>("PublishTest", true);
            foreach (var row in GetInstances<IBqlTable>())
            {
                var item = row.Value;
                //Confirm via Reflection that the DAC is not a Virtual (Unbound) DAC or that there are PXDB fields,e.g. PXDBString
                var test1 = item.GetType().CustomAttributes.Where(c => c.AttributeType.Name.Contains("Virtual")).Count() == 0;
                var test2 = item.GetType().GetProperties().Where(p => p.CustomAttributes.Where(t => t.AttributeType.Name.Contains("DB")).Count() > 0).Count() > 0;

                if (test1 &&  test2)
                {
                    //log what table we are testing
                    this.WriteLog(string.Format("Testing Table {0}", item.GetType().Name));
                    try
                    {
                        //create dynamic PXSelect command
                        var command = BqlCommand.Compose(typeof(Select<>), item.GetType());
                        //creater view using PXSelect from above
                        var view = new PXView(new PXGraph(), true, BqlCommand.CreateInstance(command));
                        //Note: this test will only work if IsActive=true on all extensions being tested. You may need to publish, activate and publish again to fully test.
                        //Note: Alternative is to create methodology to activate extensions for the test. This would require refactoring that would make this testing engine not universal.
                        //Select first row of table.
                        var test = view.SelectSingle(new List<object>().ToArray());
                        //make sure extensions are tested.

                        ((IBqlTable)test).GetExtensions();
                                                                                                                                                                                 
                        //log successful test
                        WriteLog(string.Format("Testing of Table {0} Successful", item.GetType().Name),"color:green");

                    }
                    catch (Exception ex)
                    {
                        //if error is generated then log using Red Text
                        WriteLog(string.Format("Testing of Table {0} Unsuccessful with error {1}", item.GetType().Name, ex.Message), "color:red;");
                    }
                }
            }
            PXContext.ClearSlot(typeof(bool), "PublishTest");
        }
       
        private static Dictionary<string,T> GetInstances<T>()
        {
            var retVal = new Dictionary<string, T>();
           
            foreach(var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                //test Type to see if it is same class as T
                if ((item.GetInterfaces().Contains(typeof(T)) && item.GetConstructor(Type.EmptyTypes) != null))
                {
                    //Create Instance of T
                    var entry = (T)Activator.CreateInstance(item);
                    //if we don't already have one add to return 
                    if(!retVal.Keys.Contains(entry.GetType().Name))
                        retVal.Add(entry.GetType().Name,entry);
                }
                else
                {
                    //look to see if Type has a Base property and if that Base is same class a T
                    var BaseProperty = item.BaseType?.GetRuntimeProperties().FirstOrDefault(p=>p.Name=="Base")?.PropertyType;
                    if((BaseProperty?.GetInterfaces()?.Contains(typeof(T))??false) && BaseProperty?.GetConstructor(Type.EmptyTypes) != null)
                    {
                        //Create Instance of T
                        var entry = (T)Activator.CreateInstance(BaseProperty);
                        //if we don't already have one add to return 
                        if (!retVal.Keys.Contains(entry.GetType().Name))
                            retVal.Add(entry.GetType().Name, entry);
                    }
                }
               
            }
            return retVal;
        }

    }
}
