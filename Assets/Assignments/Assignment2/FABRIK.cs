using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD.Assignment2
{
    public class FABRIK : MonoBehaviour
    {
        [Tooltip("the joints that we are controlling")]
        public Transform[] joints;
        [Tooltip("target that our end effector is trying to reach")]
        public Transform target;

        [Tooltip("error tolerance, will stop updating after distance between end effector and target is smaller than tolerance.")]
        [Range(.01f, .2f)]
        public float tolerance = 0.05f;

        [Tooltip("maximum number of iterations before we follow to the next frame")]
        [Range(1, 100)]
        public int maxIterations = 20;

        [Tooltip("rotation constraint. " +
        	"Instead of an elipse with 4 rotation limits, " +
        	"we use a circle with a single rotation limit. " +
        	"Implementation will be simpler than in the paper.")]
        [Range(0f, 180f)]
        public float rotationLimit = 45f;

        // distances/lengths between joints.
        private float[] distances;
        // total length of the system
        private float chainLength;


        Vector3 EnforceConstraints(Vector3 P, Vector3 Pprev, Vector3 Pnext)
        {
            Vector3 Ptgt = Pnext;

            Vector3 l = P - Pprev;
            Vector3 lnext = Pnext - P;

            if (Vector3.Angle(l, lnext) < rotationLimit)
                return Ptgt;


            Vector3 o = Vector3.Project(lnext, l);

            if (Vector3.Dot(o, l) < 0 && rotationLimit <= 90)
            {
                o = -o;
                lnext = Vector3.Reflect(lnext, o);
            }

            Vector3 Po = P + o;
            float r = Mathf.Abs((float)((double)o.magnitude * (double)Mathf.Tan(rotationLimit * Mathf.Deg2Rad)));

            Debug.DrawLine(P, P + o, Color.red);
            Debug.DrawLine(P, P + lnext, Color.blue);

            Vector3 d = P + lnext - Po;
            Ptgt = Po + r * d.normalized;

            return Ptgt;
        }

        private void Solve()
        {
            // YOUR IMPLEMENTATION HERE
            // FEEL FREE TO CREATE HELPER FUNCTIONS

            float dist = (joints[0].position - target.position).magnitude;
            // out of reach
            if (dist > chainLength)
            {
                for (int i = 0; i < joints.Length - 1; i++)
                {
                    float ri = (joints[i].position - target.position).magnitude;
                    float lambdai = distances[i] / ri;
                    joints[i + 1].position = (1.0f - lambdai) * joints[i].position + lambdai * target.position;

                    Vector3 right = (joints[i + 1].position - joints[i].position).normalized;
                    Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
                    Vector3 up = Vector3.Cross(forward, right).normalized;
                    Matrix4x4 rotation = new Matrix4x4(right, up, forward, new Vector4(0, 0, 0, 1));
                    joints[i].rotation = rotation.rotation;
                    
                }
                joints[joints.Length - 1].rotation = target.rotation;
            }
            else
            {
                Vector3 b = joints[0].position;

                for (int iterations = 0; iterations < maxIterations; iterations++)
                {
                    joints[joints.Length - 1].position = target.position;

                    // forward reaching
                    for (int i = joints.Length - 2; i >= 0; i--)
                    {
                        Vector3 limitTarget = joints[i].position;
                        if (i != joints.Length - 2) // we skip the last joint
                            limitTarget = EnforceConstraints(joints[i + 1].position, joints[i + 2].position, joints[i].position);

                        float ri = (joints[i + 1].position - limitTarget).magnitude;
                        float lambdai = distances[i] / ri;
                        joints[i].position = (1 - lambdai) * joints[i + 1].position + lambdai * limitTarget;

                    }

                    // backward reaching
                    joints[0].position = b;
                    for (int i = 0; i < joints.Length - 1; i++)
                    {
                        Vector3 limitTarget = joints[i + 1].position;
                        if (i != 0) // we skip the root joint
                            limitTarget = EnforceConstraints(joints[i].position, joints[i - 1].position, joints[i + 1].position);

                        float ri = (joints[i].position - limitTarget).magnitude;
                        float lambdai = distances[i] / ri;
                        joints[i + 1].position = (1 - lambdai) * joints[i].position + lambdai * limitTarget;
                    }
                }

                // set rotation of the joints
                joints[joints.Length - 1].rotation = target.rotation;
                for (int i = 0; i < joints.Length - 1; i++)
                {
                    // build a rotation matrix by defining three orthonormal axis
                    Vector3 right = (joints[i + 1].position - joints[i].position).normalized;
                    Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;
                    Vector3 up = Vector3.Cross(forward, right).normalized;
                    Matrix4x4 rotation = new Matrix4x4(right, up, forward, new Vector4(0, 0, 0, 1));
                    joints[i].rotation = rotation.rotation;
                    // has the same effect as joints[i].right = (joints[i + 1].position - joints[i].position).normalized;
                }

            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // pre-compute segment lenghts and total length of the chain
            // we assume that the segment length (aka bone length) is constant
            distances = new float[joints.Length-1];
            chainLength = 0;
            // If we have N joints, then there are N-1 segment/bone lengths
            for (int i = 0; i < joints.Length - 1; i++)
            {
                distances[i] = (joints[i + 1].position - joints[i].position).magnitude;
                chainLength += distances[i];
            }
        }


        void Update()
        {
            Solve();
            for (int i = 1; i < joints.Length - 1; i++)
            {
                DebugJointLimit(joints[i], joints[i - 1], rotationLimit, 2);
            }
        }

        void DebugJointLimit(Transform tr, Transform trPrev, float angle, float scale = 1)
        {
            float angleRad = Mathf.Deg2Rad * angle;
            float dist = Mathf.Sin(Mathf.Deg2Rad * angle);
            int steps = 72;
            float stepSize = 360f / steps;
            // 36 is the number of line segments used to draw the cone
            for (int i = 0; i < steps; i++)
            {
                Vector3 vec = new Vector3(1, 0, 0);
                float twistRad = Mathf.Deg2Rad * i * stepSize;
                vec.x = Mathf.Cos(angleRad);
                vec.y = Mathf.Cos(twistRad) * dist;
                vec.z = Mathf.Sin(twistRad) * dist;
                vec = trPrev.rotation * vec;// * tr.localToWorldMatrix.MultiplyVector(vec);
                Debug.DrawLine(tr.position, tr.position + vec * scale, Color.white);
            }

        }
    }

}