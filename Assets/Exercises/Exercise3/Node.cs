using System;
using UnityEngine;

namespace AfGD.Execise3
{
    [Serializable]
    public class Node
    {
        public Vector3 Position => m_Position;
        public string Name => m_Name;

        // Disable field is never assigned warnings
#pragma warning disable 649
        [SerializeField] string m_Name;
        [SerializeField] Vector3 m_Position;
#pragma warning restore 649
    }
}