using System;

namespace AfGD.Execise3
{
    // Struct representing a connection
    // between two nodes in a graph.
    [Serializable]
    public struct Edge
    {
        public int From;
        public int To;
        public float Cost;

        // Returns whether this Edge 
        // is valid to be used
        public bool IsValid()
        {
            return From >= 0
                && To >= 0
                && Cost >= 0f;
        }
    }
}