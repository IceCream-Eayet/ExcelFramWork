using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class DataTool
{
    private static string dataPath = "Assets/ExcelFram/XmlData/";

    #region 类转xml
    [MenuItem("Assets/Excel和Xml/Class转Xml")]
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            //显示进度条
            EditorUtility.DisplayProgressBar("文件下的类转成xml", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            ClassToXml(objs[i].name);
        }
        //刷新资源管理器
        AssetDatabase.Refresh();
        //关闭进度条
        EditorUtility.ClearProgressBar();
    }

    private static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("错误：Name为空," + name);
            return;
        }
        try
        {
            Type type = null;
            //AppDomain，应用程序域，GetAssemblies方法可以获取当前程序域中的程序集(Assembly数组)
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(name);
                if (type != null)  break;
            }
            if (type != null)
            {
                var temp = Activator.CreateInstance(type);

                if (temp is ClassBase)
                {
                    (temp as ClassBase).Construction();
                }

                string xmlPath = dataPath + name + ".xml";
                bool isDone = SerializeOpt.XmlSerialize(xmlPath, temp);
                if(isDone)
                    Debug.Log(name + "类转xml成功，xml路径为:" + xmlPath);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(name + "类转xml失败！" + e.Message);
        }
    }
    #endregion

    #region Xml转Excel
    [MenuItem("Assets/Excel和Xml/Xml转Excel")]
    public static void AssetsXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            //显示进度条
            EditorUtility.DisplayProgressBar("文件下的Xml转Excel", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            XmlToExcel(objs[i].name);
            Debug.Log(objs[i].name + "Xml转换Excel已完成，路径为：" + dataPath + objs[i].name + ".xlsx");
        }
        //刷新资源管理器
        AssetDatabase.Refresh();
        //关闭进度条
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 读取xml数据，并填写到excel
    /// </summary>
    /// <param name="name"></param>
    private static void XmlToExcel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("错误：Name为空," + name);
            return;
        }

        try
        {
            string path = dataPath + name + ".xlsx";
            if (File.Exists(path))  File.Delete(path);
            if (FileIsUsed(path))   Debug.LogError("文件被占用，无法修改！");

            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) 
            {
                //读取xml的数据
                Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
                Dictionary<string, SheetWork> sheetWorkDic = new Dictionary<string, SheetWork>();
                ReadXml(name, ref sheetDataDic, ref sheetWorkDic);

                //填写数据到excel
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    foreach (var sheetWork in sheetWorkDic.Values)
                    {
                        string sheetWorkName = GetDicKey(sheetWorkDic, sheetWork);
                        ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add(sheetWorkName);

                        List<string> sheetName = new List<string>();
                        for (int i = 0; i < sheetWork.allSheetData.Count; i++) 
                        {
                            if (!sheetName.Contains(sheetWork.allSheetData[i].Name))
                            {
                                sheetName.Add(sheetWork.allSheetData[i].Name);
                            }
                        }

                        for(int j = 0; j < sheetName.Count; j++)
                        {
                            List<string> sheetDataData = new List<string>();
                            foreach(var sheetData in sheetWork.allSheetData)
                            {
                                if (sheetName[j] == sheetData.Name) 
                                {
                                    sheetDataData.Add(sheetData.Data);
                                }
                            }
                            ExcelRange range = worksheet1.Cells[1, j + 1];
                            range.Value = sheetName[j];
                            range.AutoFitColumns();

                            for(int col = 0; col < sheetDataData.Count; col++)
                            {
                                ExcelRange rangeCol = worksheet1.Cells[col + 2 , j + 1];
                                rangeCol.Value = sheetDataData[col];
                                rangeCol.AutoFitColumns();
                            }
                        }
                    }

                    ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add(name + "Single");
                    int row2 = 1;
                    foreach (var sheetData in sheetDataDic.Values) 
                    {
                        ExcelRange range1 = worksheet2.Cells[1,row2];
                        range1.Value = sheetData.Name;
                        range1.AutoFitColumns();

                        ExcelRange range2 = worksheet2.Cells[2,row2];
                        range2.Value = sheetData.Data;
                        range2.AutoFitColumns();
                        row2++;
                    }
                    package.Save();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(name + "Xml转Excel失败！" + e.Message);
        }
    }
    /// <summary>
    /// 读取Xml文件数据到对应的字典
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sheetDataDic"></param>
    /// <param name="sheetWorkDic"></param>
    private static void ReadXml(string name, ref Dictionary<string, SheetData> sheetDataDic, ref Dictionary<string, SheetWork> sheetWorkDic)
    {
        string path = dataPath + name + ".xml";
        Type type = null;
        //AppDomain，应用程序域，GetAssemblies方法可以获取当前程序域中的程序集(Assembly数组)
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(name);
            if (type != null) break;
        }

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        object obj = SerializeOpt.XmlDeserialize(path, type);
        Dictionary<string, object> objDic = GetObjectMembers(obj, bindingFlags);
        GetSheetDataValue(objDic, ref sheetDataDic, ref sheetWorkDic);
    }
    /// <summary>
    /// 获得读取到的数据到Excel相关数据类字典
    /// </summary>
    /// <param name="objDic"></param>
    /// <param name="sheetDataDic"></param>
    /// <param name="sheetWorkDic"></param>
    private static void GetSheetDataValue(Dictionary<string, object> objDic, ref Dictionary<string, SheetData> sheetDataDic, ref Dictionary<string, SheetWork> sheetWorkDic)
    {
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        foreach (var obj1 in objDic.Values)
        {
            if (obj1 != null)
            {
                if (obj1.GetType() == typeof(string[]))
                {
                    string[] str = (string[])obj1;
                    SheetWork sheetWork = new SheetWork();
                    foreach (var item in str)
                    {
                        SheetData sheetData = new SheetData();
                        sheetData.Name = GetDicKey(objDic, obj1);
                        sheetData.Type = item.GetType();
                        sheetData.Data = item.ToString();
                        sheetWork.allSheetData.Add(sheetData);
                    }
                    sheetWorkDic.Add(sheetWork.allSheetData[0].Name, sheetWork);
                }
                else if (obj1.GetType().IsGenericType)
                {
                    IEnumerable<object> objItems = obj1 as IEnumerable<object>;
                    SheetWork sheetWork = new SheetWork();
                    string name = null;
                    foreach (var item in objItems)
                    {
                        if (item == null) continue;
                        name = GetDicKey(objDic, obj1) + "(" + item.GetType().Name + ")";
                        Dictionary<string, object> itemDic = GetObjectMembers(item, bindingFlags);
                        foreach(var iDic in itemDic.Values)
                        {
                            if (iDic == null) continue;
                            SheetData sheetData = new SheetData();
                            sheetData.Name = GetDicKey(itemDic, iDic);
                            sheetData.Type = iDic.GetType();
                            sheetData.Data = iDic.ToString();
                            sheetWork.allSheetData.Add(sheetData);
                        }
                    }
                    sheetWorkDic.Add(name, sheetWork);
                }
                else if (obj1.GetType() == typeof(string) || obj1.GetType() == typeof(int) || obj1.GetType() == typeof(float))
                {
                    SheetData sheetData = new SheetData();
                    sheetData.Name = GetDicKey(objDic, obj1);
                    sheetData.Type = obj1.GetType();
                    sheetData.Data = obj1.ToString();
                    sheetDataDic.Add(sheetData.Name, sheetData);
                }
            }
        }
    }
    /// <summary>
    /// 通过反射获得类的全部字段和属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="bindingFlags"></param>
    /// <returns></returns>
    private static Dictionary<string, object> GetObjectMembers(object obj, BindingFlags bindingFlags)
    {
        MemberInfo[] infoes = obj.GetType().GetMembers(bindingFlags);
        Type type = obj.GetType();
        Dictionary<string, object> objDic = new Dictionary<string, object>();

        foreach (var info in infoes)
        {
            object obj1 = null;
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    obj1 = type.GetField(info.Name, bindingFlags).GetValue(obj);
                    break;
                case MemberTypes.Property:
                    obj1 = type.GetProperty(info.Name, bindingFlags).GetValue(obj);
                    break;
                default:
                    break;
            }

            if (obj1 != null)
            {
                objDic.Add(info.Name, obj1);
            }
        }
        return objDic;
    }

    #endregion

    #region Excel转Xml
    [MenuItem("Assets/Excel和Xml/Excel转Xml")]
    public static void AssetsExcelToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            //显示进度条
            EditorUtility.DisplayProgressBar("文件下的Excel转Xml", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);

            ExcelToXml(objs[i].name);
            //Debug.Log("Excel转Xml已完成：" + objs[i].name);
            Debug.Log(objs[i].name + "Excel转Xml已完成，路径为：" + dataPath + objs[i].name + ".xml");
        }
        //刷新资源管理器
        AssetDatabase.Refresh();
        //关闭进度条
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 读取Excel数据并转换到Xml文件
    /// </summary>
    /// <param name="name"></param>
    private static void ExcelToXml(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("错误：Name为空," + name);
            return;
        }
        string ExcelPath = dataPath + name + ".xlsx";
        object obj = ReadExcelToClass(ExcelPath, name);
        SerializeOpt.XmlSerialize(dataPath + name + ".xml", obj);
    }
    /// <summary>
    /// 读取Excel数据
    /// </summary>
    /// <returns></returns>
    private static object ReadExcelToClass(string path,string name)
    {
        Type type = null;
        //AppDomain，应用程序域，GetAssemblies方法可以获取当前程序域中的程序集(Assembly数组)
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(name);
            if (type != null) break;
        }
        var obj = Activator.CreateInstance(type);
        InvokeMethod(obj, "Init");

        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
        ExcelPackage package = new ExcelPackage(file);
        ExcelWorksheets excelWorksheets = package.Workbook.Worksheets;

        foreach(var workSheet in excelWorksheets)
        {
            if (workSheet.Name.EndsWith("Single"))
            {
                for (int i = 1; i < ExcelPackage.MaxColumns; i++)
                {
                    if (workSheet.Cells[1, i].Value == null) break;
                    ExcelRange rangeValue = workSheet.Cells[2, i];
                    SetPropertyValue(obj, workSheet.Cells[1, i].Value.ToString(), rangeValue.Value.ToString());
                }
                for (int i = 1; i < ExcelPackage.MaxColumns; i++)
                {
                    if (workSheet.Cells[1, i].Value == null) break;
                    ExcelRange rangeValue = workSheet.Cells[2, i];
                    SetFieldValue(obj, workSheet.Cells[1, i].Value.ToString(), rangeValue.Value.ToString());
                }
            }
            else if (workSheet.Name.Contains("(") && workSheet.Name.Contains(")"))
            {
                string listName = workSheet.Name.Split('(', ')')[0];
                string className = workSheet.Name.Split('(', ')')[1];
                object classMember = GetObjectMember(obj, listName);

                Type classType = null;
                //AppDomain，应用程序域，GetAssemblies方法可以获取当前程序域中的程序集(Assembly数组)
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    classType = asm.GetType(className);
                    if (classType != null) break;
                }
                object tempList = CreateList(classType);

                if (classMember.GetType().IsGenericType)
                {
                    for (int j = 2; j < ExcelPackage.MaxRows; j++)
                    {
                        if (workSheet.Cells[j, 1].Value == null) break;
                        var classObj = Activator.CreateInstance(classType);
                        for (int i = 1; i < ExcelPackage.MaxColumns; i++)
                        {
                            ExcelRange range = workSheet.Cells[1, i];
                            ExcelRange rangeValue = workSheet.Cells[j, i];
                            if (range.Value == null || rangeValue.Value == null) break;
                            SetPropertyValue(classObj, range.Value.ToString(), rangeValue.Value.ToString());
                        }
                        tempList.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, tempList, new object[] { classObj });
                    }
                }
                if (obj.GetType().GetProperty(listName) != null)
                {
                    obj.GetType().GetProperty(listName).SetValue(obj, tempList);
                }
                else if(obj.GetType().GetField(listName) != null)
                {
                    obj.GetType().GetField(listName).SetValue(obj, tempList);
                }
            }
            else 
            {
                object classMember = GetObjectMember(obj, workSheet.Name);
                if (classMember.GetType() == typeof(string[]))
                {
                    for (int i = 1; i <= ExcelPackage.MaxRows; i++)
                    {
                        ExcelRange range = workSheet.Cells[i + 1, 1];
                        if (range.Value == null) break;

                        (classMember as string[])[i - 1] = range.Value.ToString();
                    }
                }
            }
        }

        return obj;
    }
    /// <summary>
    /// 设置指定类中指定属性的值
    /// </summary>
    /// <param name="classObj"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    private static void SetPropertyValue(object classObj,string name,string value)
    {
        if (classObj == null) return;
        PropertyInfo[] infoes = classObj.GetType().GetProperties();
        foreach (var info in infoes)
        {
            if (info.Name == name)
            {
                if (info.PropertyType == typeof(int))
                {
                    info.SetValue(classObj, Convert.ToInt32(value));
                }
                else if (info.PropertyType == typeof(string))
                {
                    info.SetValue(classObj, value);
                }
                else if (info.PropertyType == typeof(float))
                {
                    info.SetValue(classObj, Convert.ToSingle(value));
                }
                else
                {
                    Debug.LogError("ReadExcelToClass: 不支持的数据类型," + info.GetType());
                }
            }
        }
    }
    /// <summary>
    /// 设置指定类中指定字段的值
    /// </summary>
    /// <param name="classObj"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    private static void SetFieldValue(object classObj, string name, string value)
    {
        if (classObj == null) return;
        FieldInfo[] infoes = classObj.GetType().GetFields();
        foreach (var info in infoes)
        {
            if (info.Name == name)
            {
                if (info.FieldType == typeof(int))
                {
                    info.SetValue(classObj, Convert.ToInt32(value));
                }
                else if (info.FieldType == typeof(string))
                {
                    info.SetValue(classObj, value);
                }
                else if (info.FieldType == typeof(float))
                {
                    info.SetValue(classObj, Convert.ToSingle(value));
                }
                else
                {
                    Debug.LogError("ReadExcelToClass: 不支持的数据类型," + info.GetType());
                }
            }
        }
    }
    /// <summary>
    /// 反射new一個list
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object CreateList(Type type)
    {
        Type listType = typeof(List<>);
        Type specType = listType.MakeGenericType(new System.Type[] { type });//确定list<>里面T的类型
        return Activator.CreateInstance(specType, new object[] { });//new出来这个list
    }
    /// <summary>
    /// 根据反射获取类中指定名称的属性或者字段，默认取第一个
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private static object GetObjectMember(object obj,string name)
    {
        MemberInfo[] infoes = obj.GetType().GetMember(name,BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
        Type type = obj.GetType();
        object objInfo = null;
        foreach(var info in infoes)
        {
            if(info != null )
            {
                if(info.MemberType == MemberTypes.Property)
                {
                    objInfo = type.GetProperty(info.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(obj);
                }
                else if(info.MemberType == MemberTypes.Field)
                {
                    objInfo = type.GetField(info.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(obj);
                }
            }
        }
        return objInfo;
    }
    /// <summary>
    /// 根据反射，执行类中指定名字的方法
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    private static void InvokeMethod(object obj, string name)
    {
        MethodInfo info = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        info.Invoke(obj, new object[] { });
    }
    #endregion

    #region 类转二进制
    [MenuItem("Assets / Excel和Xml / Class转Binary")]
    public static void AssetsClassToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            //显示进度条
            EditorUtility.DisplayProgressBar("文件下的Class转Binary", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);

            ClassToBinary(objs[i].name);
            //Debug.Log("Class转Binary已完成：" + objs[i].name);
            Debug.Log(objs[i].name + "Class转Binary已完成，路径为：" + dataPath + objs[i].name + ".byte");
        }
        //刷新资源管理器
        AssetDatabase.Refresh();
        //关闭进度条
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 类转换为二进制（XML数据为基础数据）
    /// </summary>
    /// <param name="name"></param>
    private static void ClassToBinary(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("错误：Name为空," + name);
            return;
        }
        try
        {
            Type type = null;
            //AppDomain，应用程序域，GetAssemblies方法可以获取当前程序域中的程序集(Assembly数组)
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(name);
                if (type != null) break;
            }
            if (type != null)
            {
                var temp = Activator.CreateInstance(type);

                if (temp is ClassBase)
                {
                    (temp as ClassBase).Construction();
                }

                string xmlPath = dataPath + name + ".xml";
                string binaryPath = dataPath + name + ".byte";
                object obj = SerializeOpt.XmlDeserialize(xmlPath, type);
                SerializeOpt.BinarySerialize(binaryPath, temp);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(name + "类转xml失败！" + e.Message);
        }
        
    }
    #endregion

    /// <summary>
    /// 通过字典的value查找它的key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <param name="dic"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    private static T GetDicKey<T, K>(Dictionary<T, K> dic, K k)
    {
        foreach(KeyValuePair<T,K> pair in dic)
        {
            if (pair.Value.Equals(k))
            {
                return pair.Key;
            }
        }
        return default(T);
    }
    /// <summary>
    /// 判断文件是否被占用
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool FileIsUsed(string path)
    {
        bool result = false;

        if (!File.Exists(path))
        {
            result = false;
        }
        else
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                result = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                result = true;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }
        return result;
    }
}


