using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//该脚本用来序列化和反序列化对CustomVisionService的调用


public class CustomVisionObjects : MonoBehaviour
{


}

//IMultipartFormSection使UnityWebRequest可以将复杂的数据序列化至恰当的字节
class MultipartObject : IMultipartFormSection
{
    public string sectionName { get; set; }

    public byte[] sectionData { get; set; }

    public string fileName { get; set; }

    public string contentType { get; set; }

}

//所有的标签
public class Tags_RootObject
{
    public List<TagOfProject> Tags { get; set; }
    public int TotalTaggedImages { get; set; }
    public int TotalUntaggedImages { get; set; }
}


//单个标签
public class TagOfProject
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int ImageCount { get; set; }
}


//一张图片含有的标签（可能多个）
public class Tag_RootObject
{
    public List<Tag> Tags { get; set; }
}


//单个标签
public class Tag
{
    public string ImageId { get; set; }
    public string TagId { get; set; }
}


//所有提交的图片
public class ImageRootObject
{
    public bool IsBatchSuccessful { get; set; }
    public List<SubmittedImage> Images { get; set; }
}


//单个提交的图片的信息
public class SubmittedImage
{
    public string SourceUrl { get; set; }
    public string Status { get; set; }
    public ImageObject Image { get; set; }
}

//单张图片属性
public class ImageObject
{
    public string Id { get; set; }
    public DateTime Created { get; set; }
    public int Width { get; set; }
    public int Height { get; set;}
    public string ImageUri { get; set; }
    public string ThumbnailUri { get; set; }
}

//Service迭代
public class Iteration
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool isDefault { get; set; }
    public string Status { get; set; }
    public string Created { get; set; }
    public string LastModified { get; set; }
    public string TrainedAt { get; set; }
    public string ProjectId { get; set; }
    public string Exportable { get; set; }
    public string DomainId { get; set; }
}

//获得的所有预测结果（包括bounding box）
public class AnalysisRootObject
{
    public string id { get; set; }
    public string project { get; set; }
    public string iteration { get; set; }
    public DateTime created { get; set; }
    public List<Prediction> predictions { get; set; }
}

//单个bounding box
public class BoundingBox
{
    public double left { get; set; }
    public double top { get; set; }
    public double width { get; set; }
    public double height { get; set; }
}

//单个预测结果
public class Prediction
{
    public double probability { get; set; }
    public string tagId { get; set; }
    public string tagName { get; set; }
    public BoundingBox boundingBox { get; set; }
}












































































