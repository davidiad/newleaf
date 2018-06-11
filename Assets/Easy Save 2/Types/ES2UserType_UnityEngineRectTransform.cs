using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_UnityEngineRectTransform : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.RectTransform data = (UnityEngine.RectTransform)obj;
		// Add your writer.Write calls here.
writer.Write(data.hideFlags);

	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.RectTransform data = GetOrCreate<UnityEngine.RectTransform>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.RectTransform data = (UnityEngine.RectTransform)c;
		// Add your reader.Read calls here to read the data into the object.
data.hideFlags = reader.Read<UnityEngine.HideFlags>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineRectTransform():base(typeof(UnityEngine.RectTransform)){}
}