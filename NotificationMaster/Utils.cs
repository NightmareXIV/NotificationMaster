using System.Reflection;

namespace NotificationMaster;
public static unsafe class Utils
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
