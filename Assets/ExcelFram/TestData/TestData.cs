using System.Collections.Generic;
using System.Xml.Serialization;

[System.Serializable]
public class TestData : ClassBase
{
#if UNITY_EDITOR
    public override void Construction()
    {
        DataInt = 0;
        dataInt = 1;
        dataFloat = 0.2f;
        DataFloat = 0.1f;
        dataString = "dataString";
        DataString = "DataString";

        DataStringArray = new string[10];
        for (int i = 0; i < 10; i++)
        {
            DataStringArray[i] = DataString + "_" + i.ToString();
        }

        DataList = new List<DataClass>();
        for (int i = 0; i < 10; i++)
        {
            DataClass dataClass = new DataClass
            {
                DataInt = i * 6,
                DataFloat = i + 0.002f,
                DataString = "String" + "_" + i.ToString()
            };
            DataList.Add(dataClass);
        }
    }
#endif

    public override void Init()
    {
        DataInt = 0;
        dataInt = 0;
        dataFloat = 0.1f;
        DataFloat = 0.1f;
        dataString = "dataString";
        DataString = "DataString";
        DataStringArray = new string[100];
        DataList = new List<DataClass>(10);
    }

    [XmlElement]
    public int DataInt { get; set; }
    [XmlElement("DataFloat")]
    public float DataFloat { get; set; }
    [XmlElement("DataString")]
    public string DataString { get; set; }

    [XmlElement("dataInt")]
    public int dataInt;
    [XmlElement("dataFloat")]
    public float dataFloat;
    [XmlElement("dataString")]
    public string dataString;

    [XmlElement("DataList")]
    public List<DataClass> DataList = new List<DataClass>();
    [XmlElement("DataStringArray")]
    public string[] DataStringArray { get; set; }
}

[System.Serializable]
public class DataClass 
{
    [XmlAttribute("DataInt")]
    public int DataInt { get; set; }
    [XmlAttribute("DataFloat")]
    public float DataFloat { get; set; }
    [XmlAttribute("DataString")]
    public string DataString { get; set; }
}
