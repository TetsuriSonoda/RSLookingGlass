using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DepthPolygonDrawer : MonoBehaviour
{
    private Mesh _mesh;
    private bool is_inited = false;
    private int[] point_indices;
    private List<Vector2> point_uvs;

	//	public RSCaptureManager realsense_manager;

	public GameObject ObjectToDrawOn = null;
    public int point_width = 848 / 2;
    public int point_height = 480 / 4;

	public float begin_v = 0.0f;
    public float end_v = 1.0f;
    public bool is_reverse_quad = false;
	public Vector2 polygon_size = new Vector2(1.0f, 1.0f);

	private Texture color_texture;
	private Texture depth_texture;

    private Vector3[] point_cloud;
	private Intel.RealSense.Intrinsics depth_intrinsics;
	private Renderer _renderer;

	// Use this for initialization
	void Start()
    {
        _mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        is_inited = false;

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
        if (!is_inited)
        {

#if false
            // initialize point cloud
            int num_vertices = point_width * point_height;

            // initialize point cloud
            point_cloud = new Vector3[point_width * point_height];

            for (int y = 0; y < point_height; y++)
            {
                for (int x = 0; x < point_width; x++)
                {
                    float depth_value = 1.0f;
					float bias = 1280 / (float)point_width;
                    point_cloud[point_width * y + x].x = (point_width/2 - x) * depth_value * 0.0007f * bias;
                    point_cloud[point_width * y + x].y = (point_height/2 - y) * depth_value * 0.001f * bias;
                    if(x < point_width / 2)
                    {
                        point_cloud[point_width * y + x].z = depth_value;
                    }
                    else
                    {
						point_cloud[point_width * y + x].z = depth_value / 2;
                    }
                }
            }

            point_indices = new int[num_vertices];
            for (int i = 0; i < num_vertices; i++)
            {
                point_indices[i] = i++;
            }

            point_uvs = new List<Vector2>();
            for (int y = 0; y < point_height; y++)
            {
                for (int x = 0; x < point_width; x++)
                {
                    point_uvs.Add(new Vector2((float)x / (point_width - 1),
                                             ((float)y / (point_height - 1)) * end_v + begin_v));
                }
            }

		is_inited = true;
		_renderer.enabled = true;

		_mesh.Clear();
		_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		_mesh.vertices = point_cloud;
		_mesh.SetIndices(point_indices, MeshTopology.Points, 0);
		_mesh.SetUVs(0, point_uvs);

#else
		Vector3[] vertices = new Vector3[point_width * point_height * 4];   // 
		Vector2[] uvs = new Vector2[point_width * point_height * 4];        // for UV
		Vector2[] uvs2 = new Vector2[point_width * point_height * 4];       // for Vertex index
		int[] indices = new int[point_width * point_height * 4];

		int index_counter = 0;

		// input vertices and uvs
		if (is_reverse_quad)
		{
			for (int y = 0; y < point_height; y++)
			{
				for (int x = 0; x < point_width; x++)
				{
					// left down
					vertices[(point_width * y + x) * 4].x = -0.5f + (float)(x - 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4].y = -0.5f + (float)(y - 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4].z = 1.0f;
					uvs[(point_width * y + x) * 4].x = (x - polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4].y = (y - polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4].x = 0.1f;
					uvs2[(point_width * y + x) * 4].y = 0.1f;
					indices[index_counter] = index_counter++;

					// left up
					vertices[(point_width * y + x) * 4 + 1].x = -0.5f + (float)(x - 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 1].y = -0.5f + (float)(y + 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 1].z = 1.0f;
					uvs[(point_width * y + x) * 4 + 1].x = (x - polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 1].y = (y + polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 1].x = 0.2f;
					uvs2[(point_width * y + x) * 4 + 1].y = 0.2f;
					indices[index_counter] = index_counter++;

					// right up
					vertices[(point_width * y + x) * 4 + 2].x = -0.5f + (float)(x + 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 2].y = -0.5f + (float)(y + 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 2].z = 1.0f;
					uvs[(point_width * y + x) * 4 + 2].x = (x + polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 2].y = (y + polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 2].x = 0.3f;
					uvs2[(point_width * y + x) * 4 + 2].y = 0.3f;
					indices[index_counter] = index_counter++;

					// right down
					vertices[(point_width * y + x) * 4 + 3].x = -0.5f + (float)(x + 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 3].y = -0.5f + (float)(y - 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 3].z = 1.0f;
					uvs[(point_width * y + x) * 4 + 3].x = (x + polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 3].y = (y - polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 3].x = 0.4f;
					uvs2[(point_width * y + x) * 4 + 3].y = 0.4f;
					indices[index_counter] = index_counter++;
				}
			}
		}
		else
		{
			for (int y = 0; y < point_height; y++)
			{
				for (int x = 0; x < point_width; x++)
				{
					// left down
					vertices[(point_width * y + x) * 4].x = -0.5f + (float)(x - 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4].y = -0.5f + (float)(y - 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4].z = 0.0f;
					uvs[(point_width * y + x) * 4].x = (x - polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4].y = (y - polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4].x = 0.1f;
					uvs2[(point_width * y + x) * 4].y = 0.1f;
					indices[index_counter] = index_counter++;
					// right down
					vertices[(point_width * y + x) * 4 + 1].x = -0.5f + (float)(x + 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 1].y = -0.5f + (float)(y - 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 1].z = 0.0f;
					uvs[(point_width * y + x) * 4 + 1].x = (x + polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 1].y = (y - polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 1].x = 0.4f;
					uvs2[(point_width * y + x) * 4 + 1].y = 0.4f;
					indices[index_counter] = index_counter++;
					// right up
					vertices[(point_width * y + x) * 4 + 2].x = -0.5f + (float)(x + 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 2].y = -0.5f + (float)(y + 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 2].z = 0.0f;
					uvs[(point_width * y + x) * 4 + 2].x = (x + polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 2].y = (y + polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 2].x = 0.3f;
					uvs2[(point_width * y + x) * 4 + 2].y = 0.3f;
					indices[index_counter] = index_counter++;
					// left up
					vertices[(point_width * y + x) * 4 + 3].x = -0.5f + (float)(x - 0.5f) / (float)point_width;
					vertices[(point_width * y + x) * 4 + 3].y = -0.5f + (float)(y + 0.5f) / (float)point_height;
					vertices[(point_width * y + x) * 4 + 3].z = 0.0f;
					uvs[(point_width * y + x) * 4 + 3].x = (x - polygon_size.x / 2.0f) / ((float)point_width);
					uvs[(point_width * y + x) * 4 + 3].y = (y + polygon_size.y / 2.0f) / ((float)point_height);
					uvs2[(point_width * y + x) * 4 + 3].x = 0.2f;
					uvs2[(point_width * y + x) * 4 + 3].y = 0.2f;
					indices[index_counter] = index_counter++;
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
		is_inited = true;
#endif

		}

		if (color_texture && depth_texture)
		{
//			_renderer.material.SetTexture("_SpriteTex", color_texture);
//			_renderer.material.SetTexture("_DepthTex", depth_texture);
        }
		else{	return; }

		_renderer.material.SetMatrix("_UNITY_MATRIX_M", transform.localToWorldMatrix);
		_renderer.material.SetFloat("_FX", depth_intrinsics.fx);       // 1.896f
		_renderer.material.SetFloat("_FY", depth_intrinsics.fy);        // 1.533f
		_renderer.material.SetFloat("_PPX", depth_intrinsics.ppx);       // 1.896f
		_renderer.material.SetFloat("_PPY", depth_intrinsics.ppy);        // 1.533f
		_renderer.material.SetFloat("_WindowX", polygon_size.x / point_width);
		_renderer.material.SetFloat("_WindowY", polygon_size.y / point_height);

//    _renderer.material.SetFloat("_ScaleBias", realsense_manager.depth_scale);
	}

	public void OnColorCalibrationInit(Intel.RealSense.Intrinsics intrinsic)
	{
		depth_intrinsics = intrinsic;
		depth_intrinsics.fx = intrinsic.fx / intrinsic.width;
		depth_intrinsics.fy = intrinsic.fy / intrinsic.height;
		depth_intrinsics.ppx = intrinsic.ppx / intrinsic.width;
		depth_intrinsics.ppy = intrinsic.ppy / intrinsic.height;
	}

	public void OnDepthTextureReady(Texture texture)
	{
		depth_texture = texture;
		_renderer.material.SetTexture("_DepthTex", depth_texture);
		//		Debug.Log("DepthPolygonDrawer:Depth Texture Set");
	}

	public void OnColorTextureReady(Texture texture)
	{
		color_texture = texture;
		_renderer.material.SetTexture("_SpriteTex", color_texture);
		//		Debug.Log("DepthPolygonDrawer:Color Texture Set");
	}

}
