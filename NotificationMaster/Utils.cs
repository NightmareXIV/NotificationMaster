using ECommons;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster;
public unsafe static class Utils
{
    public static bool IsApplicationActivated => P.ThreadUpdActivated.IsApplicationActivated;
    public static Type GetTypeFromRuntimeAssembly(string assemblyName, string type)
    {
        try
        {
            var fType = Assembly.Load(assemblyName);
            var t = fType.GetType(type);
            return t;
        }
        catch(Exception e)
        {
            e.Log();
        }
        return null;
    }
}
