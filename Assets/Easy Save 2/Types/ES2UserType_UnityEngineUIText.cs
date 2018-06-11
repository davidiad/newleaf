using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ES2UserType_UnityEngineUIText : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.UI.Text data = (UnityEngine.UI.Text)obj;
		// Add your writer.Write calls here.
writer.Write(data.alignment);
writer.Write(data.horizontalOverflow);
writer.Write(data.verticalOverflow);
writer.Write(data.fontStyle);
writer.Write(data.hideFlags);

	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.UI.Text data = GetOrCreate<UnityEngine.UI.Text>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.UI.Text data = (UnityEngine.UI.Text)c;
		// Add your reader.Read calls here to read the data into the object.
data.alignment = reader.Read<UnityEngine.TextAnchor>();
data.horizontalOverflow = reader.Read<UnityEngine.HorizontalWrapMode>();
data.verticalOverflow = reader.Read<UnityEngine.VerticalWrapMode>();
data.fontStyle = reader.Read<UnityEngine.FontStyle>();
data.hideFlags = reader.Read<UnityEngine.HideFlags>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineUIText():base(typeof(UnityEngine.UI.Text)){}
}