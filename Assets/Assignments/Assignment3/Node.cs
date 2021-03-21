using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AfGD.Assignment3
{
    class Node
    {
        enum Axis
        {
            X = 0,
            Z = 2,
        }

        // Children of this node
        Node m_ChildA;
        Node m_ChildB;

        // Axis along which this cell is split
        Axis m_SplitAxis;

        // Volume owned by this node.
        Bounds m_Cell;

        // Volume of the dungeon room(s) inside this node.
        // If this is a leaf node it will match the volume of the room exactly.
        // Otherwise these bounds will encapsulate all rooms inside this cell.
        Bounds m_Room;

        // Color of this node.
        // Used for debug purposes.
        Color m_Color;

        public bool IsConnected { get; private set; }
        public bool IsLeafNode => m_ChildA == null || m_ChildB == null;

        public Bounds Room => m_Room;

        public Node(Bounds data)
        {
            m_Cell = data;
            m_Color = Color.HSVToRGB(Random.value, 0.8f, 1f);
        }

        // Returns a boolean whether a cell is considered valid
        // Valid cells may have constraints on values
        // This may include but is not limited to volume and/or ratio
        bool IsValidCell()
        {
            var volume = m_Cell.size.x * m_Cell.size.y * m_Cell.size.z;
            if (volume < 20f * 2f * 20f)
                return false;

            var ratio = Mathf.Abs(m_Cell.size.x / m_Cell.size.z);
            if (ratio > 5f || ratio < .2f)
                return false;

            return true;
        }

        public void SplitCellRecursively()
        {
            // Do not attempt to split an invalid cell
            if (!IsValidCell())
                return;

            // Split this cell if it is valid and does not have children
            if (m_ChildA == null && m_ChildA == null)
                SplitCell();

            // Split its children
            m_ChildA?.SplitCellRecursively();
            m_ChildB?.SplitCellRecursively();
        }

        // Splits a cell into two smaller cells along a random axis
        // Only if the newly formed cells are both valid
        void SplitCell()
        {
            // Randomly choose an axis to split on
            int axis = (Random.value > 0.5f) ? 0 : 2;

            // Partition the Cell into two cells; cellA & cellB.
            var min = m_Cell.min;
            var max = m_Cell.max;
            var delta = max[axis] - min[axis];
            var splitPoint = Random.Range(min[axis] + .3f * delta, max[axis] - .3f * delta);
            max[axis] = splitPoint;

            var cellA = new Node(
                new Bounds
                {
                    min = min,
                    max = max
                });

            min = m_Cell.min;
            max = m_Cell.max;
            min[axis] = splitPoint;

            var cellB = new Node(
                new Bounds
                {
                    min = min,
                    max = max
                });

            // Only if we can split into two valid cells do we actually proceed with the split
            if (cellB.IsValidCell() && cellA.IsValidCell())
            {
                m_SplitAxis = (Axis)axis;

                m_ChildA = cellA;
                m_ChildB = cellB;
            }
        }

        public void GenerateRoomsRecursively()
        {
            m_ChildA?.GenerateRoomsRecursively();
            m_ChildB?.GenerateRoomsRecursively();

            if (IsLeafNode)
                GenerateRoom();
        }

        // Randomly generates a room within the bounds of this cell.
        // Also creates a mesh to represent this room.
        void GenerateRoom()
        {
            // Randomly generate bounds for the room
            var size = m_Cell.size;

            var roomSize = new Vector3(
                 Random.Range(Mathf.Min((0.5f * size.x) + 1f, size.x - 2f), size.x - 2f),
                 size.y,
                 Random.Range(Mathf.Min((0.5f * size.z) + 1f, size.z - 2f), size.z - 2f)
                );

            var x = Random.Range(0f, size.x - roomSize.x);
            var y = Random.Range(0f, size.z - roomSize.z);

            var min = new Vector3(x, 0f, y);
            var max = new Vector3(x + roomSize.x, roomSize.y, roomSize.z);

            min += m_Cell.min;
            max += m_Cell.min;

            m_Room = new Bounds { min = min, max = max };

            // Spawn Mesh that represents room
            var room = GameObject.CreatePrimitive(PrimitiveType.Cube);
            room.transform.position = m_Room.center;
            room.transform.localScale = m_Room.size;
            room.GetComponent<MeshRenderer>().material.color = m_Color;
            room.name = "Room";
        }

        public void UpdateRoomBoundsRecursively()
        {
            // Visit children first and update their bounds.
            m_ChildA?.UpdateRoomBoundsRecursively();
            m_ChildB?.UpdateRoomBoundsRecursively();

            // Only our bounds after our children.
            UpdateRoomBounds();
        }

        // Encapsulates the room bounds of the child nodes.
        // If this node is a leaf node the room bounds contain the room exactly.
        // Otherwise it is an AABB around all the nodes in descenting children.
        void UpdateRoomBounds()
        {
            if (m_ChildA != null)
            {
                if (m_Room.Equals(new Bounds()))
                    m_Room = m_ChildA.Room;
                else
                    m_Room.Encapsulate(m_ChildA.Room);
            }

            if (m_ChildB != null)
            {
                if (m_Room.Equals(new Bounds()))
                    m_Room = m_ChildB.Room;
                else
                    m_Room.Encapsulate(m_ChildB.Room);
            }
        }

        // Recursively create pathways between segments of our dungeon.
        // Returns a boolean if a change was made. 
        public bool ConnectRoomsRecursively()
        {
            // If this node is connected to the dungeon 
            // There is no need to update anything.
            if (IsConnected || IsLeafNode)
                return false;

            var childUpdated = false;

            // Update Children first
            if (m_ChildA != null)
                childUpdated |= m_ChildA.ConnectRoomsRecursively();

            if (m_ChildB != null)
                childUpdated |= m_ChildB.ConnectRoomsRecursively();

            // If a child has updated, we cannot update this frame.
            if (childUpdated)
                return true;

            ConnectChildRooms();
            return true;
        }

        void ConnectChildRooms()
        {
            var leftAABB = m_ChildA.Room;
            var rightAABB = m_ChildB.Room;

            var xAxis = m_SplitAxis == Axis.X ? Vector3.forward : Vector3.right;

            var leftRange = new Vector2(
                Vector3.Dot(leftAABB.min, xAxis),
                Vector3.Dot(leftAABB.max, xAxis)
                );

            var rightRange = new Vector2(
                Vector3.Dot(rightAABB.min, xAxis),
                Vector3.Dot(rightAABB.max, xAxis)
                );

            var overlapRange = new Vector2
                (
                   Mathf.Max(leftRange.x, rightRange.x),
                   Mathf.Min(leftRange.y, rightRange.y)
                );

            // We overlap
            if (overlapRange.x < overlapRange.y)
            {
                var overlapPos = overlapRange.x + Random.value * (overlapRange.y - overlapRange.x);

                var leftPosition = leftAABB.center;
                var rightPositon = rightAABB.center;

                int perpendicularAxis = m_SplitAxis == Axis.X ? 2 : 0;
                leftPosition[perpendicularAxis] = overlapPos;
                rightPositon[perpendicularAxis] = overlapPos;

                int axis = (int)m_SplitAxis;
                var sign = Mathf.Sign((rightPositon - leftPosition)[axis]);
                leftPosition[axis] += sign * leftAABB.extents[axis];
                rightPositon[axis] += -sign * rightAABB.extents[axis];

                var rayOrigin = 0.5f * (leftPosition + rightPositon);

                var hallwayDirection = axis == 0 ? Vector3.right : Vector3.forward;

                Physics.Raycast(rayOrigin, hallwayDirection, out var left);
                Physics.Raycast(rayOrigin, -hallwayDirection, out var right);

                var dir = left.point - right.point;

                var room = GameObject.CreatePrimitive(PrimitiveType.Cube);
                room.transform.position = 0.5f * (left.point + right.point);

                var scale = Vector3.one;
                scale[axis] = Mathf.Abs(dir[axis]);
                room.transform.localScale = scale;// new Vector3(dir.x, 1f, dir.z) + (axis == 0 ? Vector3.forward : Vector3.right);
                room.name = "hallway";
            }

            IsConnected = true;
        }

        public void DebugDraw(DrawMode drawMode)
        {
            var color = Handles.color;
            Handles.color = m_Color;

            // Select bounds based on drawing mode
            var AABB = drawMode == DrawMode.Rooms ? m_Room : m_Cell;
            var min = AABB.min;
            var max = AABB.max;
            max.y = min.y;

            // Draw a cross at the bottom of the volume
            Handles.DrawLine(min, max);
            var x = min.x; min.x = max.x; max.x = x;
            Handles.DrawLine(min, max);

            // Draw the volume
            Handles.DrawWireCube(AABB.center, AABB.size);
            Handles.Label(AABB.center, new GUIContent($"w:{AABB.size.x:N0}, h:{AABB.size.z:N0}, v:{(AABB.size.x * AABB.size.y * AABB.size.z):N0}"));

            Handles.color = color;
        }

        // Retrieves all the leaf nodes of this graph.
        public void GetLeafNodes(List<Node> nodes)
        {
            // If this is a leaf node add it to the result
            if (IsLeafNode)
            {
                nodes.Add(this);
            }
            // Otherwise keep looking
            else
            {
                m_ChildA?.GetLeafNodes(nodes);
                m_ChildB?.GetLeafNodes(nodes);
            }
        }

        // Retrieves all nodes at a certain level of the graph.
        public void GetNodesAtLevel(List<Node> nodes, int level)
        {
            // If this node is the target level add it to the result
            if (level == 0)
            {
                nodes.Add(this);
            }
            // Otherwise keep looking
            else
            {
                m_ChildA?.GetNodesAtLevel(nodes, level - 1);
                m_ChildB?.GetNodesAtLevel(nodes, level - 1);
            }
        }
    }
}