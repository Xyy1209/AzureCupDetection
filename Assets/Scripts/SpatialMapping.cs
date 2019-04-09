using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

//该脚本进行碰撞检测

public class SpatialMapping : MonoBehaviour
{

    public static SpatialMapping Instance;

    //internal只有在同一程序集（包括dll）中才可被访问
    //静态成员变量，表示用于碰撞检测的层
    internal static int PhysicsRaycastMask;

    //用于碰撞的物理层
    internal int physicsLayer = 31;
    //创建和物理环境的碰撞
    private SpatialMappingCollider spatialMappingCollider;

    void Awake()
    {
        //单例
        Instance = this;
    }


    // Use this for initialization
    void Start ()
    {
        spatialMappingCollider = gameObject.GetComponent<SpatialMappingCollider>();
        //禁用流动更新，使物理环境更稳定
        spatialMappingCollider.freezeUpdates = false;
        spatialMappingCollider.surfaceParent = this.gameObject;
        //仅31物理layer发生碰撞
        spatialMappingCollider.layer = physicsLayer;

        //只检测physicsLayer
        PhysicsRaycastMask = 1 << physicsLayer;

        gameObject.SetActive(true);

	}
	
	
}
