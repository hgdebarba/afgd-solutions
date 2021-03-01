using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD
{

    [System.Serializable]
    public struct SkeletonJoint
    {
        public Matrix4x4 m_invBindPose;
        public string m_name;
        public byte m_iParent;
    }

    [System.Serializable]
    public class Skeleton 
    {
        public SkeletonJoint[] m_aJoint;

        /// <summary>
        /// Compute joint poses in model space given the skeleton hierarchy and 
        /// an animation pose (local space poses)
        /// </summary>
        /// <param name="animPose">animation pose with local space transforms</param>
        /// <param name="poseGlobal">output transformations in model space</param>
        public void GetPoseInGlobalSpace(AnimationPose animPose, ref Matrix4x4[] globalPose)
        {            
            // KEEP THIS check validity of the arguments
            int jointCount = m_aJoint.Length;
            if (animPose.m_aLocalPose.Length != jointCount)
                Debug.LogError("[Skeleton] the AnimationPose and Skeleton don't match");
            if (globalPose.Length != jointCount) globalPose = new Matrix4x4[jointCount];

            // TODO exercise 1.
            // You should 
            // 1. iterate through all the joints, 
            // 2. use the hierarchy of joints to retrieve the local transformation of each joint in the animPose,
            // 3. concatenate the local transformations until you reach the root joint of the skeleton (m_iParent == 255)
            // 4. store the resultint matrix on poseGlobal, at the same index as the joint
            // Replace with the correct code
            //for (int i = 0; i < animPose.m_aLocalPose.Length; i++)
            //    globalPose[i] = animPose.m_aLocalPose[i].Matrix();


            for (int i = 0; i < jointCount; i++)
            {
                Matrix4x4 P = animPose.m_aLocalPose[i].Matrix();
                byte parentId = m_aJoint[i].m_iParent;

                // go up the hierarchy 
                while (true)
                {
                    // check if it has reached the parent joint (we assume it to be == 255)
                    if (parentId == 255) break;

                    P = animPose.m_aLocalPose[parentId].Matrix() * P;
                    parentId = m_aJoint[parentId].m_iParent;
                }

                globalPose[i] = P;
            }
        }


    }

}
