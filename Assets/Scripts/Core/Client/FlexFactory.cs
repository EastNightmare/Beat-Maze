using Assets.Scripts.Common;
using Assets.Scripts.Core.Client.Enum;
using Assets.Scripts.Core.Client.Structure;
using Assets.Scripts.Tool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core.Client
{
    public class FlexFactory
    {
        public static GameObject CreateFlex(FlexGOInfo info)
        {
            var name = info.node.type.ToString() + " Flex";
            var flexGO = ResourcesLoader.Load("GameObjects/" + name) as GameObject;
            var go = Object.Instantiate(flexGO, info.pos, Quaternion.LookRotation(info.fwd)) as GameObject;
            return go;
        }

        public static void DestroyFlex(GameObject flexGO)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(flexGO);
#else
            GameObject.Destroy(flexGO);
#endif
        }

        public static List<FlexGOInfo> CreateInfoList(List<FlexNode> nodes, Vector3 pos, Vector3 fwd, float scale)
        {
            List<FlexGOInfo> infos = new List<FlexGOInfo>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var flexNode = nodes[i];
                var flexType = flexNode.type;
                if (infos.Count != 0)
                {
                    var preNode = infos[i - 1];
                    fwd = preNode.fwd;
                }
                var right = -Vector3.Cross(fwd, Vector3.up);
                var dir = flexType == FlexType.Left
                        ? -right
                        : (flexType == FlexType.Right ? right : fwd);
                if (infos.Count != 0)
                {
                    pos += dir * scale;
                }
                infos.Add(new FlexGOInfo(flexNode, pos, dir));
            }
            return infos;
        }
    }
}