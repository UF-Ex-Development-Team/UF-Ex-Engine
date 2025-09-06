namespace UndyneFight_Ex.Entities
{
	/// <summary>
	/// Interface for selectable menu items (Legacy)
	/// </summary>
	public interface ISelectAble
	{
		/// <summary>
		/// Set the item to be selected
		/// </summary>
		void Selected();
		/// <summary>
		/// Set the item to be de-selected
		/// </summary>
		void DeSelected();
		/// <summary>
		/// Event to invoke when being selected
		/// </summary>
		void SelectionEvent();
	}

	internal class OKCancelSelector : Selector
	{
		public OKCancelSelector() : base()
		{
			AutoDispose = false;
			IsCancelAvailable = false;
		}

		public override void Update()
		{
			if (playTick == 3)
			{
				PushSelection(new OK(this));
				PushSelection(new Cancel(this));
			}
			base.Update();
		}

		public event Func<bool> OKAction;

		private class OK : TextSelection
		{
			public OK(OKCancelSelector selector) : base("OK", new vec2(320, 330))
			{
				Size = 1.0f;
				this.selector = selector;
			}

			private readonly OKCancelSelector selector;
			private bool? correctOperation = true;

			public override void SelectionEvent()
			{
				correctOperation = selector.OKAction?.Invoke();
				if (correctOperation == true)
				{
					selector.Dispose();
					base.SelectionEvent();
				}
			}
		}

		private class Cancel : TextSelection
		{
			public Cancel(OKCancelSelector selector) : base("Cancel", new vec2(320, 380))
			{
				Size = 1.0f;
				this.selector = selector;
			}

			private readonly OKCancelSelector selector;

			public override void SelectionEvent()
			{
				Back();
				base.SelectionEvent();
			}
		}
	}

	internal class Selector : Entity
	{
		public Selector() : this(true) => UpdateIn120 = true;
		public Selector(bool enableMemory)
		{
			EnabledMemory = enableMemory;
			if (EnabledMemory)
			{
				Last = current;
				current = this;
			}
			UpdateIn120 = true;
		}

		protected bool EnabledMemory { get; set; } = true;

		private static Selector current
		{
			get => GameStates.CurrentScene.BaseSelector;
			set => GameStates.CurrentScene.BaseSelector = value;
		}
		protected Selector Last { get; set; }

		/// <summary>
		/// 将选择设置成0项。
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetSelect() => isReseted = true;

		protected List<ISelectAble> Selections { get; private set; } = [];
		public int currentSelect = 0;
		public int SelectionCount => Selections.Count;
		private int lastSelect = -1;
		private bool isReseted = false;
		/// <summary>
		/// 是否在选择选项之后自动关闭
		/// </summary>
		protected bool AutoDispose { get; set; } = true;
		protected int playTick = 0;

		/// <summary>
		/// Sets whether the player can press X to quit
		/// </summary>
		public bool IsCancelAvailable { get; set; } = true;

		public delegate void ChangeSelect();

		public event ChangeSelect SelectChanger;
		public event Action SelectChanged;
		public event Action Selected;

		public override void Draw() { }

		public override void Update()
		{
			if (Selections.Count == 0)
				goto A;
			if (isReseted)
			{
				if (Selections[0] is not null)
				{
					if (lastSelect != -1)
						Selections[lastSelect].DeSelected();
					Selections[0].Selected();
					currentSelect = lastSelect = 0;
				}
				isReseted = false;
			}
			playTick++;
			SelectChanger();
			if (lastSelect != currentSelect)
			{
				SelectChanged?.Invoke();
				ISelectAble selection = Selections[currentSelect];
				selection.Selected();
				if (lastSelect >= 0)
				{
					Selections[lastSelect].DeSelected();
				}
				lastSelect = currentSelect;
			}
		A:
			if (SelectionCount > 0 && playTick >= 2 && GameStates.IsKeyPressed120f(InputIdentity.Confirm))
			{
				if (AutoDispose)
					Dispose();
				Selections[lastSelect].SelectionEvent();
				Selected?.Invoke();
			}
			else if (IsCancelAvailable && GameStates.IsKeyPressed120f(InputIdentity.Cancel))
			{
				Back();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PushSelection(ISelectAble Selection)
		{
			if (Selection is GameObject)
				AddChild(Selection as GameObject);
			Selections.Add(Selection);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void InstanceSelect(int p)
		{
			Selections[p].SelectionEvent();
			Dispose();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]

		public override void Dispose() => base.Dispose();

		/// <summary>
		/// Return to the last selection screen
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Back()
		{
			if (current == null)
				return;
			Selector last = current.Last;
			if (last != null)
			{
				GameStates.InstanceCreate(last);
				current.Dispose();
				last.Reverse();
				current = last;
			}
			else
			{
				current = null;
				GameStates.ResetScene(new GameMenuScene()); //already is the root 
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Reverse()
		{
			Selections.ForEach(s =>
			{
				if (s is GameObject)
					AddChild(s as GameObject);
			});
			base.Reverse();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BackToRoot()
		{
			Selector last = current.Last;
			if (last == null)
				return; //already on the root.
			while (last.Last != null)
			{
				last = last.Last; //recursion to the root.
			}
			last.Reverse();
			current.Dispose();
			current = last;
			GameStates.InstanceCreate(last);
		}
	}
	/// <summary>
	/// A text button you can choose
	/// </summary>
	public class TextSelection(string texts, vec2 centre) : Entity, ISelectAble
	{
		private class ShinyTextEffect(TextSelection s) : Entity
		{
			private readonly string texts = s.texts;
			private float alpha = s.alpha, size = s.size * s.currentSize;
			private col showingColor = col.Lerp(s.showingColor, col.Gold, 0.5f);
			public override void Draw() => FightResources.Font.NormalFont.CentreDraw(texts, s.Centre, showingColor * alpha, size, 0.9f);

			public override void Update()
			{
				collidingBox.Y -= 0.1f + alpha * 0.4f;
				alpha *= 0.9f;
				alpha -= 0.03f;
				size += ((2 - alpha) / 40f + 0.04f) / 1.6f;
				if (alpha < 0)
					Dispose();
			}
		}

		protected string texts = texts;
		public string subText { private get; set; }
		protected float alpha = 0.0f;
		private float currentSize = 1.0f, maxSize = 1.35f;
		private const float sizeChangeSpeed = 0.18f;
		private bool isSelected;

		public float MaxSize
		{
			set => maxSize = value;
		}
		public float Size
		{
			set => size = value;
		}
		public col TextColor
		{
			set => showingColor = value;
			get => showingColor;
		}
		private float size = 0.8f;
		protected float GetSize => size * currentSize;
		private col showingColor = col.White;

		private Action action;
		public Action SetSelectionAction
		{
			set => action = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DeSelected() => isSelected = false;

		public override void Draw()
		{
			string FinalText = texts;
			if (!string.IsNullOrEmpty(subText))
				FinalText += ":" + subText;
			FightResources.Font.NormalFont.CentreDraw(FinalText, Centre = centre, showingColor * alpha, currentSize * size, 0.9f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Selected() => isSelected = true;

		public override void Update()
		{
			if (alpha < 1f)
				alpha += 0.025f;
			else
				alpha = 1f;
			currentSize = isSelected
				? currentSize * (1 - sizeChangeSpeed) + maxSize * sizeChangeSpeed
				: currentSize * (1 - sizeChangeSpeed) + sizeChangeSpeed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void SelectionEvent()
		{
			action?.Invoke();
			GameStates.InstanceCreate(new ShinyTextEffect(this));
		}
	}
}