namespace UndyneFight_Ex;
/// <summary>
/// Protected data
/// </summary>
/// <typeparam name="T"></typeparam>
public struct Protected<T>
{
	/// <summary>
	/// Whether the data was hacked
	/// </summary>
	public bool Hacked { get; private set; }
#if NET10_0_OR_GREATER
	/// <summary>
	/// The value of the protected data
	/// </summary>
	public T Value
	{
		get
		{
			if (field.GetHashCode() != hash)
				Hacked = true;
			return field;
		}
		set
		{
			field = value;
			hash = value.GetHashCode();
		}
	}
#else
	private T value;
	/// <summary>
	/// The value of the protected data
	/// </summary>
	public T Value
	{
		get
		{
			if (value.GetHashCode() != hash)
				Hacked = true;
			return value;
		}
		set
		{
			this.value = value;
			hash = value.GetHashCode();
		}
	}
#endif
	private int hash;
	/// <summary>
	/// Converts the value of the data to the desired type
	/// </summary>
	/// <param name="val">The value to convert</param>
	public static implicit operator T(Protected<T> val) => val.Value;
}