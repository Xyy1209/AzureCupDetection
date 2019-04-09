using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;
//Linq用于查询


public class ImageCapture : MonoBehaviour {


    public static ImageCapture Instance;

    private int captureCount = 0;
    private PhotoCapture photoCaptureObject = null;
    private GestureRecognizer recognizer;

    internal bool captureIsActive;

    //现在被分析的图片路径
    internal string filePath = string.Empty;


    private void Awake()
    {
        //单例
        Instance = this;
    }



    // Use this for initialization
    void Start ()
    {
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        var fileInfo = info.GetFiles();

        //清空该应用的LocalState文件夹
        foreach (var file in fileInfo)
        {
            try
            {
                file.Delete();
            }
            catch(Exception)
            {
                Debug.LogFormat("Cannot delete file: ", file.Name);
            }
        }

        //d订阅手势识别，识别Tap手势
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
           
	}



    private void TapHandler(TappedEventArgs obj)
    {
        if(!captureIsActive)
        {
            captureIsActive = true;

            Debug.Log("Start Capturing...");
            //Tap手势点击进行拍照时，cursor为红色。（1）Cursor红色，camera忙，不可以； （2）cursor绿色，camera可用。
            SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.red;
            
            //Invoke方法：多少秒后，执行某个函数.
            //Invoke不接受含有参数的方法；
            Invoke("ExecuteImageCaptureAndAnalysis", 0);

        }
    }


    private void ExecuteImageCaptureAndAnalysis()
    {
        SceneOrganiser.Instance.PlaceAnalysisLabel();

       
            if(PhotoCapture.SupportedResolutions==null)
            {
                Debug.Log("SupportedResolutions is null.");
            }
            else
            {
            Debug.Log("SupportedResolution is not null. " );
            }
          
           
        
  
        
         //此句出错 队列为空
         //解决方式：老版HoloLens系统可运行，RS4不可。
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First(); 
     
        Texture2D targetTexture = new Texture2D(cameraResolution.width , cameraResolution.height);

        PhotoCapture.CreateAsync(true,delegate (PhotoCapture captureObject)
        {
            //photoCaptureObject可在该类的所有方法中使用
            photoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters
            {
                hologramOpacity = 1.0f,
                cameraResolutionWidth = targetTexture.width,
                cameraResolutionHeight = targetTexture.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };


            captureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
             {
                 string filename = string.Format(@"CapturedImage{0}.jpg", captureCount);
                 filePath = Path.Combine(Application.persistentDataPath, filename);
                 captureCount++;
                 photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
             });
        }
        );

    }



    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        try
        {
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        catch(Exception e)
        {
            Debug.LogFormat("Exception capturing photo to disk: {0}", e.Message);
        }
    }


    //当拍照完成时，触发AnalyseLastImageCaptured（）方法，对最新照片进行分析
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Stopped Photo Mode...");

        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        StartCoroutine(CustomVisionAnalyser.Instance.AnalyseLastImageCaptured(filePath));
    }


   internal void ResetImageCapture()
    {
        captureIsActive = false;

        SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.green;
        //取消该脚本中的所有调用
        CancelInvoke();
    }


}

