using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SceneOrganiser : MonoBehaviour
{
    public static SceneOrganiser Instance;

    //cursor attached to main camera
    internal GameObject cursor;

    public GameObject label;
    public GameObject box;
    //lastLabelPlaced盛放LabelPrefab
    internal Transform lastLabelPlaced;
    internal TextMesh lastLabelPlacedText;

    internal float probabilityThreshold = 0.75f;

    private GameObject quad;
    internal Renderer quadRenderer;

    private void Awake()
    {
        Instance = this;
     
        //添加组件即添加类class
        this.gameObject.AddComponent<ImageCapture>();
        this.gameObject.AddComponent<CustomVisionAnalyser>();
        this.gameObject.AddComponent<CustomVisionObjects>();

    }



    //在完成点击手势时，触发该方法。初始时，将quad放置在用户前3米位置处，标签文字初始为空。
    public void PlaceAnalysisLabel()
    {
        lastLabelPlaced = Instantiate(label.transform, cursor.transform.position, transform.rotation);
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
        lastLabelPlacedText.text = "";
        lastLabelPlaced.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        //创建可显示texture的quad
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        //该材质是透明的
        Material m = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        quadRenderer.material = m;

        float transparency = 0f;
        quadRenderer.material.color = new Color(1, 1, 1, transparency);

        //transform在这指Camera,将parent设置为camera,子对象会对齐父对象，则quad的位置和朝向都依赖于camera（也就是用户位置）
        quad.transform.parent = transform;
        quad.transform.rotation = transform.rotation;
        //将quad的位置设为用户前面一点
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
        //quad的scale实验得出
        quad.transform.localScale = new Vector3(3f, 1.65f, 1f);
        quad.transform.parent = null;

    }


    //1.设置标签文字为最高概率预测
    //2.调用CalculateBoundingBoxPosition方法，在场景中放置标签
    //3.运用Raycast，方向为Camera至BoundingBox的方向，与物理层产生碰撞，即可获得深度信息,调整标签深度
    //4.充值Capture过程，使用户可以捕获新的照片
    public void FinaliseLabel(AnalysisRootObject analysisRootObject)
    {
        if(analysisRootObject.predictions!=null)
        {
            lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();
            List<Prediction> sortedPredictions = new List<Prediction>();
            //OrderBy默认升序排序
            sortedPredictions = analysisRootObject.predictions.OrderBy(p => p.probability).ToList();
            Prediction bestPrediction = new Prediction();
            bestPrediction = sortedPredictions[sortedPredictions.Count - 1];

            if(bestPrediction.probability>probabilityThreshold)
            {
                quadRenderer = quad.GetComponent<Renderer>() as Renderer;
                //Bounds边界盒
                Bounds quadBounds = quadRenderer.bounds;

                lastLabelPlaced.transform.parent = quad.transform;
                //设定相对于父组件的相对位置。标签文字这时是在Quad上的
                //lastLabelPlaced的parent是quad,故localPosition是针对于quad来说的。localPosition为父子组件之间中心的距离
                lastLabelPlaced.transform.localPosition = CalculateBoundingBoxPosition(quadBounds, bestPrediction.boundingBox);
                lastLabelPlacedText.text = bestPrediction.tagName;



                /*
                //Cube半透明
                Debug.Log("Placing The Box.");
                box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float BoxTransparency = 0.5f;
                box.GetComponent<Renderer>().material = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
                box.GetComponent<Renderer>().material.color = new Color(1, 1, 1, BoxTransparency);
                box.transform.parent = quad.transform;
                box.transform.localPosition = CalculateBoundingBoxPosition(quadBounds, bestPrediction.boundingBox);

                //注：localScale相对于父物体缩放比例在（0，1）之间
                //貌似正确的Box大小
                box.transform.localScale = new Vector3((float)(bestPrediction.boundingBox.width/(2*quad.transform.localScale.x)),(float)(bestPrediction.boundingBox.height/(2*quad.transform.localScale.y)), 0.05f);
                Debug.LogFormat(@"The Local Scale X of Quad:{0}  The Local Scale Y of Quad:{1} ", quad.transform.localScale.x, quad.transform.localScale.y);
                Debug.Log("Firstly The Label Position: " + lastLabelPlaced.transform.position);
                Debug.Log("Firstly The Box Position: " + box.transform.position);
                */


                Debug.Log("Repositioning Label...");


                Vector3 headPosition = Camera.main.transform.position;
                RaycastHit objHitInfo;
                Vector3 objDirection = lastLabelPlaced.position;

                if(Physics.Raycast(headPosition,objDirection,out objHitInfo,30.0f,SpatialMapping.PhysicsRaycastMask))
                {
                    //设定世界坐标位置
                    lastLabelPlaced.position = objHitInfo.point;
                    /*
                    box.transform.position = objHitInfo.point;
                    Debug.Log("After Raycast The Label Position: " + lastLabelPlaced.position);
                    Debug.Log("After Raycast The Box Position: " + box.transform.position);
                    */
                }

            }

        }

        cursor.GetComponent<Renderer>().material.color = Color.green;

        ImageCapture.Instance.ResetImageCapture();
    }

   

    //放置标签的文字
    //依据分析所得到的Bounding Box预测，在真实环境中，在quad上标出TextTag
    //quad的Z坐标取决于拍摄时的深度
    private Vector3 CalculateBoundingBoxPosition(Bounds b, BoundingBox boundingBox)
    {

        Debug.Log($"BoundingBox: left {boundingBox.left}, top {boundingBox.top}, width {boundingBox.width}, height {boundingBox.height}");

        //像素图左上角为原点,y轴向下正向，故Top为y轴正向，BottomFromTop为距底部的距离。
        double centerFromLeft = boundingBox.left + (boundingBox.width / 2);
        double centerFromTop = boundingBox.top + (boundingBox.height / 2);
        Debug.Log($"Bounding Box: CenterFromLeft {centerFromLeft}, CenterFromTop {centerFromTop}");

        //注：Normalized方法不改变原向量，而是返回一个新向量，方向不变，长度为1。此处即体对角线长队为1。
        double quadWidth = b.size.normalized.x;
        double quadHeight = b.size.normalized.y;
        Debug.Log($"Quad Width {quadWidth}, Quad Height {quadHeight}");

       
        double normalisedPos_X = (quadWidth * centerFromLeft) - (quadWidth / 2);
        double normalisedPos_y = (quadHeight * centerFromTop) - (quadHeight / 2);

        return new Vector3((float)normalisedPos_X, (float)normalisedPos_y, 0);

    }





}
