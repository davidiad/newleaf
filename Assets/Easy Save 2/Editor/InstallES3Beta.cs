using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InstallES3Beta : Editor 
{
	[MenuItem("Assets/Install or Update Easy Save 3 Beta", false, 1100)]
	public static void Install()
	{
		AssetDatabase.ImportPackage(Application.dataPath + "/Easy Save 2/Easy Save 3 Beta.unitypackage", false);
	}
}
