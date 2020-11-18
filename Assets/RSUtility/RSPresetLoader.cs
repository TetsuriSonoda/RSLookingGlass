using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Intel.RealSense;

public class RSPresetLoader : MonoBehaviour
{
	public RsDeviceInspector deviceInspector;
	public string fileName = "";
	// Start is called before the first frame update

	// Update is called once per frame
	void Update()
	{
		if (fileName != "")
		{
			var dev = deviceInspector.device;
			if(dev == null) { return; }
			if (dev.Info.Supports(CameraInfo.AdvancedMode))
			{
				var adv = dev.As<AdvancedDevice>();
				if (adv.AdvancedModeEnabled)
				{
#if UNITY_ANDROID && !UNITY_EDITOR
					var path = "jar:file://" + Application.dataPath + "!/assets/" + fileName;

					if (path.Length != 0)
					{
						WWW www = new WWW(path);
						while (!www.isDone) { }
						adv.JsonConfiguration = www.text;
						Debug.Log("Config uploaded");
					}
#else
					var path = Application.streamingAssetsPath + "/" + fileName;
					if (path.Length != 0)
					{
						adv.JsonConfiguration = File.ReadAllText(path);
						Debug.Log(path);
						Debug.Log("Config uploaded");
					}
#endif
				}
			}

			fileName = "";
		}
	}
}
