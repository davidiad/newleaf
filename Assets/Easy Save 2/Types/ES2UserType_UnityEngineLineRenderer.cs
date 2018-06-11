using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_UnityEngineLineRenderer : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		UnityEngine.LineRenderer data = (UnityEngine.LineRenderer)obj;
		// Add your writer.Write calls here.
		writer.Write(data.startWidth);
		writer.Write(data.endWidth);
		writer.Write(data.widthMultiplier);
		writer.Write(data.startColor);
		writer.Write(data.endColor);
		writer.Write(data.colorGradient);
		writer.Write(data.positionCount);
		writer.Write(data.useWorldSpace);
		writer.Write(data.loop);
		writer.Write(data.numCornerVertices);
		writer.Write(data.numCapVertices);
		writer.Write(data.textureMode);
		writer.Write(data.alignment);
		writer.Write(data.enabled);
		writer.Write(data.shadowCastingMode);
		writer.Write(data.receiveShadows);
		writer.Write(data.material);
		writer.Write(data.sharedMaterial);
		writer.Write(data.materials);
		writer.Write(data.sharedMaterials);
		writer.Write(data.lightmapIndex);
		writer.Write(data.realtimeLightmapIndex);
		writer.Write(data.lightmapScaleOffset);
		writer.Write(data.motionVectorGenerationMode);
		writer.Write(data.realtimeLightmapScaleOffset);
		writer.Write(data.lightProbeUsage);
		writer.Write(data.lightProbeProxyVolumeOverride);
		writer.Write(data.probeAnchor);
		writer.Write(data.reflectionProbeUsage);
		writer.Write(data.sortingLayerName);
		writer.Write(data.sortingLayerID);
		writer.Write(data.sortingOrder);
		writer.Write(data.lightmapScaleOffset);
		writer.Write(data.probeAnchor);
		writer.Write(data.shadowCastingMode);
		writer.Write(data.lightProbeUsage);
		Vector3[] positions = new Vector3[data.positionCount];
		writer.Write (data.GetPositions (positions));
	}
	
	public override object Read(ES2Reader reader)
	{
		UnityEngine.LineRenderer data = GetOrCreate<UnityEngine.LineRenderer>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		UnityEngine.LineRenderer data = (UnityEngine.LineRenderer)c;
		// Add your reader.Read calls here to read the data into the object.
		data.startWidth = reader.Read<System.Single>();
		data.endWidth = reader.Read<System.Single>();
		data.widthMultiplier = reader.Read<System.Single>();
		data.startColor = reader.Read<UnityEngine.Color>();
		data.endColor = reader.Read<UnityEngine.Color>();
		data.colorGradient = reader.Read<UnityEngine.Gradient>();
		data.positionCount = reader.Read<System.Int32>();
		data.useWorldSpace = reader.Read<System.Boolean>();
		data.loop = reader.Read<System.Boolean>();
		data.numCornerVertices = reader.Read<System.Int32>();
		data.numCapVertices = reader.Read<System.Int32>();
		data.textureMode = reader.Read<UnityEngine.LineTextureMode>();
		data.alignment = reader.Read<UnityEngine.LineAlignment>();
		data.enabled = reader.Read<System.Boolean>();
		data.shadowCastingMode = reader.Read<UnityEngine.Rendering.ShadowCastingMode>();
		data.receiveShadows = reader.Read<System.Boolean>();
		data.material = reader.Read<UnityEngine.Material>();
		data.sharedMaterial = reader.Read<UnityEngine.Material>();
		data.materials = reader.ReadArray<UnityEngine.Material>();
		data.sharedMaterials = reader.ReadArray<UnityEngine.Material>();
		data.lightmapIndex = reader.Read<System.Int32>();
		data.realtimeLightmapIndex = reader.Read<System.Int32>();
		data.lightmapScaleOffset = reader.Read<UnityEngine.Vector4>();
		data.motionVectorGenerationMode = reader.Read<UnityEngine.MotionVectorGenerationMode>();
		data.realtimeLightmapScaleOffset = reader.Read<UnityEngine.Vector4>();
		data.lightProbeUsage = reader.Read<UnityEngine.Rendering.LightProbeUsage>();
		data.lightProbeProxyVolumeOverride = reader.Read<UnityEngine.GameObject>();
		data.probeAnchor = reader.Read<UnityEngine.Transform>();
		data.reflectionProbeUsage = reader.Read<UnityEngine.Rendering.ReflectionProbeUsage>();
		data.sortingLayerName = reader.Read<System.String>();
		data.sortingLayerID = reader.Read<System.Int32>();
		data.sortingOrder = reader.Read<System.Int32>();
		data.lightmapScaleOffset = reader.Read<UnityEngine.Vector4>();
		data.probeAnchor = reader.Read<UnityEngine.Transform>();
		data.shadowCastingMode = reader.Read<UnityEngine.Rendering.ShadowCastingMode>();
		data.lightProbeUsage = reader.Read<UnityEngine.Rendering.LightProbeUsage>();
		data.SetPositions( reader.ReadArray<Vector3>() );
	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_UnityEngineLineRenderer():base(typeof(UnityEngine.LineRenderer)){}
}