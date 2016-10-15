using Assets.Scripts.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    public class FormulaTree : SingletonScriptObject<FormulaTree>
    {
        public FormulaNode head;
        public Dictionary<string, FormulaObject> formulaObjs;
    }
}