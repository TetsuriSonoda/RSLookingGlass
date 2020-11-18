using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Intel.RealSense;
using System.Linq;

public class RSCalibrationManager : MonoBehaviour
{
	public RsFrameProvider Source;
	public RealsenseColorStreamActiveEvent OnColorCalibrationInit;
	public RealsenseDepthStreamActiveEvent OnDepthCalibrationInit;
	public RealsenseExtrinsicActiveEvent OnDepthToColorCalibrationInit;
	public Camera targetCamera;

	[Serializable]
	public class RealsenseColorStreamActiveEvent : UnityEvent<Intel.RealSense.Intrinsics> { }
	[Serializable]
	public class RealsenseDepthStreamActiveEvent : UnityEvent<Intel.RealSense.Intrinsics> { }
	[Serializable]
	public class RealsenseExtrinsicActiveEvent : UnityEvent<Intel.RealSense.Extrinsics> { }

	private Intel.RealSense.Intrinsics depthIntrinsics;
	private Intel.RealSense.Intrinsics colorIntrinsics;
	private Intel.RealSense.Extrinsics depthToColorExtrinsics;

	// Use this for initialization
	void Start()
	{
		Source.OnStart += OnStartStreaming;
	}

	private void OnStartStreaming(PipelineProfile obj)
	{
		//		var ds = obj.Streams.FirstOrDefault(s => s.Stream == Stream.Depth) as VideoStreamProfile;
		var ds = obj.GetStream<VideoStreamProfile>(Stream.Depth, -1);
		if (ds != null)
		{
			depthIntrinsics = ds.GetIntrinsics();
			OnDepthCalibrationInit.Invoke(depthIntrinsics);
		}

//		var cs = obj.Streams.FirstOrDefault(s => s.Stream == Stream.Color) as VideoStreamProfile;
		var cs = obj.GetStream<VideoStreamProfile>(Stream.Color, -1);
		if (cs != null)
		{
			colorIntrinsics = cs.GetIntrinsics();
			OnColorCalibrationInit.Invoke(colorIntrinsics);
		}

		if (ds != null && cs != null)
		{
			depthToColorExtrinsics = ds.GetExtrinsicsTo(cs);
			OnDepthToColorCalibrationInit.Invoke(depthToColorExtrinsics);
		}

		// Unity camera FOV alignment with color intrinsics
		if (targetCamera)
		{
			targetCamera.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan2(colorIntrinsics.height / 2.0f, colorIntrinsics.fy);
		}

	}
}