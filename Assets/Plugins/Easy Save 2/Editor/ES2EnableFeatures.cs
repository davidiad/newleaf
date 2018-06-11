using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public static class ES2EnableFeatures
{
	[MenuItem ("Assets/Easy Save 2/Enable or Update Playmaker Action...", false, 1000)]
	public static void EnableOrUpdatePlayMaker()
	{
		AssetDatabase.ImportPackage(Application.dataPath+"/Easy Save 2/Disabled/ES2Playmaker.unitypackage", false);
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Easy Save 2 PlayMaker Action Enabled",
		                            "Easy Save 2 PlayMaker Action has been Enabled and Updated.", "Ok");
	}
}

