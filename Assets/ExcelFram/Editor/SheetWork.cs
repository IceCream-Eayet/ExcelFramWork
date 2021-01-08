using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Excel标签页数据类
/// </summary>
public class SheetWork
{
    public List<SheetData> allSheetData = new List<SheetData>();
}

/// <summary>
/// Excel表格数据类
/// </summary>
public class SheetData
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public string Data { get; set; }
}
