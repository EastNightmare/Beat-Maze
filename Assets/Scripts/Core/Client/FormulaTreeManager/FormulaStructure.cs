using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    public class FormulaObject
    {
        public object value;

        public FormulaObject(object v)
        {
            value = v;
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
                        return Log(new FormulaObject(0.5f));
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
            obj = new FormulaObject(1f) / obj;
            return Pow(obj);
        }

        public FormulaObject Pow(FormulaObject obj)
        {
            var strValue = value.ToString();
            var intResult = 0;
            var floatResult = 0f;
            var result = Mathf.Pow((float)value, (float)obj.value);
            if (int.TryParse(strValue, out intResult))
            {
                value = (int)result;
            }
            else if (float.TryParse(strValue, out floatResult))
            {
                value = (float)result;
            }
            return new FormulaObject(value);
        }

        public static FormulaObject operator +(FormulaObject lhs, FormulaObject rhs)
        {
            var lStr = lhs.value.ToString();
            var rStr = rhs.value.ToString();
            var intResult = 0;
            var floatResult = 0f;
            if (int.TryParse(lStr, out intResult))
            {
                intResult += int.Parse(rStr);
                return new FormulaObject(intResult);
            }
            else if (float.TryParse(lStr, out floatResult))
            {
                floatResult += float.Parse(rStr);
                return new FormulaObject(floatResult);
            }
            return new FormulaObject(lStr + rStr);
        }

        public static FormulaObject operator -(FormulaObject lhs, FormulaObject rhs)
        {
            var rStr = rhs.value.ToString();
            var intResult = 0;
            var floatResult = 0f;
            if (int.TryParse(rStr, out intResult))
            {
                intResult = -intResult;
                return lhs + new FormulaObject(intResult);
            }
            else if (float.TryParse(rStr, out floatResult))
            {
                floatResult = -floatResult;
                return lhs + new FormulaObject(floatResult);
            }
            return lhs;
        }

        public static FormulaObject operator *(FormulaObject lhs, FormulaObject rhs)
        {
            var lStr = lhs.value.ToString();
            var rStr = rhs.value.ToString();
            var intResult = 0;
            var floatResult = 0f;
            if (int.TryParse(lStr, out intResult))
            {
                floatResult *= int.Parse(rStr);
                return new FormulaObject(floatResult);
            }
            else if (float.TryParse(lStr, out floatResult))
            {
                floatResult *= float.Parse(rStr);
                return new FormulaObject(floatResult);
            }
            return lhs;
        }

        public static FormulaObject operator /(FormulaObject lhs, FormulaObject rhs)
        {
            var rStr = rhs.value.ToString();
            var floatResult = 0f;
            if (float.TryParse(rStr, out floatResult))
            {
                floatResult = 1 / floatResult;
                return lhs * new FormulaObject(floatResult);
            }
            return lhs;
        }
    }

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
            operate = FormulaNodeOperate.Add;
        }

        public FormulaNode(string k, FormulaNode p, List<FormulaNode> list)
        {
            key = k;
            parent = p;
            childs = list;
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
}