
using UnityEngine;

public class AppIntegrity
{
    private static bool IsNull(object value)
    {
        if (value == null) return true;
        // Unity treats `null` differently for game objects
        if (value is GameObject && !((GameObject)value)) return true;

        return false;
    }

    private static System.Type GetType(object value) {
        try
        {
             return value.GetType();
        }
        catch (System.Exception)
        {
            
            return null;
        }
    }

    public static void AssertPresent(object value)
    {
        if (AppIntegrity.IsNull(value)) Debug.LogException(new System.Exception("FAILED NULL CHECK: " + AppIntegrity.GetType(value)));
    }
    public static void AssertPresent<T>(object value)
    {
        if (AppIntegrity.IsNull(value)) Debug.LogException(new System.Exception("FAILED NULL CHECK: " + typeof(T)));
    }

    public static void AssertNonZero(object value)
    {
        AppIntegrity.AssertPresent(value);
        if ((int)value == 0) Debug.LogException(new System.Exception("FAILED NON-ZERO CHECK: " + value.GetType()));
    }
}
