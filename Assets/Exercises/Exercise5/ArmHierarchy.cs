using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace AfGD
{

    public class ArmHierarchy : MonoBehaviour
    {

        Skeleton arm;
        AnimationPose[] animationPoses;

        AnimationPose currentPose;


        [Range(0.1f, 10.0f)]
        public float animationTime = 2.0f;

        private void Start()
        {
            Init();
        }

        void Init()
        {
            if (animationPoses != null)
                return;

            // create the arm skeleton
            SkeletonJoint shoulder = new SkeletonJoint() { m_invBindPose = Matrix4x4.identity, m_name = "shoulder", m_iParent = 255 };
            SkeletonJoint elbow = new SkeletonJoint() { m_invBindPose = Matrix4x4.identity, m_name = "elbow", m_iParent = 0 };
            SkeletonJoint wrist = new SkeletonJoint() { m_invBindPose = Matrix4x4.identity, m_name = "wrist", m_iParent = 1 };
            SkeletonJoint[] joints = new SkeletonJoint[3] { shoulder, elbow, wrist };
            arm = new Skeleton() { m_aJoint = joints };

            // frame 0
            JointPose shoulder0 = new JointPose{
                m_rot = Quaternion.AngleAxis(0.0f, Vector3.forward),
                m_pos = new Vector3(0.0f, 0.0f, 0.0f),
                m_scale = 1.0f
            };
            JointPose elbow0 = new JointPose { 
                m_rot = Quaternion.AngleAxis(0.0f, Vector3.forward),
                m_pos = new Vector3(1.0f, 0.0f, 0.0f), 
                m_scale = 1.0f 
            };
            JointPose wrist0 = new JointPose { 
                m_rot = Quaternion.AngleAxis(0.0f, Vector3.forward), 
                m_pos = new Vector3(1.0f, 0.0f, 0.0f),
                m_scale = 1.0f 
            };
            JointPose[] poses0 = new JointPose[3] { shoulder0, elbow0, wrist0 };
            AnimationPose frame0 = new AnimationPose { m_aLocalPose = poses0 };

            // frame 1
            JointPose shoulder1 = new JointPose
            {
                m_rot = Quaternion.AngleAxis(60.0f, Vector3.forward),
                m_pos = new Vector3(0.0f, 0.0f, 0.0f),
                m_scale = 1.0f
            };
            JointPose elbow1 = new JointPose
            {
                m_rot = Quaternion.AngleAxis(-65.0f, Vector3.forward),
                m_pos = new Vector3(1.0f, 0.0f, 0.0f),
                m_scale = 1.0f
            };
            JointPose wrist1 = new JointPose
            {
                m_rot = Quaternion.AngleAxis(60.0f, Vector3.forward),
                m_pos = new Vector3(1.0f, 0.0f, 0.0f),
                m_scale = 1.0f
            };

            JointPose[] poses1 = new JointPose[3] { shoulder1, elbow1, wrist1 };
            AnimationPose frame1 = new AnimationPose { m_aLocalPose = poses1 };

            // array of animation poses
            animationPoses = new AnimationPose[2] { frame0, frame1 };

            // initialize current pose
            JointPose[] poses = new JointPose[3] { shoulder0, wrist0, wrist0 };
            currentPose = new AnimationPose { m_aLocalPose = poses };
        }

        private void Update()
        {
            // we get our normilized time (in the range [0, 1])
            float t = Time.time;
            t = t % animationTime / animationTime;


            // TODO for each local pose of a joint:
            // perform interpolation between two consecutive AnimationFrames (animationPoses)
            // should interpolate rotation, translation and scale
            // save the result in the currentPose object
            for (int i = 0; i < currentPose.m_aLocalPose.Length; i++)
            {
                Vector3 p0 = animationPoses[0].m_aLocalPose[i].m_pos;
                Vector3 p1 = animationPoses[1].m_aLocalPose[i].m_pos;
                currentPose.m_aLocalPose[i].m_pos = Vector3.Lerp(p0, p1, t);

                Quaternion q0 = animationPoses[0].m_aLocalPose[i].m_rot;
                Quaternion q1 = animationPoses[1].m_aLocalPose[i].m_rot;
                currentPose.m_aLocalPose[i].m_rot = Quaternion.Slerp(q0, q1, t);
                
                float s0 = animationPoses[0].m_aLocalPose[i].m_scale;
                float s1 = animationPoses[1].m_aLocalPose[i].m_scale;
                currentPose.m_aLocalPose[i].m_scale = Mathf.Lerp(s0, s1, t);
            }

        }

        private void OnDrawGizmos()
        {

            Init();

            // retrieve the current pose in 
            Matrix4x4[] globalPose = new Matrix4x4[arm.m_aJoint.Length];
            arm.GetPoseInGlobalSpace(currentPose, ref globalPose);

            int jointCount = arm.m_aJoint.Length;
            for (int i = 0; i < jointCount; i++)
            {
                // ith joint in world space
                Matrix4x4 P = transform.localToWorldMatrix * globalPose[i];
                // draw sphere at joint location
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(P.GetColumn(3), 0.05f);
                // draw local basis vectors
                DebugDraw.DrawFrame(P, .2f); 

                // draw a line connecting the joints
                byte parentId = arm.m_aJoint[i].m_iParent;
                // skip this one if it is the root
                if (parentId == 255) 
                    continue;
                Debug.DrawLine(P.GetColumn(3), 
                    (transform.localToWorldMatrix * globalPose[parentId]).GetColumn(3), Color.cyan);

            }

        }


    }
}