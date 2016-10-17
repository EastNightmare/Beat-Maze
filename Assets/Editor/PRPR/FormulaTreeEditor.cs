using Assets.Scripts.Common;
using Assets.Scripts.Core.Client.FormulaTreeManager;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.PRPR
{
    public class FormulaTreeEditor : SingtonEditor<FormulaTreeEditor>
    {
        private static float m_Height = 1000;
        private Rect m_DynamicWindowRect = new Rect(0, 0, 300, m_Height);
        private Rect m_TreeWindowRect = new Rect(300, 0, 500, m_Height);
        private Rect m_FuncWindowRect = new Rect(800, 0, 300, m_Height);
        private Rect m_TypeNameWindowRect = new Rect(100, 100, 100, 100);
        private Rect[] m_FormulaNodeRect = new Rect[255];
        private Vector2 m_DynamicWindowOffset;
        private SerializedObject m_FormulaTreeObj;
        private bool m_NewTypeName = false;
        private string m_CurTypeName = string.Empty;
        private int m_SelectedTypeIdx = 0;
        private bool m_CreateFlag = false;

        [SerializeField]
        private List<string> m_TypeNames = new List<string>();

        [MenuItem("PRPR Studio Tools/Formula Tree")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow(typeof(FormulaTreeEditor), false, "Formula Tree");
            window.Show();
        }

        private void OnEnable()
        {
            m_FormulaTreeObj = new SerializedObject(Resources.Load<FormulaTree>("Formulas/Formula Data"));
            for (int i = 0; i < m_FormulaNodeRect.Length; i++)
            {
                m_FormulaNodeRect[i] = new Rect(500, 200, 100, 100);
            }
        }

        private void OnGUI()
        {
            BeginWindows();
            m_DynamicWindowRect = GUI.Window(1, m_DynamicWindowRect, OnDynamicParamsWindow, "Dynamic Params");
            OnTreeWindow();
            m_FuncWindowRect = GUI.Window(3, m_FuncWindowRect, OnFuncWindow, "Func");
            if (m_NewTypeName)
            {
                m_TypeNameWindowRect = GUI.Window(4, m_TypeNameWindowRect, OnTypeNameWindow, "Add Type Name");
            }
            EndWindows();
        }

        private void OnTreeWindow()
        {
            GUI.Box(new Rect(300, 0, 500, 20), "Tree Graph");

            for (int i = 0; i < 1; i++)
            {
                m_FormulaNodeRect[i] = GUI.Window(5, m_FormulaNodeRect[i], OnFormulaNodeWindow, "Formula Node");
            }
        }

        private void OnFuncWindow(int idx)
        {
        }

        private void OnFormulaNodeWindow(int idx)
        {
            m_FormulaTreeObj.Update();
            GUI.DragWindow(new Rect(0, 0, 100, 10));
            var head = m_FormulaTreeObj.FindProperty("head").FindPropertyRelative("operate");
            EditorGUILayout.PropertyField(head, GUIContent.none);
            m_FormulaTreeObj.ApplyModifiedProperties();
        }

        private void OnTypeNameWindow(int idx)
        {
            GUI.DragWindow(new Rect(0, 0, 100, 10));
            m_CurTypeName = GUILayout.TextField(m_CurTypeName);
            GUILayout.Space(5);
            if (GUILayout.Button("Finish"))
            {
                m_TypeNames.Add(m_CurTypeName);
                m_SelectedTypeIdx = m_TypeNames.Count - 1;
                m_NewTypeName = false;
                m_CreateFlag = true;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Cancell"))
            {
                m_NewTypeName = false;
            }
        }

        private void OnDynamicParamsWindow(int idx)
        {
            m_FormulaTreeObj.Update();
            var pairs = m_FormulaTreeObj.FindProperty("formulaObjs").FindPropertyRelative("pairs");
            if (GUILayout.Button("New Group"))
            {
                m_NewTypeName = true;
            }
            if (m_CreateFlag)
            {
                pairs.InsertArrayElementAtIndex(pairs.arraySize);
                var newPair = pairs.GetArrayElementAtIndex(pairs.arraySize - 1);
                newPair.FindPropertyRelative("type").stringValue = m_CurTypeName;
                m_CreateFlag = false;
            }

            m_SelectedTypeIdx = GUILayout.SelectionGrid(m_SelectedTypeIdx, m_TypeNames.ToArray(), m_TypeNames.Count);
            GUILayout.BeginVertical();
            m_DynamicWindowOffset = GUILayout.BeginScrollView(m_DynamicWindowOffset, false, true);

            for (int i = 0; i < pairs.arraySize; i++)
            {
                var pair = pairs.GetArrayElementAtIndex(i);
                var key = pair.FindPropertyRelative("key");
                var value = pair.FindPropertyRelative("value");
                var typeName = pair.FindPropertyRelative("type").stringValue;
                if (!m_TypeNames.Contains(typeName))
                {
                    m_TypeNames.Add(typeName);
                    m_SelectedTypeIdx = m_TypeNames.Count - 1;
                }
                GUILayout.BeginHorizontal();

                if (typeName == m_TypeNames[m_SelectedTypeIdx])
                {
                    GUILayout.Label("Key:");
                    EditorGUILayout.PropertyField(key, GUIContent.none);
                    GUILayout.Label("Value:");
                    EditorGUILayout.PropertyField(value.FindPropertyRelative("value"), GUIContent.none);
                    if (GUILayout.Button("+", EditorStyles.miniButtonMid))
                    {
                        pairs.InsertArrayElementAtIndex(i);
                        var newPair = pairs.GetArrayElementAtIndex(i + 1);
                        newPair.FindPropertyRelative("type").stringValue = typeName;
                    }
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight))
                    {
                        pairs.DeleteArrayElementAtIndex(i);
                        if (!ContainType(typeName))
                        {
                            m_TypeNames.Remove(typeName);
                            if (m_TypeNames.Count > 0)
                            {
                                m_SelectedTypeIdx = m_TypeNames.Count - 1;
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            m_FormulaTreeObj.ApplyModifiedProperties();
        }

        private bool ContainType(string strType)
        {
            var pairs = m_FormulaTreeObj.FindProperty("formulaObjs").FindPropertyRelative("pairs");
            for (int i = 0; i < pairs.arraySize; i++)
            {
                var type = pairs.GetArrayElementAtIndex(i).FindPropertyRelative("type").stringValue;
                if (type == strType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}