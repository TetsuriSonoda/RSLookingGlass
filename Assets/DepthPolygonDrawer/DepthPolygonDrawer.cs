using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DepthPolygonDrawer : MonoBehaviour
{
	// public params
	public GameObject ObjectToDrawOn = null;
    public int pointWidth = 848 / 2;
    public int pointHeight = 480 / 2;

	// for reverse polygon
    public bool isReverseQuad = false;
	// each polygon size. 1 is normal. n>1 will have overlap each other.
	public Vector2 polygonSize = new Vector2(1.0f, 1.0f);

	// components
	private Renderer _renderer;
	private Mesh	 _mesh;

	private bool isInited = false;

	// pointcloud
	private Vector3[] pointCloud;
	private int[] pointIndices;
	private List<Vector2> pointUVS;

	// textures and intrinsics set by callback
	private Intel.RealSense.Intrinsics depthIntrinsics;
	private Texture colorTexture;
	private Texture depthTexture;

	// Use this for initialization
	void Start()
    {
        _mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        isInited = false;

        //Creating new object		
        if (ObjectToDrawOn == null)
        {
            ObjectToDrawOn = this.gameObject;
        }

		_renderer = ObjectToDrawOn.GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update()
    {
        if (!isInited)
        {
			// Init polygon (vertices, uv/uv2, indices)
			Vector3[] vertices = new Vector3[pointWidth * pointHeight * 4];   // 
			Vector2[] uvs = new Vector2[pointWidth * pointHeight * 4];        // for UV
			Vector2[] uvs2 = new Vector2[pointWidth * pointHeight * 4];       // for Vertex index
			int[] indices = new int[pointWidth * pointHeight * 4];

			int indexCounter = 0;

			// input vertices and uvs
			if (isReverseQuad)
			{
				// input data for polygon 
				for (int y = 0; y < pointHeight; y++)
				{
					for (int x = 0; x < pointWidth; x++)
					{
						// left down
						vertices[(pointWidth * y + x) * 4].x = -0.5f + (float)(x - 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4].y = -0.5f + (float)(y - 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4].z = 1.0f;
						uvs[(pointWidth * y + x) * 4].x = (x - polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4].y = (y - polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4].x = 0.1f;
						uvs2[(pointWidth * y + x) * 4].y = 0.1f;
						indices[indexCounter] = indexCounter++;

						// left up
						vertices[(pointWidth * y + x) * 4 + 1].x = -0.5f + (float)(x - 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 1].y = -0.5f + (float)(y + 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 1].z = 1.0f;
						uvs[(pointWidth * y + x) * 4 + 1].x = (x - polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 1].y = (y + polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 1].x = 0.2f;
						uvs2[(pointWidth * y + x) * 4 + 1].y = 0.2f;
						indices[indexCounter] = indexCounter++;

						// right up
						vertices[(pointWidth * y + x) * 4 + 2].x = -0.5f + (float)(x + 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 2].y = -0.5f + (float)(y + 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 2].z = 1.0f;
						uvs[(pointWidth * y + x) * 4 + 2].x = (x + polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 2].y = (y + polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 2].x = 0.3f;
						uvs2[(pointWidth * y + x) * 4 + 2].y = 0.3f;
						indices[indexCounter] = indexCounter++;

						// right down
						vertices[(pointWidth * y + x) * 4 + 3].x = -0.5f + (float)(x + 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 3].y = -0.5f + (float)(y - 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 3].z = 1.0f;
						uvs[(pointWidth * y + x) * 4 + 3].x = (x + polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 3].y = (y - polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 3].x = 0.4f;
						uvs2[(pointWidth * y + x) * 4 + 3].y = 0.4f;
						indices[indexCounter] = indexCounter++;
					}
				}
			}
			else
			{
				for (int y = 0; y < pointHeight; y++)
				{
					for (int x = 0; x < pointWidth; x++)
					{
						// left down
						vertices[(pointWidth * y + x) * 4].x = -0.5f + (float)(x - 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4].y = -0.5f + (float)(y - 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4].z = 0.0f;
						uvs[(pointWidth * y + x) * 4].x = (x - polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4].y = (y - polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4].x = 0.1f;
						uvs2[(pointWidth * y + x) * 4].y = 0.1f;
						indices[indexCounter] = indexCounter++;
						// right down
						vertices[(pointWidth * y + x) * 4 + 1].x = -0.5f + (float)(x + 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 1].y = -0.5f + (float)(y - 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 1].z = 0.0f;
						uvs[(pointWidth * y + x) * 4 + 1].x = (x + polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 1].y = (y - polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 1].x = 0.4f;
						uvs2[(pointWidth * y + x) * 4 + 1].y = 0.4f;
						indices[indexCounter] = indexCounter++;
						// right up
						vertices[(pointWidth * y + x) * 4 + 2].x = -0.5f + (float)(x + 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 2].y = -0.5f + (float)(y + 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 2].z = 0.0f;
						uvs[(pointWidth * y + x) * 4 + 2].x = (x + polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 2].y = (y + polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 2].x = 0.3f;
						uvs2[(pointWidth * y + x) * 4 + 2].y = 0.3f;
						indices[indexCounter] = indexCounter++;
						// left up
						vertices[(pointWidth * y + x) * 4 + 3].x = -0.5f + (float)(x - 0.5f) / (float)pointWidth;
						vertices[(pointWidth * y + x) * 4 + 3].y = -0.5f + (float)(y + 0.5f) / (float)pointHeight;
						vertices[(pointWidth * y + x) * 4 + 3].z = 0.0f;
						uvs[(pointWidth * y + x) * 4 + 3].x = (x - polygonSize.x / 2.0f) / ((float)pointWidth);
						uvs[(pointWidth * y + x) * 4 + 3].y = (y + polygonSize.y / 2.0f) / ((float)pointHeight);
						uvs2[(pointWidth * y + x) * 4 + 3].x = 0.2f;
						uvs2[(pointWidth * y + x) * 4 + 3].y = 0.2f;
						indices[indexCounter] = indexCounter++;
					}
				}
			}

			// update mesh
			_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			_mesh.Clear();
			_mesh.vertices = vertices;
			_mesh.uv = uvs;
			_mesh.uv2 = uvs2;
			_mesh.SetIndices(indices, MeshTopology.Quads, 0);
			isInited = true;
		}

		if (!colorTexture || !depthTexture) { return; }

		// Set params to the shader
		_renderer.material.SetMatrix("_UNITY_MATRIX_M", transform.localToWorldMatrix);
		_renderer.material.SetFloat("_FX", depthIntrinsics.fx);       // 1.896f
		_renderer.material.SetFloat("_FY", depthIntrinsics.fy);        // 1.533f
		_renderer.material.SetFloat("_PPX", depthIntrinsics.ppx);       // 1.896f
		_renderer.material.SetFloat("_PPY", depthIntrinsics.ppy);        // 1.533f
		_renderer.material.SetFloat("_WindowX", polygonSize.x / pointWidth);
		_renderer.material.SetFloat("_WindowY", polygonSize.y / pointHeight);
//		_renderer.material.SetFloat("_ScaleBias", realsense_manager.depth_scale);
	}

	// Callback for calibration params
	public void OnColorCalibrationInit(Intel.RealSense.Intrinsics intrinsic)
	{
		depthIntrinsics = intrinsic;
		depthIntrinsics.fx = intrinsic.fx / intrinsic.width;
		depthIntrinsics.fy = intrinsic.fy / intrinsic.height;
		depthIntrinsics.ppx = intrinsic.ppx / intrinsic.width;
		depthIntrinsics.ppy = intrinsic.ppy / intrinsic.height;
	}

	// Callback for depth texture
	public void OnDepthTextureReady(Texture texture)
	{
		depthTexture = texture;
		_renderer.material.SetTexture("_DepthTex", depthTexture);
	}

	// Callback for color texture
	public void OnColorTextureReady(Texture texture)
	{
		colorTexture = texture;
		_renderer.material.SetTexture("_SpriteTex", colorTexture);
	}

}
