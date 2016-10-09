using Assets.Scripts.Common;
using Assets.Scripts.Core.Client;
using Assets.Scripts.Core.Client.Enum;
using Assets.Scripts.Core.Client.Structure;
using Assets.Scripts.PRPR_Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.PRPR
{
    public class MazeMapEditor : SingtonEditor<MazeMapEditor>
    {
        private int m_SelectFlexTypeIdx;
        private int m_SelectEffectIdx;

        private float m_Scale = 1;
        private float m_ReactTime = 0.2f;
        private Vector3 m_FlexPosition;
        private Vector3 m_FlexForwardDir;
        private float m_FlexTime;

        private Dictionary<GameObject, FlexNode> m_FlexDic;
        private GameObject m_FlexGOParent;
        private GameObject m_GizmosGO;
        private Vector2 m_ScrollPos;

        private void OnGUI()
        {
            OnGUIInfo();
            if (GUILayout.Button("Add"))
            {
                Push();
                Func();
            }
            if (GUILayout.Button("Remove"))
            {
                Pop();
            }
            if (GUILayout.Button("Clear"))
            {
                Clear();
            }
            if (GUILayout.Button("Save"))
            {
                Save();
            }
            if (GUILayout.Button("Load"))
            {
                Load();
            }
            OnFlexNodeShow();
        }

        private void OnFlexNodeShow()
        {
            if (m_FlexDic == null)
            {
                return;
            }
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            foreach (var flexNode in m_FlexDic.Values)
            {
                GUILayout.Box("Flex:" + flexNode.idx.ToString());
                flexNode.type = (FlexType)EditorGUILayout.EnumPopup("FlexType:", flexNode.type);
                flexNode.time = EditorGUILayout.FloatField("Time", flexNode.time);
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnGUIInfo()
        {
            this.titleContent = new GUIContent("Maze Maker");
            GUILayout.Label("Flex Direction");
            m_SelectFlexTypeIdx = GUILayout.SelectionGrid(m_SelectFlexTypeIdx, new[] { new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/LeftArrow.png") as Texture), new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/StraightArrow.png") as Texture), new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/RightArrow.png") as Texture) }, 3);
            m_Scale = EditorGUILayout.FloatField("Scale", m_Scale);
            m_ReactTime = EditorGUILayout.FloatField("React Time", m_ReactTime);
            m_FlexPosition = EditorGUILayout.Vector3Field("Flex Position", m_FlexPosition);
            m_FlexForwardDir = EditorGUILayout.Vector3Field("Flex Forward Direction", m_FlexForwardDir);
            m_FlexTime = EditorGUILayout.FloatField("Flex Time", m_FlexTime);
            m_SelectEffectIdx = EditorGUILayout.Popup(m_SelectEffectIdx, new[] { "Red", "Green", "Blue" });
        }

        private void Save()
        {
            var path = EditorUtility.SaveFilePanel("Save Xml", "", "", "xml");
            if (path.Length != 0)
            {
                var startPos = m_FlexDic.Count > 0 ? m_FlexDic.Keys.First<GameObject>().transform.position : Vector3.zero;
                var startDir = m_FlexDic.Count > 0 ? m_FlexDic.Keys.First<GameObject>().transform.forward : Vector3.forward;
                var mazeInfo = new MazeInfo(m_FlexDic.Values.ToList(), m_Scale, m_ReactTime, startPos, startDir);
                var xmlStr = XmlUtil.Serializer(typeof(MazeInfo), mazeInfo);
                File.WriteAllText(path, xmlStr);
            }
            AssetDatabase.Refresh();
        }

        private void Load()
        {
            var path = EditorUtility.OpenFilePanel("Load Xml", "", "xml");
            if (path.Length != 0)
            {
                Clear();
                var xmlStr = File.ReadAllText(path);
                var mazeInfo = XmlUtil.Deserialize(typeof(MazeInfo), xmlStr) as MazeInfo;
                foreach (var node in mazeInfo.flexNodeList)
                {
                    Push(node);
                }
            }
        }

        private void Func()
        {
        }

        private void Pop()
        {
            if (m_FlexDic == null || m_FlexDic.Count == 0)
            {
                return;
            }
            OnFlexChange(false);
            var go = m_FlexDic.Keys.Last<GameObject>();
            m_FlexDic.Remove(go);
            DestroyImmediate(go);
        }

        private void Push(FlexNode node = null)
        {
            if (m_FlexDic == null)
            {
                m_FlexDic = new Dictionary<GameObject, FlexNode>();
            }
            if (m_FlexGOParent == null)
            {
                m_FlexGOParent = new GameObject("Flexs");
            }
            if (m_GizmosGO == null)
            {
                m_GizmosGO = MazeMapGizmos.instance.gameObject;
                m_GizmosGO.name = "Maze Maker Gizmos";
            }
            var flexType = (FlexType)m_SelectFlexTypeIdx;
            node = node ?? new FlexNode(m_FlexDic.Count, flexType, m_FlexTime);
            var info = new FlexGOInfo(node, m_FlexPosition, m_FlexForwardDir);
            var flexGO = FlexFactory.CreateFlex(info);
            m_FlexDic.Add(flexGO, node);
            flexGO.transform.localScale = Vector3.one * m_Scale;
            flexGO.transform.SetParent(m_FlexGOParent.transform);

            OnFlexChange();
            OnOtherFlexAdd();
        }

        private void Clear()
        {
            DestroyImmediate(m_FlexGOParent);
            MazeMapGizmos.instance.DestroyInstance();
            m_FlexGOParent = null;
            m_FlexDic = null;
            m_FlexPosition = Vector3.zero;
            m_FlexForwardDir = Vector3.zero;
        }

        private void OnOtherFlexAdd()
        {
            if (m_FlexDic.Count <= 1)
            {
                return;
            }
            var list = m_FlexDic.ToList();
            var preFlexGO = list[m_FlexDic.Count - 2].Key;
            var curFlexGO = list[m_FlexDic.Count - 1].Key;
            var startPos = preFlexGO.transform.position;
            var endPos = curFlexGO.transform.position;
            var dir = curFlexGO.transform.forward;
            var num = Mathf.Ceil(Vector3.Distance(startPos, endPos) / m_Scale);
            for (int i = 1; i < num; i++)
            {
                var info = new FlexGOInfo(new FlexNode(), startPos + i * dir * m_Scale, dir);
                var flexGO = FlexFactory.CreateFlex(info);
                flexGO.transform.SetParent(m_FlexGOParent.transform, false);
            }
        }

        private void OnFlexChange(bool isPush = true)
        {
            if (m_FlexDic.Count == 0)
            {
                m_FlexForwardDir = Vector3.zero;
                m_FlexPosition = Vector3.zero;
            }
            else
            {
                if (isPush)
                {
                    var flexType = m_FlexDic.Values.Last<FlexNode>().type;
                    var flexGO = m_FlexDic.Keys.Last<GameObject>();
                    var flexTime = m_FlexDic.Values.Last<FlexNode>().time;
                    m_FlexForwardDir = flexType == FlexType.Left
                        ? -flexGO.transform.right
                        : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);
                    m_FlexPosition += m_FlexForwardDir * m_Scale * (flexTime / m_ReactTime);
                }
                else
                {
                    var flexType = m_FlexDic.Values.Last<FlexNode>().type;
                    var flexTime = m_FlexDic.Values.Last<FlexNode>().time;
                    var flexGO = m_FlexDic.Keys.Last<GameObject>();
                    m_FlexForwardDir = flexType == FlexType.Left
                        ? -flexGO.transform.right
                        : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);
                    m_FlexPosition -= m_FlexForwardDir * m_Scale * (flexTime / m_ReactTime);
                    foreach (var pair in m_FlexDic)
                    {
                        if (pair.Value.idx == m_FlexDic.Count - 2)
                        {
                            flexType = pair.Value.type;
                            flexGO = pair.Key;
                            break;
                        }
                    }
                    m_FlexForwardDir = flexType == FlexType.Left
                       ? -flexGO.transform.right
                       : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);
                }
            }
            MazeMapGizmos.instance.pathPos = m_FlexPosition;
            MazeMapGizmos.instance.pathDirection = m_FlexForwardDir;
        }

        [MenuItem("PRPR Studio Tools/Maze Maker")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow(typeof(MazeMapEditor));
            window.Show();
        }
    }
}