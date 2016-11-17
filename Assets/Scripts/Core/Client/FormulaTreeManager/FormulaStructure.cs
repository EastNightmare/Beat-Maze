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

        public FormulaObject Operate(FormulaObject obj, FormulaNodeType type)
        {
            switch (type)
            {
                case FormulaNodeType.Add:
                    {
                        return this + obj;
                    }

                case FormulaNodeType.Minus:
                    {
                        return this - obj;
                    }
                case FormulaNodeType.Mul:
                    {
                        return this * obj;
                    }
                case FormulaNodeType.Divid:
                    {
                        return this * obj;
                    }
                case FormulaNodeType.Pow:
                    {
                        return Pow(obj);
                    }
                case FormulaNodeType.Sqrt:
                    {
                        return Log(new FormulaObject("2"));
                    }
                case FormulaNodeType.Log:
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
        public FormulaNodeType type;
        public List<FormulaNode> childs;

        public FormulaObject value
        {
            get
            {
                switch (type)
                {
                    case FormulaNodeType.Variable:
                        {
                            return GetSelfValue();
                        }
                    case FormulaNodeType.JsonData:
                        {
                            var idx = "0";
                            var parentArray = GetParent(node => node.type == FormulaNodeType.Array);
                            idx = parentArray.GetChild(child => child.key == "Index").value.value;
                            return new FormulaObject((string)ConfigManager.ConfigManager.instance.pool[idx][key]);
                        }
                    case FormulaNodeType.Constance:
                        {
                            return new FormulaObject(key);
                        }
                    case FormulaNodeType.Array:
                        {
                            return new FormulaObject(key);
                        }
                    default:
                        {
                            return CalculateValue();
                        }
                }
            }
        }

        public FormulaNode()
        {
            key = string.Empty;
            parent = null;
            childs = new List<FormulaNode>();
            type = FormulaNodeType.None;
        }

        public FormulaNode(string k, FormulaNode p, List<FormulaNode> list, FormulaNodeType t)
        {
            key = k;
            parent = p;
            childs = list;
            type = t;
            p.childs.Add(this);
        }

        private FormulaObject GetSelfValue()
        {
            var formulaObj = FormulaTree.instance.formulaObjs[key];
            return formulaObj;
        }

        private FormulaObject CalculateValue()
        {
            return childs.Select(child => FormulaTree.instance.formulaObjs[child.key]).Aggregate<FormulaObject, FormulaObject>(null, (current, obj) => obj.Operate(current, type));
        }

        private FormulaNode GetParent(Func<FormulaNode, bool> matchFunc)
        {
            var p = parent;
            if (p == null)
            {
                return p;
            }
            var isMatch = matchFunc(parent);
            if (!isMatch)
            {
                return p.GetParent(matchFunc);
            }
            return p;
        }

        private FormulaNode GetChild(Func<FormulaNode, bool> matchFunc)
        {
            if (childs == null)
            {
                return null;
            }
            foreach (var child in childs)
            {
                var isMatch = matchFunc(child);
                if (isMatch)
                {
                    return child;
                }
                else
                {
                    return child.GetChild(matchFunc);
                }
            }
            return null;
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