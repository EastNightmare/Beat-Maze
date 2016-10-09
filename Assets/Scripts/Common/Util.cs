using DG.Tweening;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Assets.Scripts.Common
{
    public class DoTweenUtil
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
                    XmlSerializer xmlDes = new XmlSerializer(type);
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
            XmlSerializer xmlDes = new XmlSerializer(type);
            return xmlDes.Deserialize(stream);
        }

        public static string Serializer(Type type, object obj)
        {
            MemoryStream stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                xml.Serialize(stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            stream.Dispose();

            return str;
        }
    }
}