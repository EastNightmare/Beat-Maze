﻿using UnityEngine;

namespace Assets.Scripts.Core.Client.FormulaTreeManager
{
    public enum FormulaNodeType
    {
        Add,
        Minus,
        Mul,
        Divid,
        Pow,
        Sqrt,
        Log,
        None,

        Constance,
        JsonData,
        Variable,

        Array,
    }
}