using Assets.Scripts.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    [System.Serializable]
    public class FormulaObject
    {
        [SerializeField]
        public string value;

        public FormulaObject(string v)
        {
            value = v;
        }

        public FormulaObject()
        {
            value = string.Empty;
        }

        public FormulaObject Operate(FormulaObject obj, FormulaNodeOperate operate)
        {
            switch (operate)
            {
                case FormulaNodeOperate.Add:
                    {
                        return this + obj;
                    }

                case FormulaNodeOperate.Minus:
                    {
                        return this - obj;
                    }
                case FormulaNodeOperate.Mul:
                    {
                        return this * obj;
                    }
                case FormulaNodeOperate.Divid:
                    {
                        return this * obj;
                    }
                case FormulaNodeOperate.Pow:
                    {
                        return Pow(obj);
                    }
                case FormulaNodeOperate.Sqrt:
                    {
                        return Log(new FormulaObject("0.5"));
                    }
                case FormulaNodeOperate.Log:
                    {
                        return Log(obj);
                    }
            }
            return this;
        }

        public FormulaObject Log(FormulaObject obj)
        {
            obj = new FormulaObject("1.0") / obj;
            return Pow(obj);
        }

        public FormulaObject Pow(FormulaObject obj)
        {
            var intResult = 0;
            var result = Mathf.Pow(float.Parse(value), float.Parse(obj.value));
            if (int.TryParse(value, out intResult))
            {
                intResult = (int)result;
            }
            return new FormulaObject(intResult.ToString());
        }

        public static FormulaObject operator +(FormulaObject lhs, FormulaObject rhs)
        {
            var intResult = 0;
            var floatResult = 0f;
            if (int.TryParse(lhs.value, out intResult))
            {
                intResult += int.Parse(rhs.value);
                return new FormulaObject(intResult.ToString());
            }
            else if (float.TryParse(lhs.value, out floatResult))
            {
                floatResult += float.Parse(rhs.value);
                return new FormulaObject(floatResult.ToString());
            }
            return new FormulaObject(lhs.value + rhs.value);
        }

        public static FormulaObject operator -(FormulaObject lhs, FormulaObject rhs)
        {
            var opposite = new FormulaObject("-" + rhs.value);
            return lhs + opposite;
        }

        public static FormulaObject operator *(FormulaObject lhs, FormulaObject rhs)
        {
            var intResult = 0;
            var floatResult = 0f;
            if (int.TryParse(lhs.value, out intResult))
            {
                intResult *= int.Parse(rhs.value);
                return new FormulaObject(intResult.ToString());
            }
            else if (float.TryParse(lhs.value, out floatResult))
            {
                floatResult *= float.Parse(rhs.value);
                return new FormulaObject(floatResult.ToString());
            }
            return new FormulaObject(lhs.value);
        }

        public static FormulaObject operator /(FormulaObject lhs, FormulaObject rhs)
        {
            var floatResult = 0f;
            if (float.TryParse(rhs.value, out floatResult))
            {
                floatResult = 1 / floatResult;
                return lhs * new FormulaObject(floatResult.ToString());
            }
            return new FormulaObject(lhs.value);
        }
    }

    [System.Serializable]
    public class FormulaNode
    {
        public string key;
        public FormulaNode parent;
        public FormulaNodeOperate operate;
        public List<FormulaNode> childs;

        public FormulaObject value
        {
            get
            {
                return childs.Count == 0 ? GetSelfValue() : CalculateValue();
            }
        }

        public FormulaNode()
        {
            key = string.Empty;
            parent = null;
            childs = new List<FormulaNode>();
            operate = FormulaNodeOperate.None;
        }

        public FormulaNode(string k, FormulaNode p, List<FormulaNode> list, FormulaNodeOperate o)
        {
            key = k;
            parent = p;
            childs = list;
            operate = o;
            p.childs.Add(this);
        }

        private FormulaObject GetSelfValue()
        {
            var formulaObj = FormulaTree.instance.formulaObjs[key];
            return formulaObj;
        }

        private FormulaObject CalculateValue()
        {
            return childs.Select(child => FormulaTree.instance.formulaObjs[child.key]).Aggregate<FormulaObject, FormulaObject>(null, (current, obj) => obj.Operate(current, operate));
        }
    }

    [System.Serializable]
    public class FormulaObjPair
    {
        public string key;
        public string type;
        public FormulaObject value;

        public FormulaObjPair()
        {
            key = string.Empty;
            type = string.Empty;
            value = new FormulaObject();
        }

        public FormulaObjPair(string k, string t, FormulaObject f)
        {
            key = k;
            type = t;
            value = f;
        }
    }

    [System.Serializable]
    public class FormulaObjDictionary
    {
        public List<FormulaObjPair> pairs;

        public FormulaObject this[string idx]
        {
            get
            {
                return (from formulaPair in pairs where formulaPair.key == idx select formulaPair.value).FirstOrDefault();
            }
            set
            {
                foreach (var formulaPair in pairs)
                {
                    if (formulaPair.key == idx)
                    {
                        formulaPair.value = value;
                        break;
                    }
                }
            }
        }
    }
}