using Assets.Scripts.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    public class FormulaTreeManager
    {
        [MenuItem(StringCommon.FormulaTreeMenuItem)]
        public static void CreateFormulaData()
        {
            var formulaTree = ScriptableObject.CreateInstance<FormulaTree>();
            AssetDatabase.CreateAsset(formulaTree, StringCommon.FormulaTreePathInAssets);
        }
    }
}