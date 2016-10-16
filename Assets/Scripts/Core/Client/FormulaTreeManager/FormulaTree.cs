using Assets.Scripts.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    [CreateAssetMenu()]
    public class FormulaTree : SingletonScriptObject<FormulaTree>
    {
        public FormulaNode head;
        public FormulaObjDictionary formulaObjs;
    }
}