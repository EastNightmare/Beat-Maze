using DG.Tweening;
using System;
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