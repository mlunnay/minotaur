using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MinotaurTests.Common.GUI
{
  /// <summary>
  /// binds a value of an objects parameter or field so that changes in a GUI control are made to the parameter of field
  /// </summary>
  public class Binding
  {
    private MemberInfo _boundMember;
    private object _boundObject;

    public Binding(object target, string path)
    {
      _boundObject = target;
      _boundMember = target.GetType().GetMember(path).FirstOrDefault();
    }

    public void Set(object value)
    {
      FieldInfo finfo = _boundMember as FieldInfo;
      if (finfo != null)
      {
        finfo.SetValue(_boundObject, value);
        return;
      }

      PropertyInfo pinfo = _boundMember as PropertyInfo;
      if (pinfo != null)
      {
        pinfo.SetValue(_boundObject, value, null);
      }
    }

    public T Get<T>()
    {
      FieldInfo finfo = _boundMember as FieldInfo;
      if (finfo != null)
      {
        return (T)finfo.GetValue(_boundObject);
      }

      PropertyInfo pinfo = _boundMember as PropertyInfo;
      if (pinfo != null)
      {
        return (T)pinfo.GetValue(_boundObject, null);
      }

      return default(T);
    }
  }
}
