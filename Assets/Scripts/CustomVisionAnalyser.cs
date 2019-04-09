using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;


//1.将图片以字节数组形式加载
//2.使用UnityWebRequest将数据传至服务器分析
//3.获得返回的Json数据response
//4.序列化response,传至SceneOrganiser类，此类会关心如何显示response

public class CustomVisionAnalyser : MonoBehaviour {


    //该类的唯一实例，单例
    public static CustomVisionAnalyser Instance;

    private string predictionKey = "f96e02f98ad8466fbe4990a36601ee10";
    private string predictionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/b1120191-ad14-4d97-a93a-d1eb49d85aea/image?iterationId=8ade97cf-e4ef-492c-bf83-6424720893ef";

    //1.存放即将分析的图片的位数组；2.[HideInInspector]可将public变量在Unity Editor中隐藏
    [HideInInspector]public byte[] imageBytes;

    void Awake()
    {
        //确保该实例为单例
        Instance = this;
    }

    public IEnumerator AnalyseLastImageCaptured(string imagePath)
    {
        Debug.Log("Analyzing...");

        //WWWForm是一个HelperClass辅助类，用来生成表单数据，用来存储要访问服务器的数据
        WWWForm webForm = new WWWForm();

        //post方法包括uri和WWWForm表单BodyData两部分
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            imageBytes = GetImageAsByteArray(imagePath);

            //设定请求头
            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            //上传数据体至服务器端，需要指定请求头的contentType
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            //download handler帮助接收服务器端分析的数据
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            //yield只能建立在IEnumerator类中执行
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;
            Debug.Log("response: " + jsonResponse);

            
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(imageBytes);
            SceneOrganiser.Instance.quadRenderer.material.SetTexture("_MainTex", tex);
                    
            AnalysisRootObject analysisRootObject = new AnalysisRootObject();
            analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

            //Azure服务对最新照片分析完毕后，触发FinaliseLabel()方法，正确放置标签文字
            SceneOrganiser.Instance.FinaliseLabel(analysisRootObject);
            

        }
    }


    //将指定的图片内容作为字节数组返回
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

        //BinaryReader将基元数据读作二进制值
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

}
