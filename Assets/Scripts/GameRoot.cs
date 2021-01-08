using UnityEngine;

public class GameRoot : MonoBehaviour
{
    string XmlPath = "Assets/ExcelFram/XmlData/";

    void Awake()
    {
        ReadXml();
    }

    private void ReadXml()
    {
        TestData testData = SerializeOpt.XmlDeserialize<TestData>(XmlPath + "TestData.Xml");
        Debug.Log(testData.DataInt);
    }
}
