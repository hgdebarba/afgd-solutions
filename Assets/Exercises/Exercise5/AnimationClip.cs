using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD
{
    public struct AnimationClip
    {
        public Skeleton m_skeleton;
        public float m_framesPerSecond;
        public float m_speedMultiplier;
        public int m_frameCount;
        public AnimationPose[] m_aLocalPose;
        public bool m_isLooping;
    }
}