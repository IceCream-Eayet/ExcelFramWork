using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

public static class SerializeOpt 
{
    /// <summary>
    /// 类序列化成xml
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool XmlSerialize(string path, object obj)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    XmlSerializer xs = new XmlSerializer(obj.GetType());
                    xs.Serialize(sw, obj);
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("此类无法转换成xml： " + obj.GetType() + "，" + e);
        }
        return false;
    }

    /// <summary>
    /// 反序列化xml文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T XmlDeserialize<T>(string path) where T : class
    {
        T t = default(T);
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                 XmlSerializer xs = new XmlSerializer(typeof(T));
                 t = (T)xs.Deserialize(fs);
            }
            return t;
        }
        catch (Exception e)
        {
            Debug.LogError("Xml文件读取错误：" + path + "，" + e);
        }
        return t;
    }

    /// <summary>
    /// 反序列化xml文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object XmlDeserialize(string path, Type type)
    {
        object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xs = new XmlSerializer(type);
                obj = xs.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Xml文件读取错误： " + path + "，" + e);
        }
        return obj;
    }
    /// <summary>
    /// 类序列化成二进制
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool BinarySerialize(string path, object obj)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BinaryFormatter xs = new BinaryFormatter();
                xs.Serialize(fs, obj);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("此类无法转换成二进制： " + obj.GetType() + "，" + e);
        }

        return false;
    }
    /// <summary>
    /// 反序列化二进制
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static object BinaryDeserialize(string path)
    {
        object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BinaryFormatter xs = new BinaryFormatter();
                obj = xs.Deserialize(fs);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("二进制文件读取错误： " + path + "，" + e);
        }

        return obj;
    }
}
