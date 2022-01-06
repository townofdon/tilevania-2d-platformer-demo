
using UnityEngine;

public class AppIntegrity
{
    public static void AssertPresent(object value)
    {
        if (value == null) Debug.LogError("FAILED NULL CHECK: " + value.GetType());
    }

    public static void AssertNonZero(object value)
    {
        AppIntegrity.AssertPresent(value);
        if ((int)value == 0) Debug.LogError("FAILED NON-ZERO CHECK: " + value.GetType());
    }
}
