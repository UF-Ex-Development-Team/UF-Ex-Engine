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
	public Shader(string path) : this() => effect = GlobalData.LoadContent<Effect>(path, Scene.Loader);
	private readonly Effect effect;
#if NET10_0_OR_GREATER
	/// <summary>
	/// The name of the effect used within the shader
	/// </summary>
	public string EffectName
	{
		get;
		set
		{
			field = value;
			effect.CurrentTechnique = effect.Techniques[value];
		}
	} = "NormalDrawing";
#else
	/// <summary>
	/// The name of the effect used within the shader
	/// </summary>
	public string EffectName
	{
		get => _effectName;
		set
		{
			_effectName = value;
			effect.CurrentTechnique = effect.Techniques[value];
		}
	}
	private string _effectName = "NormalDrawing";
#endif
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
		foreach ((string key, object value) in vals)
		{
			//A table is all I can think of to automatically convert all available
			//SetValue types
			if (value is float flt)
				effect.Parameters[key].SetValue(flt);
			else if (value is float[] fltarr)
				effect.Parameters[key].SetValue(fltarr);
			else if (value is int inte)
				effect.Parameters[key].SetValue(inte);
			else if (value is int[] intearr)
				effect.Parameters[key].SetValue(intearr);
			else if (value is bool bl)
				effect.Parameters[key].SetValue(bl);
			else if (value is Matrix mtx)
				effect.Parameters[key].SetValue(mtx);
			else if (value is Matrix[] mtxarr)
				effect.Parameters[key].SetValue(mtxarr);
			else if (value is Quaternion quat)
				effect.Parameters[key].SetValue(quat);
			else if (value is Texture tex)
				effect.Parameters[key].SetValue(tex);
			else if (value is Vector2 vec2)
				effect.Parameters[key].SetValue(vec2);
			else if (value is Vector2[] vec2arr)
				effect.Parameters[key].SetValue(vec2arr);
			else if (value is Vector3 vec3)
				effect.Parameters[key].SetValue(vec3);
			else if (value is Vector3[] vec3arr)
				effect.Parameters[key].SetValue(vec3arr);
			else if (value is Vector4 vec4)
				effect.Parameters[key].SetValue(vec4);
			else if (value is Vector4[] vec4arr)
				effect.Parameters[key].SetValue(vec4arr);
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
		if (PartEvents?.TryGetValue(EffectName, out Action<Effect> value) ?? false)
			value(effect);
	}
	/// <summary>
	/// Converts the shader into its effect
	/// </summary>
	/// <param name="shader">The shader to convert form</param>
	public static implicit operator Effect(Shader shader) => shader?.effect;
}