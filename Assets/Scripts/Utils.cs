using UnityEngine;

public class Utils
{
    public static void Elapse(ref float timer, float amount, float max = Mathf.Infinity)
    {
        timer = Mathf.Min(timer + amount, max + Mathf.Epsilon);
    }

    public static Vector3 FlipX(Vector3 v) {
        return Vector3.Reflect(v, Vector3.left);
        // v.x *= -1;
        // return v;
    }

    public static void DebugDrawRect(Vector3 pos, float size, Color color)
    {
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
        Debug.DrawLine(new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
    }
    public static void DebugDrawRect(Vector3 position, float size)
    {
        DebugDrawRect(position, size, Color.red);
    }
    public static void DebugDrawRect(Vector3 position, Color color)
    {
        DebugDrawRect(position, .1f, color);
    }
    public static void DebugDrawRect(Vector3 position)
    {
        DebugDrawRect(position, .1f, Color.red);
    }

    // check to see whether a LayerMask contains a layer
    // see: https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html
    public static bool LayerMaskContainsLayer(int mask, int layer) {
        bool contains = ((mask & (1 << layer)) != 0);
        return contains;
    }

    // get the layer num from a layermask
    // see: https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-3021162
    public static int ToLayer(int layerMask) {
        int result = layerMask > 0 ? 0 : 31;
        while( layerMask > 1 ) {
            layerMask = layerMask >> 1;
            result++;
        }
        return result;
    }

    
    public static bool shouldBlink(float timeElapsed, float rate) {
        return (timeElapsed / rate % 2f) < 1f;
    }

    // Get a child game object by name or tag
    // see: https://answers.unity.com/questions/183649/how-to-find-a-child-gameobject-by-name.html
    public static GameObject FindChild(GameObject parent, string lookup) {
        Transform[] ts = parent.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == lookup || t.gameObject.tag == lookup) return t.gameObject;
        return null;
    }

    // Helper method that gets a component and also asserts its presence - useful for early-error pattern
    // 
    // USAGE:
    // ```
    // Rigidbody2D rb = GetRequiredComponent<Rigiidbody2D>(this);
    // ```
    public static T GetRequiredComponent<T>(GameObject gameObject)
    {
        T component = gameObject.GetComponent<T>();
        AppIntegrity.AssertPresent<T>(component);
        return component;
    }

    public static GameObject GetRequiredChild(GameObject parent, string lookup)
    {
        GameObject child = Utils.FindChild(parent, lookup);
        AppIntegrity.AssertPresent<GameObject>(child);
        return child;
    }

    public static string ToTimeString(float t) {
        string minutes = Mathf.Floor(t / 60).ToString("0");
        string seconds = (t % 60).ToString("00");
        return string.Format("{0}:{1}", minutes, seconds);
    }
}
