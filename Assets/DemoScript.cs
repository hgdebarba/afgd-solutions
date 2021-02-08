using UnityEngine;

[ExecuteInEditMode]
public class DemoScript : MonoBehaviour
{
    Vector2 vec2D = Vector2.one; //(1, 1)
    Vector3 vec3D = Vector3.one; //(1, 1, 1)
    Vector3 vec3D2 = new Vector3(1, 0, 1);
    void Awake()
    {
        Debug.Log("Editor causes this Awake");
    }

    private void Start()
    {

        vec3D = vec3D / vec3D.magnitude;
    }

    void Update()
    {
        Debug.Log("Editor causes this Update");
        Debug.DrawLine(Vector3.zero, vec3D);
        Debug.Log(Vector3.Dot(vec3D, vec3D2));
    }
}