using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Assets.Scripts.Common
{
    public class DOTweenUtil
    {
        public static Sequence Delay(Action callFunc, float dt)
        {
            var seq = DOTween.Sequence();
            seq.AppendInterval(dt);
            seq.AppendCallback(() =>
            {
                callFunc();
            });
            seq.Play();
            return seq;
        }
    }

    public class ArrayUtil<T>
    {
        public static bool Contains(T[] array, T value)
        {
            return new List<T>(array).Contains(value);
        }

        public static T[] Add(T[] array, T value)
        {
            return new List<T>(array) { value }.ToArray();
        }

        public static T[] Remove(T[] array, T value)
        {
            var list = new List<T>(array);
            list.Remove(value);
            return list.ToArray();
        }
    }

    public class StringUtil
    {
        public static string LastAfter(string str, char split)
        {
            var strArray = str.Split(split);
            return strArray[strArray.Length - 1];
        }

        public static string BeginBefore(string str, char split)
        {
            return str.Split(split)[0];
        }
    }

    public class XmlUtil
    {
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    var xmlDes = new XmlSerializer(type);
                    return xmlDes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static object Deserialize(Type type, Stream stream)
        {
            var xmlDes = new XmlSerializer(type);
            return xmlDes.Deserialize(stream);
        }

        public static string Serializer(Type type, object obj)
        {
            var stream = new MemoryStream();
            var xml = new XmlSerializer(type);
            try
            {
                xml.Serialize(stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var str = sr.ReadToEnd();

            sr.Dispose();
            stream.Dispose();

            return str;
        }
    }
}