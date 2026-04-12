using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.GameMain;

namespace UndyneFight_Ex;
/// <summary>
/// The shader class
/// </summary>
public class Shader()
{
	/// <summary>
	/// Creates a shader using an existing <see cref="Effect"/>
	/// </summary>
	/// <param name="effect"></param>
	public Shader(Effect effect) : this() => this.effect = effect;
	/// <summary>
	/// Loads a shader in the given path
	/// </summary>
	/// <param name="path">The path of the shader</param>
	public Shader(string path) : this() => effect = GlobalResources.LoadContent<Effect>(path, Scene.Loader);
	private readonly Effect effect;
	private string effectName = "NormalDrawing";
	/// <summary>
	/// The name of the effect used within the shader
	/// </summary>
	public string EffectName { get => effectName; set { effectName = value; effect.CurrentTechnique = effect.Techniques[value]; } }
	/// <summary>
	/// The parameters of the shader
	/// </summary>
	public EffectParameterCollection Parameters => effect.Parameters;
	public Dictionary<string, Action<Effect>> PartEvents { private get; set; }
	/// <summary>
	/// The event to execute within the shader
	/// </summary>
	public Action<Effect> StableEvents { private get; set; }
	/// <summary>
	/// Applies the given texture to the shader
	/// </summary>
	/// <param name="tex">The texture to import to the shader</param>
	/// <param name="index">The index of the texture to import (Range should be [1, inf))</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RegisterTexture(Texture2D tex, int index) => RegisterTextures[index - 1] = tex;
	/// <summary>
	/// Sets multiple parameters to the shader
	/// </summary>
	/// <param name="vals">A KeyValuePair of the name of the parameter and the value</param>
	public void SetParameters(params KeyValuePair<string, object>[] vals)
	{
		foreach (KeyValuePair<string, object> kvp in vals)
		{
			//A table is all I can think of to automatically convert all available
			//SetValue types
			if (kvp.Value is float flt)
				effect.Parameters[kvp.Key].SetValue(flt);
			else if (kvp.Value is float[] fltarr)
				effect.Parameters[kvp.Key].SetValue(fltarr);
			else if (kvp.Value is int inte)
				effect.Parameters[kvp.Key].SetValue(inte);
			else if (kvp.Value is int[] intearr)
				effect.Parameters[kvp.Key].SetValue(intearr);
			else if (kvp.Value is bool bl)
				effect.Parameters[kvp.Key].SetValue(bl);
			else if (kvp.Value is Matrix mtx)
				effect.Parameters[kvp.Key].SetValue(mtx);
			else if (kvp.Value is Matrix[] mtxarr)
				effect.Parameters[kvp.Key].SetValue(mtxarr);
			else if (kvp.Value is Quaternion quat)
				effect.Parameters[kvp.Key].SetValue(quat);
			else if (kvp.Value is Texture tex)
				effect.Parameters[kvp.Key].SetValue(tex);
			else if (kvp.Value is Vector2 vec2)
				effect.Parameters[kvp.Key].SetValue(vec2);
			else if (kvp.Value is Vector2[] vec2arr)
				effect.Parameters[kvp.Key].SetValue(vec2arr);
			else if (kvp.Value is Vector3 vec3)
				effect.Parameters[kvp.Key].SetValue(vec3);
			else if (kvp.Value is Vector3[] vec3arr)
				effect.Parameters[kvp.Key].SetValue(vec3arr);
			else if (kvp.Value is Vector4 vec4)
				effect.Parameters[kvp.Key].SetValue(vec4);
			else if (kvp.Value is Vector4[] vec4arr)
				effect.Parameters[kvp.Key].SetValue(vec4arr);
		}
	}
	/// <summary>
	/// Sets a parameter of the shader
	/// </summary>
	/// <param name="val">The name and value to set</param>
	public void SetParameter(KeyValuePair<string, object> val) => SetParameters([val]);
	/// <summary>
	/// Updates the shader
	/// </summary>
	public void Update()
	{
		StableEvents?.Invoke(this);
		if (PartEvents?.TryGetValue(effectName, out Action<Effect> value) ?? false)
			value(effect);
	}
	/// <summary>
	/// Converts the shader into its effect
	/// </summary>
	/// <param name="shader">The shader to convert form</param>
	public static implicit operator Effect(Shader shader) => shader?.effect;
}