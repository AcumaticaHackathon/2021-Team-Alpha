using System;
using PX.Data;

namespace AcuChecker
{
  [Serializable]
  [PXCacheName("Acucheckpref")]
  public class Acucheckpref : IBqlTable
  {
    #region EnableTests
    [PXDBBool()]
    [PXUIField(DisplayName = "Enable Tests")]
    public virtual bool? EnableTests { get; set; }
    public abstract class enableTests : PX.Data.BQL.BqlBool.Field<enableTests> { }
    #endregion
  }
}