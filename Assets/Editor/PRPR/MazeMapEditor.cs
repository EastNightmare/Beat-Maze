using Assets.Scripts.Common;
using Assets.Scripts.Core.Client;
using Assets.Scripts.Core.Client.Enum;
using Assets.Scripts.Core.Client.MazeManager;
using Assets.Scripts.Core.Client.Structure;
using Assets.Scripts.PRPR_Editor;
using Assets.Scripts.Tool;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private float m_FlexTime = 0.0f;

        private Dictionary<int, FlexNode> m_Flexs;
        private GameObject m_FlexGOParent;
        private GameObject m_GizmosGO;
        private Vector2 m_ScrollPos;
        private List<int[]> m_OtherFlexIdxs;
        private AudioClip m_Clip;

        private void OnGUI()
        {
            OnGUIInfo();
            if (GUILayout.Button("Add"))
            {
                Push();
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
            if (m_Flexs == null)
            {
                return;
            }
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            foreach (var flexNode in m_Flexs)
            {
                GUILayout.Box("Flex:" + flexNode.Value.idx.ToString());
                flexNode.Value.type = (FlexType)EditorGUILayout.EnumPopup("FlexType:", flexNode.Value.type);
                flexNode.Value.time = EditorGUILayout.FloatField("Time", flexNode.Value.time);
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnGUIInfo()
        {
            this.titleContent = new GUIContent("Maze Maker");
            GUILayout.Label("Flex Direction");
            m_SelectFlexTypeIdx = GUILayout.SelectionGrid(m_SelectFlexTypeIdx, new[] { new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/LeftArrow.png") as Texture), new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/StraightArrow.png") as Texture), new GUIContent(EditorGUIUtility.Load("PRPR/Gizmos/RightArrow.png") as Texture) }, 3);
            m_Clip = EditorGUILayout.ObjectField(m_Clip, typeof(AudioClip)) as AudioClip;
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
                var go = m_FlexGOParent.transform.GetChild(m_Flexs[0].idx).gameObject;
                var startPos = m_Flexs.Count > 0 ? go.transform.position : Vector3.zero;
                var startDir = m_Flexs.Count > 0 ? go.transform.forward : Vector3.forward;
                var musicPath = AssetDatabase.GetAssetPath(m_Clip).Replace("Assets/Resources/", string.Empty);
                musicPath = musicPath.Split('.')[0];
                var mazeInfo = new MazeInfo(m_Flexs.Values.ToList(), m_Scale, m_ReactTime, startPos, startDir, musicPath);
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
                m_FlexPosition = mazeInfo.startPos;
                m_FlexForwardDir = mazeInfo.startDir;
                foreach (var node in mazeInfo.flexNodeList)
                {
                    Push(node);
                }
                m_Clip = ResourcesLoader.Load(mazeInfo.musicPath) as AudioClip;
            }
        }

        private void ClearEditorPrefs()
        {
            EditorPrefs.DeleteKey("MazeInfo.OtherFlexIdx");
            EditorPrefs.DeleteKey("MazeInfo.Flexs.Key");
            EditorPrefs.DeleteKey("MazeInfo.Flexs.Value");
        }

        private void SaveToEditorPrebs()
        {
            var strList = XmlUtil.Serializer(typeof(List<int[]>), m_OtherFlexIdxs);
            var strDicKey = XmlUtil.Serializer(typeof(List<int>), m_Flexs.Keys.ToList());
            var strDicValue = XmlUtil.Serializer(typeof(List<FlexNode>), m_Flexs.Values.ToList());
            EditorPrefs.SetString("MazeInfo.OtherFlexIdx", strList);
            EditorPrefs.SetString("MazeInfo.Flexs.Key", strDicKey);
            EditorPrefs.SetString("MazeInfo.Flexs.Value", strDicValue);
        }

        private void OnHierarchyChange()
        {
            LoadFromEditorPrefs();
        }

        private void LoadFromEditorPrefs()
        {
            var strList = EditorPrefs.GetString("MazeInfo.OtherFlexIdx");
            var strDicKey = EditorPrefs.GetString("MazeInfo.Flexs.Key");
            var strDicValue = EditorPrefs.GetString("MazeInfo.Flexs.Value");
            m_OtherFlexIdxs = XmlUtil.Deserialize(typeof(List<int[]>), strList) as List<int[]>;
            var keys = XmlUtil.Deserialize(typeof(List<int>), strDicKey) as List<int>;
            var values = XmlUtil.Deserialize(typeof(List<FlexNode>), strDicValue) as List<FlexNode>;
            m_Flexs = new Dictionary<int, FlexNode>();
            if (keys != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    m_Flexs.Add(keys[i], values[i]);
                }
            }
        }

        private void Pop()
        {
            if (m_Flexs == null || m_Flexs.Count == 0)
            {
                return;
            }
            var lastOne = m_Flexs.Keys.Last<int>();
            var go = m_FlexGOParent.transform.GetChild(lastOne).gameObject;
            OnFlexChange(false);
            OnOtherFlexChange(false);
            m_Flexs.Remove(lastOne);
            DestroyImmediate(go);
            UpdateGizmos();
        }

        private void Push(FlexNode node = null)
        {
            if (m_Flexs == null)
            {
                m_Flexs = new Dictionary<int, FlexNode>();
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

            OnFlexChange(true, node);
            OnOtherFlexChange();
            var flexType = (FlexType)m_SelectFlexTypeIdx;
            var n = node ?? new FlexNode(m_Flexs.Count, flexType, m_FlexTime);
            var info = new FlexGOInfo(n, m_FlexPosition, m_FlexForwardDir);
            var flexGO = MazeManager.CreateFlex(info);
            flexGO.transform.localScale = Vector3.one * m_Scale;
            flexGO.transform.SetParent(m_FlexGOParent.transform);
            m_Flexs.Add(flexGO.transform.GetSiblingIndex(), n);
            UpdateGizmos();
            SaveToEditorPrebs();
        }

        private void UpdateGizmos()
        {
            var lastFlex = m_Flexs.Values.Last<FlexNode>();
            var lastGO = m_FlexGOParent.transform.GetChild(m_FlexGOParent.transform.childCount - 1);
            var gizmosDir = lastFlex.type == FlexType.Left
                    ? -lastGO.transform.right
                    : (lastFlex.type == FlexType.Right ? lastGO.transform.right : lastGO.transform.forward);
            var gizmosPos = lastGO.position + gizmosDir;
            MazeMapGizmos.instance.pathPos = gizmosPos;
            MazeMapGizmos.instance.pathDirection = gizmosDir;
        }

        private void Clear()
        {
            DestroyImmediate(m_FlexGOParent);
            MazeMapGizmos.instance.DestroyInstance();
            m_FlexGOParent = null;
            m_Flexs = null;
            m_FlexPosition = Vector3.zero;
            m_FlexForwardDir = Vector3.zero;
            m_OtherFlexIdxs = null;
            m_FlexTime = 0;
            m_Clip = null;
            ClearEditorPrefs();
        }

        private void OnOtherFlexChange(bool add = true)
        {
            if (m_Flexs.Count < 1)
            {
                return;
            }
            if (m_OtherFlexIdxs == null)
            {
                m_OtherFlexIdxs = new List<int[]>();
            }
            if (add)
            {
                var preFlexGO = m_FlexGOParent.transform.GetChild(m_Flexs.Keys.ToList()[m_Flexs.Count - 1]).gameObject;
                var startPos = preFlexGO.transform.position;
                var endPos = m_FlexPosition;
                var dir = m_FlexForwardDir;
                var offsetNum = Vector3.Distance(startPos, endPos) / m_Scale;
                var num = Mathf.CeilToInt(offsetNum);
                var rest = offsetNum - num;
                rest = rest > 0 ? rest : 1 + rest;
                var goIdxs = new int[num - 1];
                for (int i = 0; i < goIdxs.Length; i++)
                {
                    var scale = m_Scale;
                    var pos = startPos + (i + 1) * dir * scale;
                    if (i == goIdxs.Length - 1)
                    {
                        scale = m_Scale * rest;
                        pos -= dir * (1 - scale) / 2;
                    }
                    var info = new FlexGOInfo(new FlexNode(), pos, dir);
                    var flexGO = MazeManager.CreateFlex(info);
                    flexGO.transform.SetParent(m_FlexGOParent.transform, false);
                    flexGO.transform.localScale = new Vector3(flexGO.transform.localScale.x * m_Scale, flexGO.transform.localScale.y * m_Scale, flexGO.transform.localScale.z * scale);
                    goIdxs[i] = flexGO.transform.GetSiblingIndex();
                }

                m_OtherFlexIdxs.Add(goIdxs);
            }
            else
            {
                if (m_OtherFlexIdxs.Count > 0)
                {
                    var goIdxs = m_OtherFlexIdxs[m_OtherFlexIdxs.Count - 1];
                    var goToRemove = new List<GameObject>();
                    foreach (var i in goIdxs)
                    {
                        var go = m_FlexGOParent.transform.GetChild(i).gameObject;
                        goToRemove.Add(go);
                    }
                    foreach (var go in goToRemove)
                    {
                        MazeManager.DestroyFlex(go);
                    }
                    m_OtherFlexIdxs.Remove(goIdxs);
                }
            }
        }

        private void OnFlexChange(bool isPush = true, FlexNode node = null)
        {
            if (m_Flexs.Count == 0)
            {
                return;
            }
            var flexGO = m_FlexGOParent.transform.GetChild(m_Flexs.Keys.Last<int>()).gameObject;
            var flexType = m_Flexs.Values.Last<FlexNode>().type;
            if (isPush)
            {
                var preFlexTime = m_Flexs.Values.Last<FlexNode>().time;
                m_FlexTime = node != null ? node.time : m_FlexTime;
                var timeBetween = m_FlexTime - preFlexTime;
                m_FlexForwardDir = flexType == FlexType.Left
                    ? -flexGO.transform.right
                    : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);
                m_FlexPosition += m_FlexForwardDir * m_Scale * (timeBetween / m_ReactTime);
            }
            else
            {
                var curFlexTime = m_Flexs.Values.Last<FlexNode>().time;
                var preFlexTime = 0f;
                if (m_Flexs.Count > 1)
                {
                    preFlexTime = m_Flexs.Values.ToList()[m_Flexs.Count - 2].time;
                }
                var timeBetween = curFlexTime - preFlexTime;

                if (m_Flexs.Count > 1)
                {
                    flexGO = m_FlexGOParent.transform.GetChild(m_Flexs.Keys.ToList()[m_Flexs.Count - 2]).gameObject; ;
                    m_FlexForwardDir = flexType == FlexType.Left
                        ? -flexGO.transform.right
                        : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);
                }
                else
                {
                    m_FlexForwardDir = Vector3.zero;
                }
                m_FlexForwardDir = flexType == FlexType.Left
                   ? -flexGO.transform.right
                   : (flexType == FlexType.Right ? flexGO.transform.right : flexGO.transform.forward);

                m_FlexPosition -= m_FlexForwardDir * m_Scale * (timeBetween / m_ReactTime);
            }
        }

        [MenuItem("PRPR Studio Tools/Maze Maker")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow(typeof(MazeMapEditor));
            window.Show();
        }
    }
}