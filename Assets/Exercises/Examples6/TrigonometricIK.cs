using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Examples6
{
    // this implementation only works in the x/y plane
    // it assumes that a segment with angle 0 points to +x-axis, and +90 points to +y-axis
    public class TrigonometricIK : MonoBehaviour
    {
        [Tooltip("the joints that we are controlling")]
        public Transform topJoint, midJoint, endJoint;
        [Tooltip("target that our endJoint is trying to reach")]
        public Transform target;
        [Tooltip("there are two possible solutions to the 3 joint IK problem on the plane")]
        public bool alternateSolution = false;

        // rotation angle to be used for rotating top and middle joints
        private float topAngle = 0, midAngle = 0;


        public void Solve()
        {
            // CLASS CODE HERE
            Vector3 t2tgt = target.position - topJoint.position;  // these are the lengths of the edges of our triangle
            float a = (midJoint.position - topJoint.position).magnitude;
            float b = (endJoint.position - midJoint.position).magnitude;float c = t2tgt.magnitude;
            float angleToTgt = Vector3.Angle(Vector3.right, t2tgt.normalized); 
            // we need to make a distinction between clockwise and counter-cw
            if (t2tgt.y < 0) angleToTgt *= -1;

            if (a + b < c)
            { // not a valid triangle! 
                // kinematic chain is fully extended 
                topAngle = angleToTgt;
                midAngle = 0;
            }
            else
            {
                // law of cosines to solve for the angles 
                float gamma = Mathf.Acos((-c * c + a * a + b * b) / (2 * a * b));
                gamma *= Mathf.Rad2Deg * (alternateSolution ? -1 : 1);

                float beta = Mathf.Acos((-b * b + a * a + c * c) / (2 * a * c));
                beta *= Mathf.Rad2Deg * (alternateSolution ? -1 : 1);
                // transform from angles in a triangle to angles in our plane
                topAngle = angleToTgt + beta;
                midAngle = 180 + gamma;
            }

            // we control rotation, notice that we assume all rotations are around the z axis
            topJoint.localRotation = Quaternion.AngleAxis(topAngle, Vector3.forward);
            midJoint.localRotation = Quaternion.AngleAxis(midAngle, Vector3.forward);
        }


        void Update()
        {
            Solve();

        }
    }

}