namespace UndyneFight_Ex
{
	public struct Protected<T>
	{
		public bool Hacked { get; private set; }
		private T value;
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
		private int hash;
		public static implicit operator T(Protected<T> val) => val.Value;
	}
}