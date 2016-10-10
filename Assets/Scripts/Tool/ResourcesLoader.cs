using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Tool
{
    public class ResourcesLoader
    {
        public static Object Load(string path)
        {
            var obj = Resources.Load(path);
            return obj;
        }
    }
}