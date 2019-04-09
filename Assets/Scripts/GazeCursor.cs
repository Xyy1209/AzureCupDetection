using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeCursor : MonoBehaviour {

    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start ()
    {
        //网格渲染器，用来渲染Cursor
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        SceneOrganiser.Instance.cursor = this.gameObject;
        this.gameObject.GetComponent<Renderer>().material.color = Color.green;

        this.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 headPosition = Camera.main.transform.position;
        Vector3 gazeDirection = Camera.main.transform.forward;

        RaycastHit gazeHitInfo;
        if(Physics.Raycast(headPosition,gazeDirection,out gazeHitInfo,30.0f,SpatialMapping.PhysicsRaycastMask))
        {
            meshRenderer.enabled=true;
            this.gameObject.transform.position = gazeHitInfo.point;
            this.gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, gazeHitInfo.normal);
        }
        else
        {
            meshRenderer.enabled = false;
        }
		
	}

}
