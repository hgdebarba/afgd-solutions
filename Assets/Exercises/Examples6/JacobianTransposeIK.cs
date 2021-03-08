using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Examples6
{
    public class JacobianTransposeIK : MonoBehaviour
    {
        [Tooltip("the joints that we are controlling")]
        public Transform joint1, joint2, joint3, joint4, jointEnd;
        [Tooltip("target that our end effector is trying to reach")]
        public Transform target;

        [Tooltip("scalar used to scale down the update rate")]
        [Range(0.01f, 0.2f)]
        public float alpha = .05f;

        [Tooltip("error tolerance, will stop updating after distance between end effector and target is smaller than tolerance.")]
        [Range(0.01f, 0.5f)]
        public float tolerance = .05f;

        [Tooltip("maximum number of iterations before we follow to the next frame")]
        [Range(1, 100)]
        public int maxIterations = 20;

        [Range(0, 180)]
        public float rotationLimit = 50f;

        void Solve()
        {
            float error = (target.position - jointEnd.position).magnitude;
            int iterations = 0;
            while (error > tolerance && iterations < maxIterations)
            {
                // CLASS CODE HERE

                // find the jacobian matrix
                Vector3 f = Vector3.forward;
                Vector3 j1dv = Vector3.Cross(f, (jointEnd.position - joint1.position));
                Vector3 j2dv = Vector3.Cross(f, (jointEnd.position - joint2.position));
                Vector3 j3dv = Vector3.Cross(f, (jointEnd.position - joint3.position));
                Vector3 j4dv = Vector3.Cross(f, (jointEnd.position - joint4.position));
                Matrix4x4 J = new Matrix4x4(j1dv, j2dv, j3dv, j4dv);
                J.SetRow(3, Vector4.zero); // not using the last row, we set it to (0,0,0,0)

                // find rotation increment to move closer to a valid configuration
                Vector4 e = target.position - jointEnd.position;
                Vector4 dO = alpha * (J.transpose * e); 

                // retrieve current angles
                Vector4 O = Vector4.zero; 
                O.x = joint1.localEulerAngles.z; 
                O.y = joint2.localEulerAngles.z; 
                O.z = joint3.localEulerAngles.z;
                O.w = joint4.localEulerAngles.z;
                
                O = O + dO; // Eq. 3 in the paper – the update angles  

                // enforce joint rotation limits
                //O.x = O.x > 180f ? O.x - 360 : O.x;
                //O.y = O.y > 180f ? O.y - 360 : O.y;
                //O.z = O.z > 180f ? O.z - 360 : O.z;
                //O.w = O.w > 180f ? O.w - 360 : O.w;
                //float limit = rotationLimit;
                //O.x = Mathf.Clamp(O.x, -limit, limit);
                //O.y = Mathf.Clamp(O.y, -limit, limit);
                //O.z = Mathf.Clamp(O.z, -limit, limit);
                //O.w = Mathf.Clamp(O.w, -limit, limit);

                joint1.localEulerAngles = new Vector3(0, 0, O.x); 
                joint2.localEulerAngles = new Vector3(0, 0, O.y); 
                joint3.localEulerAngles = new Vector3(0, 0, O.z); 
                joint4.localEulerAngles = new Vector3(0, 0, O.w);
                

                error = (target.position - jointEnd.position).magnitude;
                iterations++;
            }
        }

        // Update is called once per frame
        void Update()
        {
            Solve();
        }


    }

}