using Assets.Scripts.Core.Client.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Client.Structure
{
    public class Node
    {
    }

    public class FlexNode : Node
    {
        public FlexType type;
        public int idx;
        public float time;

        public FlexNode(int i, FlexType t, float d)
        {
            idx = i;
            type = t;
            time = d;
        }

        public FlexNode()
        {
            type = FlexType.Straight;
            idx = 0;
            time = 0;
        }
    }

    public class FlexGOInfo
    {
        public FlexNode node;
        public Vector3 pos, fwd;

        public FlexGOInfo(FlexNode n, Vector3 p, Vector3 f)
        {
            node = n;
            pos = p;
            fwd = f;
        }
    }

    public class MazeInfo
    {
        public List<FlexNode> flexNodeList;
        public float scale;
        public float reactTime;
        public Vector3 startPos, startDir;

        public MazeInfo(List<FlexNode> l, float s, float r, Vector3 pos, Vector3 dir)
        {
            flexNodeList = l;
            scale = s;
            reactTime = r;
            startPos = pos;
            startDir = dir;
        }

        public MazeInfo()
        {
            flexNodeList = new List<FlexNode>();
            scale = 0.0f;
            reactTime = 0.0f;
            startPos = Vector3.zero;
            startDir = Vector3.forward;
        }
    }
}