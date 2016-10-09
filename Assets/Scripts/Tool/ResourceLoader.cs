using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Tool
{
    public class ResourceLoader
    {
        public static Object Load(string path)
        {
            var obj = Resources.Load(path);
            return obj;
        }
    }
}