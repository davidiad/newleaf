using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_UnityEngineCanvasRenderer : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.CanvasRenderer data = (UnityEngine.CanvasRenderer)obj;
		// Add your writer.Write calls here.
writer.Write(data.hideFlags);

	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.CanvasRenderer data = GetOrCreate<UnityEngine.CanvasRenderer>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.CanvasRenderer data = (UnityEngine.CanvasRenderer)c;
		// Add your reader.Read calls here to read the data into the object.
data.hideFlags = reader.Read<UnityEngine.HideFlags>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineCanvasRenderer():base(typeof(UnityEngine.CanvasRenderer)){}
}