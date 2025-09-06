using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UndyneFight_Ex.Entities;

namespace UndyneFight_Ex
{
	/// <summary>
	/// Carrier of all entities, the FPS of every scene is 125 fps
	/// </summary>
	public abstract class Scene : Entity
	{
		/// <summary>
		/// The drawing settings
		/// </summary>
		public class DrawingSettings
		{
			/// <summary>
			/// The color of the background
			/// </summary>
			public Color backGroundColor = Color.Black;
			/// <summary>
			/// The theme color of the chart
			/// </summary>
			public Color themeColor = Color.White;
			/// <summary>
			/// The color of the UI
			/// </summary>
			public Color UIColor = Color.White;
			internal float screenScale = 1f;
			internal float sceneOutScale = 3.0f;
			internal float masterAlpha = 1.0f;
			/// <summary>
			/// The default width of the scene
			/// </summary>
			public float defaultWidth = 640f;
			internal Vector2 screenDelta;
			internal Vector2 shakings = Vector2.Zero;
			internal float screenAngle = 0.0f;
			internal Dictionary<string, Surface> surfaces = [];
			internal Vector4 Extending
			{
				get => extending;
				set
				{
					extending = value;
					GameMain.ResetRendering();
				}
			}
			private Vector4 extending;

			internal float SurfaceScale => defaultWidth / 640f;

			public DrawingSettings()
			{
				surfaces.Add("normal", Surface.Normal);
				surfaces.Add("hidden", Surface.Hidden);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void PrepareLoader(IServiceProvider serviceProvider) => Loader = new(serviceProvider) { RootDirectory = "Content" };
		/// <summary>
		/// The current drawing settings of this scene
		/// </summary>
		public DrawingSettings CurrentDrawingSettings { get; set; } = new();
		/// <summary>
		/// The rendering manager of the background
		/// </summary>
		public RenderingManager BackgroundRendering { get; internal set; } = new();
		/// <summary>
		/// The rendering manager for the scene
		/// </summary>
		public RenderingManager SceneRendering { get; internal set; } = new();

		private readonly List<GameObject> buffer = [];
		/// <summary>
		/// The list of <see cref="GameObject"/>s in the scene
		/// </summary>
		public List<GameObject> Objects { get; } = [];
		/// <summary>
		/// The loader of the current scene
		/// </summary>
		public static ContentManager Loader { get; private set; }
		internal Selector BaseSelector { get; set; }

		internal float stopTime = 0;
		/// <summary>
		/// Creates a game object
		/// </summary>
		/// <param name="t">The object to create</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InstanceCreate(GameObject t) => buffer.Add(t);
		public override void Draw()
		{
			if (stopTime > 0.01f)
				return;
		}
		private Dictionary<string, List<GameEventArgs>> GameEvents { get; set; } = [];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Broadcast(GameEventArgs gameEventArgs)
		{
			if (!GameEvents.TryGetValue(gameEventArgs.ActionName, out List<GameEventArgs> value))
				GameEvents.Add(gameEventArgs.ActionName, value = []);

			value.Add(gameEventArgs);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<GameEventArgs> DetectEvent(string ActionName) => GameEvents.TryGetValue(ActionName, out List<GameEventArgs> value) ? value : null;
		/// <summary>
		/// Gets the list of global objects (<see cref="GameObject.CrossScene"/> is true)
		/// </summary>
		/// <returns>The list of global objects</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<GameObject> GlobalObjects() => Objects.FindAll(s => s.CrossScene);

		public override void Update()
		{
			if (!BeingUpdated)
			{
				BeingUpdated = true;
				Fight.Functions.ScreenDrawing.Reset();
				Start();
			}
			buffer.ForEach(Objects.Add);
			buffer.Clear();
			_ = Objects.RemoveAll(s => s.Disposed);
			Objects.ForEach(s => s.TreeUpdate());
			foreach (KeyValuePair<string, List<GameEventArgs>> v in GameEvents)
			{
				_ = v.Value.RemoveAll(s => s.Disposed);
			}

			if (stopTime > 0)
				stopTime -= 0.5f;
			else if (stopTime < 0f)
			{
				stopTime = 0;
				Fight.Functions.PlaySound(FightResources.Sounds.change);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SceneUpdate() => Update();

		protected Scene(GameObject startObj) => Objects.Add(startObj);
		protected Scene() { }

		public override void Dispose()
		{
			Objects.ForEach(s => { if (!s.CrossScene) s.Dispose(); });
			buffer.Clear();
			base.Dispose();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RenderTarget2D DrawAll(RenderTarget2D mission) => SceneRendering.Draw(mission);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void UpdateRendering()
		{
			SceneRendering.UpdateAll();
			BackgroundRendering.UpdateAll();
		}
	}
}