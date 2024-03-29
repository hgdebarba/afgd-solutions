using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    public float PaddleSpeed = 2;
    public float MaxOff = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localPos = this.transform.localPosition;

        if (Input.GetKey(KeyCode.A))
            localPos += PaddleSpeed * Vector3.left * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            localPos += PaddleSpeed * Vector3.right * Time.deltaTime;

        localPos.x = Mathf.Clamp(localPos.x, -MaxOff, MaxOff);

        this.transform.localPosition = localPos;

    }
}
