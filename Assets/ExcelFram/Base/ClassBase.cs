using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClassBase 
{
#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器下给需要转换的类进行数据填充，以完成正确的转换
    /// </summary>
    public virtual void Construction() { }
#endif

    public virtual void Init() { }
}
