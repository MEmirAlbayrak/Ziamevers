using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
#else
using Thread = System.Threading.Thread;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/Pathfinder")]

public class AstarPath : VersionedMonoBehaviour {
	public static readonly System.Version Version = new System.Version(4, 2, 15);

	public enum AstarDistribution { WebsiteDownload, AssetStore, PackageManager };

	public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;

	
	public static readonly string Branch = "master";

	
	[System.Obsolete]
	public System.Type[] graphTypes {
		get {
			return data.graphTypes;
		}
	}

	[UnityEngine.Serialization.FormerlySerializedAs("astarData")]
	public AstarData data;

	
	[System.Obsolete("The 'astarData' field has been renamed to 'data'")]
	public AstarData astarData { get { return data; } }

	
#if UNITY_4_6 || UNITY_4_3
	public static new AstarPath active;
#else
	public static AstarPath active;
#endif

	public NavGraph[] graphs {
		get {
			if (data == null)
				data = new AstarData();
			return data.graphs;
		}
	}

	#region InspectorDebug
	
	public bool showNavGraphs = true;

	
	public bool showUnwalkableNodes = true;

	
	public GraphDebugMode debugMode;

	
	public float debugFloor = 0;

	
	public float debugRoof = 20000;

	
	public bool manualDebugFloorRoof = false;


	
	public bool showSearchTree = false;

	
	public float unwalkableNodeDebugSize = 0.3F;

	
	public PathLog logPathResults = PathLog.Normal;

	
	#endregion

	#region InspectorSettings
	
	public float maxNearestNodeDistance = 100;

	
	public float maxNearestNodeDistanceSqr {
		get { return maxNearestNodeDistance*maxNearestNodeDistance; }
	}

	
	public bool scanOnStartup = true;

	
	public bool fullGetNearestSearch = false;

	
	public bool prioritizeGraphs = false;

	
	public float prioritizeGraphsLimit = 1F;

	
	public AstarColor colorSettings;

	
	[SerializeField]
	protected string[] tagNames = null;

	
	public Heuristic heuristic = Heuristic.Euclidean;

	
	public float heuristicScale = 1F;

	
	public ThreadCount threadCount = ThreadCount.One;

	
	public float maxFrameTime = 1F;

	
	public bool batchGraphUpdates = false;

	
	public float graphUpdateBatchingInterval = 0.2F;

	
	[System.Obsolete("This field has been renamed to 'batchGraphUpdates'")]
	public bool limitGraphUpdates { get { return batchGraphUpdates; } set { batchGraphUpdates = value; } }

	
	[System.Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
	public float maxGraphUpdateFreq { get { return graphUpdateBatchingInterval; } set { graphUpdateBatchingInterval = value; } }

	/// <summary>@}</summary>
	#endregion

	#region DebugVariables
	

#if ProfileAstar
	/// <summary>
	/// How many paths has been computed this run. From application start.\n
	/// Debugging variable
	/// </summary>
	public static int PathsCompleted = 0;

	public static System.Int64 TotalSearchedNodes = 0;
	public static System.Int64 TotalSearchTime = 0;
#endif

	
	public float lastScanTime { get; private set; }

	
	[System.NonSerialized]
	public PathHandler debugPathData;

	[System.NonSerialized]
	public ushort debugPathID;

	
	string inGameDebugPath;

	/* @} */
	#endregion

	#region StatusVariables

	
	[System.NonSerialized]
	bool isScanningBacking;

	
	public bool isScanning { get { return isScanningBacking; } private set { isScanningBacking = value; } }

	
	public int NumParallelThreads {
		get {
			return pathProcessor.NumThreads;
		}
	}

	
	public bool IsUsingMultithreading {
		get {
			return pathProcessor.IsUsingMultithreading;
		}
	}

	
	[System.Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
	public bool IsAnyGraphUpdatesQueued { get { return IsAnyGraphUpdateQueued; } }

	
	public bool IsAnyGraphUpdateQueued { get { return graphUpdates.IsAnyGraphUpdateQueued; } }

	
	public bool IsAnyGraphUpdateInProgress { get { return graphUpdates.IsAnyGraphUpdateInProgress; } }


	public bool IsAnyWorkItemInProgress { get { return workItems.workItemsInProgress; } }

	
	internal bool IsInsideWorkItem { get { return workItems.workItemsInProgressRightNow; } }

	#endregion

	#region Callbacks
	
	public static System.Action OnAwakeSettings;

	public static OnGraphDelegate OnGraphPreScan;

	public static OnGraphDelegate OnGraphPostScan;

	public static OnPathDelegate OnPathPreSearch;

	public static OnPathDelegate OnPathPostSearch;

	public static OnScanDelegate OnPreScan;

	public static OnScanDelegate OnPostScan;

	public static OnScanDelegate OnLatePostScan;

	public static OnScanDelegate OnGraphsUpdated;

	
	public static System.Action On65KOverflow;

	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated;

	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated2;

	/* @} */
	#endregion

	#region MemoryStructures

	readonly GraphUpdateProcessor graphUpdates;

	internal readonly HierarchicalGraph hierarchicalGraph = new HierarchicalGraph();

	public readonly NavmeshUpdates navmeshUpdates = new NavmeshUpdates();

	readonly WorkItemProcessor workItems;

	PathProcessor pathProcessor;

	bool graphUpdateRoutineRunning = false;

	bool graphUpdatesWorkItemAdded = false;

	
	float lastGraphUpdate = -9999F;

	PathProcessor.GraphUpdateLock workItemLock;

	internal readonly PathReturnQueue pathReturnQueue;

	
	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	#endregion

	public bool showGraphs = false;


	private ushort nextFreePathID = 1;

	private AstarPath () {
		pathReturnQueue = new PathReturnQueue(this);

		pathProcessor = new PathProcessor(this, pathReturnQueue, 1, false);

		workItems = new WorkItemProcessor(this);
		graphUpdates = new GraphUpdateProcessor(this);

		graphUpdates.OnGraphsUpdated += () => {
			if (OnGraphsUpdated != null) {
				OnGraphsUpdated(this);
			}
		};
	}


	public string[] GetTagNames () {
		if (tagNames == null || tagNames.Length != 32) {
			tagNames = new string[32];
			for (int i = 0; i < tagNames.Length; i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}


	public static void FindAstarPath () {
		if (Application.isPlaying) return;
		if (active == null) active = GameObject.FindObjectOfType<AstarPath>();
		if (active != null && (active.data.graphs == null || active.data.graphs.Length == 0)) active.data.DeserializeGraphs();
	}


	public static string[] FindTagNames () {
		FindAstarPath();
		return active != null? active.GetTagNames () : new string[1] { "There is no AstarPath component in the scene" };
	}

	internal ushort GetNextPathID () {
		if (nextFreePathID == 0) {
			nextFreePathID++;

			if (On65KOverflow != null) {
				System.Action tmp = On65KOverflow;
				On65KOverflow = null;
				tmp();
			}
		}
		return nextFreePathID++;
	}

	void RecalculateDebugLimits () {
		debugFloor = float.PositiveInfinity;
		debugRoof = float.NegativeInfinity;

		bool ignoreSearchTree = !showSearchTree || debugPathData == null;
		for (int i = 0; i < graphs.Length; i++) {
			if (graphs[i] != null && graphs[i].drawGizmos) {
				graphs[i].GetNodes(node => {
					if (node.Walkable && (ignoreSearchTree || Pathfinding.Util.GraphGizmoHelper.InSearchTree(node, debugPathData, debugPathID))) {
						if (debugMode == GraphDebugMode.Penalty) {
							debugFloor = Mathf.Min(debugFloor, node.Penalty);
							debugRoof = Mathf.Max(debugRoof, node.Penalty);
						} else if (debugPathData != null) {
							var rnode = debugPathData.GetPathNode(node);
							switch (debugMode) {
							case GraphDebugMode.F:
								debugFloor = Mathf.Min(debugFloor, rnode.F);
								debugRoof = Mathf.Max(debugRoof, rnode.F);
								break;
							case GraphDebugMode.G:
								debugFloor = Mathf.Min(debugFloor, rnode.G);
								debugRoof = Mathf.Max(debugRoof, rnode.G);
								break;
							case GraphDebugMode.H:
								debugFloor = Mathf.Min(debugFloor, rnode.H);
								debugRoof = Mathf.Max(debugRoof, rnode.H);
								break;
							}
						}
					}
				});
			}
		}

		if (float.IsInfinity(debugFloor)) {
			debugFloor = 0;
			debugRoof = 1;
		}

		if (debugRoof-debugFloor < 1) debugRoof += 1;
	}

	Pathfinding.Util.RetainedGizmos gizmos = new Pathfinding.Util.RetainedGizmos();

	private void OnDrawGizmos () {
		
		if (active == null) active = this;

		if (active != this || graphs == null) {
			return;
		}

		if (Event.current.type != EventType.Repaint) return;

		colorSettings.PushToStatic(this);

		AstarProfiler.StartProfile("OnDrawGizmos");

		if (workItems.workItemsInProgress || isScanning) {
			
			gizmos.DrawExisting();
		} else {
			if (showNavGraphs && !manualDebugFloorRoof) {
				RecalculateDebugLimits();
			}

			Profiler.BeginSample("Graph.OnDrawGizmos");
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && graphs[i].drawGizmos)
					graphs[i].OnDrawGizmos(gizmos, showNavGraphs);
			}
			Profiler.EndSample();

			if (showNavGraphs) {
				euclideanEmbedding.OnDrawGizmos();
				if (debugMode == GraphDebugMode.HierarchicalNode) hierarchicalGraph.OnDrawGizmos(gizmos);
			}
		}

		gizmos.FinalizeDraw();

		AstarProfiler.EndProfile("OnDrawGizmos");
	}

#if !ASTAR_NO_GUI
	
	private void OnGUI () {
		if (logPathResults == PathLog.InGame && inGameDebugPath != "") {
			GUI.Label(new Rect(5, 5, 400, 600), inGameDebugPath);
		}
	}
#endif


	private void LogPathResults (Path path) {
		if (logPathResults != PathLog.None && (path.error || logPathResults != PathLog.OnlyErrors)) {
			string debug = path.DebugString(logPathResults);

			if (logPathResults == PathLog.InGame) {
				inGameDebugPath = debug;
			} else if (path.error) {
				Debug.LogWarning(debug);
			} else {
				//Debug.Log(debug);
			}
		}
	}

	
	private void Update () {
		
		if (!Application.isPlaying) return;

		navmeshUpdates.Update();

		if (!isScanning) {
			PerformBlockingActions();
		}

		pathProcessor.TickNonMultithreaded();

		pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions (bool force = false) {
		if (workItemLock.Held && pathProcessor.queue.AllReceiversBlocked) {
			
			pathReturnQueue.ReturnPaths(false);

			Profiler.BeginSample("Work Items");
			if (workItems.ProcessWorkItems(force)) {
				workItemLock.Release();
			}
			Profiler.EndSample();
		}
	}

	
	[System.Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void QueueWorkItemFloodFill () {
		throw new System.Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}


	[System.Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void EnsureValidFloodFill () {
		throw new System.Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	public void AddWorkItem (System.Action callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	
	public void AddWorkItem (System.Action<IWorkItemContext> callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	
	public void AddWorkItem (AstarWorkItem item) {
		workItems.AddWorkItem(item);

		if (!workItemLock.Held) {
			workItemLock = PausePathfindingSoon();
		}

#if UNITY_EDITOR
		if (!Application.isPlaying) {
			FlushWorkItems();
		}
#endif
	}

	#region GraphUpdateMethods

	
	public void QueueGraphUpdates () {
		if (!graphUpdatesWorkItemAdded) {
			graphUpdatesWorkItemAdded = true;
			var workItem = graphUpdates.GetWorkItem();

			
			AddWorkItem(new AstarWorkItem(() => {
				graphUpdatesWorkItemAdded = false;
				lastGraphUpdate = Time.realtimeSinceStartup;

				workItem.init();
			}, workItem.update));
		}
	}


	IEnumerator DelayedGraphUpdate () {
		graphUpdateRoutineRunning = true;

		yield return new WaitForSeconds(graphUpdateBatchingInterval-(Time.realtimeSinceStartup-lastGraphUpdate));
		QueueGraphUpdates();
		graphUpdateRoutineRunning = false;
	}


	public void UpdateGraphs (Bounds bounds, float delay) {
		UpdateGraphs(new GraphUpdateObject(bounds), delay);
	}


	public void UpdateGraphs (GraphUpdateObject ob, float delay) {
		StartCoroutine(UpdateGraphsInternal(ob, delay));
	}

	IEnumerator UpdateGraphsInternal (GraphUpdateObject ob, float delay) {
		yield return new WaitForSeconds(delay);
		UpdateGraphs(ob);
	}

	public void UpdateGraphs (Bounds bounds) {
		UpdateGraphs(new GraphUpdateObject(bounds));
	}


	public void UpdateGraphs (GraphUpdateObject ob) {
		graphUpdates.AddToQueue(ob);

		// If we should limit graph updates, start a coroutine which waits until we should update graphs
		if (batchGraphUpdates && Time.realtimeSinceStartup-lastGraphUpdate < graphUpdateBatchingInterval) {
			if (!graphUpdateRoutineRunning) {
				StartCoroutine(DelayedGraphUpdate());
			}
		} else {
			// Otherwise, graph updates should be carried out as soon as possible
			QueueGraphUpdates();
		}
	}

	/// <summary>
	/// Forces graph updates to complete in a single frame.
	/// This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	/// When all threads have paused, graph updates will be performed.
	/// Warning: Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	/// But you probably wont have to worry about that.
	///
	/// Note: This is almost identical to <see cref="FlushWorkItems"/>, but added for more descriptive name.
	/// This function will also override any time limit delays for graph updates.
	/// This is because graph updates are implemented using work items.
	/// So calling this function will also execute any other work items (if any are queued).
	///
	/// Will not do anything if there are no graph updates queued (not even execute other work items).
	/// </summary>
	public void FlushGraphUpdates () {
		if (IsAnyGraphUpdateQueued) {
			QueueGraphUpdates();
			FlushWorkItems();
		}
	}

	#endregion

	/// <summary>
	/// Forces work items to complete in a single frame.
	/// This will force all work items to run immidiately.
	/// This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	/// When all threads have paused, work items will be executed (which can be e.g graph updates).
	///
	/// Warning: Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	/// But you probably wont have to worry about that
	///
	/// Note: This is almost (note almost) identical to <see cref="FlushGraphUpdates"/>, but added for more descriptive name.
	///
	/// Will not do anything if there are no queued work items waiting to run.
	/// </summary>
	public void FlushWorkItems () {
		if (workItems.anyQueued) {
			var graphLock = PausePathfinding();
			PerformBlockingActions(true);
			graphLock.Release();
		}
	}

	/// <summary>
	/// Make sure work items are executed.
	///
	/// See: AddWorkItem
	///
	/// Deprecated: Use <see cref="FlushWorkItems()"/> instead.
	/// </summary>
	/// <param name="unblockOnComplete">If true, pathfinding will be allowed to start running immediately after completing all work items.</param>
	/// <param name="block">If true, work items that usually take more than one frame to complete will be forced to complete during this call.
	///              If false, then after this call there might still be work left to do.</param>
	[System.Obsolete("Use FlushWorkItems() instead")]
	public void FlushWorkItems (bool unblockOnComplete, bool block) {
		var graphLock = PausePathfinding();

		// Run tasks
		PerformBlockingActions(block);
		graphLock.Release();
	}

	/// <summary>
	/// Forces thread safe callbacks to run.
	/// Deprecated: Use <see cref="FlushWorkItems"/> instead
	/// </summary>
	[System.Obsolete("Use FlushWorkItems instead")]
	public void FlushThreadSafeCallbacks () {
		FlushWorkItems();
	}

	/// <summary>
	/// Calculates number of threads to use.
	/// If count is not Automatic, simply returns count casted to an int.
	/// Returns: An int specifying how many threads to use, 0 means a coroutine should be used for pathfinding instead of a separate thread.
	///
	/// If count is set to Automatic it will return a value based on the number of processors and memory for the current system.
	/// If memory is <= 512MB or logical cores are <= 1, it will return 0. If memory is <= 1024 it will clamp threads to max 2.
	/// Otherwise it will return the number of logical cores clamped to 6.
	///
	/// When running on WebGL this method always returns 0
	/// </summary>
	public static int CalculateThreadCount (ThreadCount count) {
#if UNITY_WEBGL
		return 0;
#else
		if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad) {
			int logicalCores = Mathf.Max(1, SystemInfo.processorCount);
			int memory = SystemInfo.systemMemorySize;

			if (memory <= 0) {
				Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
				memory = 1024;
			}

			if (logicalCores <= 1) return 0;
			if (memory <= 512) return 0;

			return 1;
		} else {
			return (int)count > 0 ? 1 : 0;
		}
#endif
	}

	/// <summary>
	/// Sets up all needed variables and scans the graphs.
	/// Calls Initialize, starts the ReturnPaths coroutine and scans all graphs.
	/// Also starts threads if using multithreading
	/// See: <see cref="OnAwakeSettings"/>
	/// </summary>
	protected override void Awake () {
		base.Awake();
		// Very important to set this. Ensures the singleton pattern holds
		active = this;

		if (FindObjectsOfType(typeof(AstarPath)).Length > 1) {
			Debug.LogError("You should NOT have more than one AstarPath component in the scene at any time.\n" +
				"This can cause serious errors since the AstarPath component builds around a singleton pattern.");
		}

		// Disable GUILayout to gain some performance, it is not used in the OnGUI call
		useGUILayout = false;

		// This class uses the [ExecuteInEditMode] attribute
		// So Awake is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		if (OnAwakeSettings != null) {
			OnAwakeSettings();
		}

		// To make sure all graph modifiers have been enabled before scan (to avoid script execution order issues)
		GraphModifier.FindAllModifiers();
		RelevantGraphSurface.FindAllGraphSurfaces();

		InitializePathProcessor();
		InitializeProfiler();
		ConfigureReferencesInternal();
		InitializeAstarData();

		// Flush work items, possibly added in InitializeAstarData to load graph data
		FlushWorkItems();

		euclideanEmbedding.dirty = true;

		navmeshUpdates.OnEnable();

		if (scanOnStartup && (!data.cacheStartup || data.file_cachedStartup == null)) {
			Scan();
		}
	}

	/// <summary>Initializes the <see cref="pathProcessor"/> field</summary>
	void InitializePathProcessor () {
		int numThreads = CalculateThreadCount(threadCount);

		// Outside of play mode everything is synchronous, so no threads are used.
		if (!Application.isPlaying) numThreads = 0;

		// Trying to prevent simple modding to add support for more than one thread
		if (numThreads > 1) {
			threadCount = ThreadCount.One;
			numThreads = 1;
		}

		int numProcessors = Mathf.Max(numThreads, 1);
		bool multithreaded = numThreads > 0;
		pathProcessor = new PathProcessor(this, pathReturnQueue, numProcessors, multithreaded);

		pathProcessor.OnPathPreSearch += path => {
			var tmp = OnPathPreSearch;
			if (tmp != null) tmp(path);
		};

		pathProcessor.OnPathPostSearch += path => {
			LogPathResults(path);
			var tmp = OnPathPostSearch;
			if (tmp != null) tmp(path);
		};

		// Sent every time the path queue is unblocked
		pathProcessor.OnQueueUnblocked += () => {
			if (euclideanEmbedding.dirty) {
				euclideanEmbedding.RecalculateCosts();
			}
		};

		if (multithreaded) {
			graphUpdates.EnableMultithreading();
		}
	}

	/// <summary>Does simple error checking</summary>
	internal void VerifyIntegrity () {
		if (active != this) {
			throw new System.Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
		}

		if (data == null) {
			throw new System.NullReferenceException("data is null... A* not set up correctly?");
		}

		if (data.graphs == null) {
			data.graphs = new NavGraph[0];
			data.UpdateShortcuts();
		}
	}

	/// <summary>\cond internal</summary>
	/// <summary>
	/// Internal method to make sure <see cref="active"/> is set to this object and that <see cref="data"/> is not null.
	/// Also calls OnEnable for the <see cref="colorSettings"/> and initializes data.userConnections if it wasn't initialized before
	///
	/// Warning: This is mostly for use internally by the system.
	/// </summary>
	public void ConfigureReferencesInternal () {
		active = this;
		data = data ?? new AstarData();
		colorSettings = colorSettings ?? new AstarColor();
		colorSettings.PushToStatic(this);
	}
	/// <summary>\endcond</summary>

	/// <summary>Calls AstarProfiler.InitializeFastProfile</summary>
	void InitializeProfiler () {
		AstarProfiler.InitializeFastProfile(new string[14] {
			"Prepare",          //0
			"Initialize",       //1
			"CalculateStep",    //2
			"Trace",            //3
			"Open",             //4
			"UpdateAllG",       //5
			"Add",              //6
			"Remove",           //7
			"PreProcessing",    //8
			"Callback",         //9
			"Overhead",         //10
			"Log",              //11
			"ReturnPaths",      //12
			"PostPathCallback"  //13
		});
	}

	/// <summary>
	/// Initializes the AstarData class.
	/// Searches for graph types, calls Awake on <see cref="data"/> and on all graphs
	///
	/// See: AstarData.FindGraphTypes
	/// </summary>
	void InitializeAstarData () {
		data.FindGraphTypes();
		data.Awake();
		data.UpdateShortcuts();
	}

	/// <summary>Cleans up meshes to avoid memory leaks</summary>
	void OnDisable () {
		gizmos.ClearCache();
	}

	/// <summary>
	/// Clears up variables and other stuff, destroys graphs.
	/// Note that when destroying an AstarPath object, all static variables such as callbacks will be cleared.
	/// </summary>
	void OnDestroy () {
		// This class uses the [ExecuteInEditMode] attribute
		// So OnDestroy is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		if (logPathResults == PathLog.Heavy)
			Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");

		if (active != this) return;

		// Block until the pathfinding threads have
		// completed their current path calculation
		PausePathfinding();

		navmeshUpdates.OnDisable();

		euclideanEmbedding.dirty = false;
		FlushWorkItems();

		// Don't accept any more path calls to this AstarPath instance.
		// This will cause all pathfinding threads to exit (if any exist)
		pathProcessor.queue.TerminateReceivers();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Processing Possible Work Items");

		// Stop the graph update thread (if it is running)
		graphUpdates.DisableMultithreading();

		// Try to join pathfinding threads
		pathProcessor.JoinThreads();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Returning Paths");


		// Return all paths
		pathReturnQueue.ReturnPaths(false);

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Destroying Graphs");


		// Clean up graph data
		data.OnDestroy();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Cleaning up variables");

		// Clear variables up, static variables are good to clean up, otherwise the next scene might get weird data

		// Clear all callbacks
		OnAwakeSettings         = null;
		OnGraphPreScan          = null;
		OnGraphPostScan         = null;
		OnPathPreSearch         = null;
		OnPathPostSearch        = null;
		OnPreScan               = null;
		OnPostScan              = null;
		OnLatePostScan          = null;
		On65KOverflow           = null;
		OnGraphsUpdated         = null;

		active = null;
	}

	#region ScanMethods

	/// <summary>
	/// Floodfills starting from the specified node.
	///
	/// Deprecated: Deprecated: Not meaningful anymore. The HierarchicalGraph takes care of things automatically behind the scenes
	/// </summary>
	[System.Obsolete("Not meaningful anymore. The HierarchicalGraph takes care of things automatically behind the scenes")]
	public void FloodFill (GraphNode seed) {
	}

	/// <summary>
	/// Floodfills starting from 'seed' using the specified area.
	///
	/// Deprecated: Not meaningful anymore. The HierarchicalGraph takes care of things automatically behind the scenes
	/// </summary>
	[System.Obsolete("Not meaningful anymore. The HierarchicalGraph takes care of things automatically behind the scenes")]
	public void FloodFill (GraphNode seed, uint area) {
	}

	/// <summary>
	/// Floodfills all graphs and updates areas for every node.
	/// The different colored areas that you see in the scene view when looking at graphs
	/// are called just 'areas', this method calculates which nodes are in what areas.
	/// See: Pathfinding.Node.area
	///
	/// Deprecated: Avoid using. This will force a full recalculation of the connected components. In most cases the HierarchicalGraph class takes care of things automatically behind the scenes now.
	/// </summary>
	[ContextMenu("Flood Fill Graphs")]
	[System.Obsolete("Avoid using. This will force a full recalculation of the connected components. In most cases the HierarchicalGraph class takes care of things automatically behind the scenes now.")]
	public void FloodFill () {
		hierarchicalGraph.RecalculateAll();
		workItems.OnFloodFill();
	}

	/// <summary>
	/// Returns a new global node index.
	/// Warning: This method should not be called directly. It is used by the GraphNode constructor.
	/// </summary>
	internal int GetNewNodeIndex () {
		return pathProcessor.GetNewNodeIndex();
	}

	/// <summary>
	/// Initializes temporary path data for a node.
	/// Warning: This method should not be called directly. It is used by the GraphNode constructor.
	/// </summary>
	internal void InitializeNode (GraphNode node) {
		pathProcessor.InitializeNode(node);
	}


	internal void DestroyNode (GraphNode node) {
		pathProcessor.DestroyNode(node);
	}


	[System.Obsolete("Use PausePathfinding instead. Make sure to call Release on the returned lock.", true)]
	public void BlockUntilPathQueueBlocked () {
	}

	
	public PathProcessor.GraphUpdateLock PausePathfinding () {
		return pathProcessor.PausePathfinding(true);
	}

	/// <summary>Blocks the path queue so that e.g work items can be performed</summary>
	PathProcessor.GraphUpdateLock PausePathfindingSoon () {
		return pathProcessor.PausePathfinding(false);
	}


	public void Scan (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		Scan(new NavGraph[] { graphToScan });
	}

	
	public void Scan (NavGraph[] graphsToScan = null) {
		var prevProgress = new Progress();

		Profiler.BeginSample("Scan");
		Profiler.BeginSample("Init");
		foreach (var p in ScanAsync(graphsToScan)) {
			if (prevProgress.description != p.description) {
#if !NETFX_CORE && UNITY_EDITOR
				Profiler.EndSample();
				Profiler.BeginSample(p.description);
				// Log progress to the console
				System.Console.WriteLine(p.description);
				prevProgress = p;
#endif
			}
		}
		Profiler.EndSample();
		Profiler.EndSample();
	}


	public IEnumerable<Progress> ScanAsync (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		return ScanAsync(new NavGraph[] { graphToScan });
	}

	
	public IEnumerable<Progress> ScanAsync (NavGraph[] graphsToScan = null) {
		if (graphsToScan == null) graphsToScan = graphs;

		if (graphsToScan == null) {
			yield break;
		}

		if (isScanning) throw new System.InvalidOperationException("Another async scan is already running");

		isScanning = true;

		VerifyIntegrity();

		var graphUpdateLock = PausePathfinding();

		// Make sure all paths that are in the queue to be returned
		// are returned immediately
		// Some modifiers (e.g the funnel modifier) rely on
		// the nodes being valid when the path is returned
		pathReturnQueue.ReturnPaths(false);

		if (!Application.isPlaying) {
			data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
		}

		int startFrame = Time.frameCount;

		yield return new Progress(0.05F, "Pre processing graphs");


		if (Time.frameCount != startFrame) {
			throw new System.Exception("Async scanning can only be done in the pro version of the A* Pathfinding Project");
		}

		if (OnPreScan != null) {
			OnPreScan(this);
		}

		GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);

		data.LockGraphStructure();

		var watch = System.Diagnostics.Stopwatch.StartNew();

		// Destroy previous nodes
		for (int i = 0; i < graphsToScan.Length; i++) {
			if (graphsToScan[i] != null) {
				((IGraphInternals)graphsToScan[i]).DestroyAllNodes();
			}
		}

		// Loop through all graphs and scan them one by one
		for (int i = 0; i < graphsToScan.Length; i++) {
			// Skip null graphs
			if (graphsToScan[i] == null) continue;

			// Just used for progress information
			// This graph will advance the progress bar from minp to maxp
			float minp = Mathf.Lerp(0.1F, 0.8F, (float)(i)/(graphsToScan.Length));
			float maxp = Mathf.Lerp(0.1F, 0.8F, (float)(i+0.95F)/(graphsToScan.Length));

			var progressDescriptionPrefix = "Scanning graph " + (i+1) + " of " + graphsToScan.Length + " - ";

			// Like a foreach loop but it gets a little complicated because of the exception
			// handling (it is not possible to yield inside try-except clause).
			var coroutine = ScanGraph(graphsToScan[i]).GetEnumerator();
			while (true) {
				try {
					if (!coroutine.MoveNext()) break;
				} catch {
					isScanning = false;
					data.UnlockGraphStructure();
					graphUpdateLock.Release();
					throw;
				}
				yield return coroutine.Current.MapTo(minp, maxp, progressDescriptionPrefix);
			}
		}

		data.UnlockGraphStructure();
		yield return new Progress(0.8F, "Post processing graphs");

		if (OnPostScan != null) {
			OnPostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);

		FlushWorkItems();

		yield return new Progress(0.9F, "Computing areas");

		hierarchicalGraph.RecalculateIfNecessary();

		yield return new Progress(0.95F, "Late post processing");

		// Signal that we have stopped scanning here
		// Note that no yields can happen after this point
		// since then other parts of the system can start to interfere
		isScanning = false;

		if (OnLatePostScan != null) {
			OnLatePostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);

		euclideanEmbedding.dirty = true;
		euclideanEmbedding.RecalculatePivots();

		// Perform any blocking actions
		FlushWorkItems();
		// Resume pathfinding threads
		graphUpdateLock.Release();

		watch.Stop();
		lastScanTime = (float)watch.Elapsed.TotalSeconds;

		System.GC.Collect();

		if (logPathResults != PathLog.None && logPathResults != PathLog.OnlyErrors) {
			Debug.Log("Scanning - Process took "+(lastScanTime*1000).ToString("0")+" ms to complete");
		}
	}

	IEnumerable<Progress> ScanGraph (NavGraph graph) {
		if (OnGraphPreScan != null) {
			yield return new Progress(0, "Pre processing");
			OnGraphPreScan(graph);
		}

		yield return new Progress(0, "");

		foreach (var p in ((IGraphInternals)graph).ScanInternal()) {
			yield return p.MapTo(0, 0.95f);
		}

		yield return new Progress(0.95f, "Assigning graph indices");

		// Assign the graph index to every node in the graph
		graph.GetNodes(node => node.GraphIndex = (uint)graph.graphIndex);

		if (OnGraphPostScan != null) {
			yield return new Progress(0.99f, "Post processing");
			OnGraphPostScan(graph);
		}
	}

	#endregion

	private static int waitForPathDepth = 0;

	[System.Obsolete("This method has been renamed to BlockUntilCalculated")]
	public static void WaitForPath (Path path) {
		BlockUntilCalculated(path);
	}

	
	/// <param name="path">The path to wait for. The path must be started, otherwise an exception will be thrown.</param>
	public static void BlockUntilCalculated (Path path) {
		if (active == null)
			throw new System.Exception("Pathfinding is not correctly initialized in this scene (yet?). " +
				"AstarPath.active is null.\nDo not call this function in Awake");

		if (path == null) throw new System.ArgumentNullException("Path must not be null");

		if (active.pathProcessor.queue.IsTerminating) return;

		if (path.PipelineState == PathState.Created) {
			throw new System.Exception("The specified path has not been started yet.");
		}

		waitForPathDepth++;

		if (waitForPathDepth == 5) {
			Debug.LogError("You are calling the BlockUntilCalculated function recursively (maybe from a path callback). Please don't do this.");
		}

		if (path.PipelineState < PathState.ReturnQueue) {
			if (active.IsUsingMultithreading) {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsTerminating) {
						waitForPathDepth--;
						throw new System.Exception("Pathfinding Threads seem to have crashed.");
					}

					// Wait for threads to calculate paths
					Thread.Sleep(1);
					active.PerformBlockingActions(true);
				}
			} else {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsEmpty && path.PipelineState != PathState.Processing) {
						waitForPathDepth--;
						throw new System.Exception("Critical error. Path Queue is empty but the path state is '" + path.PipelineState + "'");
					}

					// Calculate some paths
					active.pathProcessor.TickNonMultithreaded();
					active.PerformBlockingActions(true);
				}
			}
		}

		active.pathReturnQueue.ReturnPaths(false);
		waitForPathDepth--;
	}

	[System.Obsolete("Use AddWorkItem(System.Action) instead. Note the slight change in behavior (mentioned in the documentation).")]
	public static void RegisterSafeUpdate (System.Action callback) {
		active.AddWorkItem(new AstarWorkItem(callback));
	}

	
	public static void StartPath (Path path, bool pushToFront = false) {
		// Copy to local variable to avoid multithreading issues
		var astar = active;

		if (System.Object.ReferenceEquals(astar, null)) {
			Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
			return;
		}

		if (path.PipelineState != PathState.Created) {
			throw new System.Exception("The path has an invalid state. Expected " + PathState.Created + " found " + path.PipelineState + "\n" +
				"Make sure you are not requesting the same path twice");
		}

		if (astar.pathProcessor.queue.IsTerminating) {
			path.FailWithError("No new paths are accepted");
			return;
		}

		if (astar.graphs == null || astar.graphs.Length == 0) {
			Debug.LogError("There are no graphs in the scene");
			path.FailWithError("There are no graphs in the scene");
			Debug.LogError(path.errorLog);
			return;
		}

		path.Claim(astar);

		// Will increment p.state to PathState.PathQueue
		((IPathInternals)path).AdvanceState(PathState.PathQueue);
		if (pushToFront) {
			astar.pathProcessor.queue.PushFront(path);
		} else {
			astar.pathProcessor.queue.Push(path);
		}

		// Outside of play mode, all path requests are synchronous
		if (!Application.isPlaying) {
			BlockUntilCalculated(path);
		}
	}


	static readonly NNConstraint NNConstraintNone = NNConstraint.None;

	public NNInfo GetNearest (Vector3 position) {
		return GetNearest(position, NNConstraintNone);
	}

	
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
		return GetNearest(position, constraint, null);
	}


	public NNInfo GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
		// Cache property lookup
		var graphs = this.graphs;

		float minDist = float.PositiveInfinity;
		NNInfoInternal nearestNode = new NNInfoInternal();
		int nearestGraph = -1;

		if (graphs != null) {
			for (int i = 0; i < graphs.Length; i++) {
				NavGraph graph = graphs[i];

				// Check if this graph should be searched
				if (graph == null || !constraint.SuitableGraph(i, graph)) {
					continue;
				}

				NNInfoInternal nnInfo;
				if (fullGetNearestSearch) {
					// Slower nearest node search
					// this will try to find a node which is suitable according to the constraint
					nnInfo = graph.GetNearestForce(position, constraint);
				} else {
					// Fast nearest node search
					// just find a node close to the position without using the constraint that much
					// (unless that comes essentially 'for free')
					nnInfo = graph.GetNearest(position, constraint);
				}

				GraphNode node = nnInfo.node;

				// No node found in this graph
				if (node == null) {
					continue;
				}

				// Distance to the closest point on the node from the requested position
				float dist = ((Vector3)nnInfo.clampedPosition-position).magnitude;

				if (prioritizeGraphs && dist < prioritizeGraphsLimit) {
					// The node is close enough, choose this graph and discard all others
					minDist = dist;
					nearestNode = nnInfo;
					nearestGraph = i;
					break;
				} else {
					// Choose the best node found so far
					if (dist < minDist) {
						minDist = dist;
						nearestNode = nnInfo;
						nearestGraph = i;
					}
				}
			}
		}

		// No matches found
		if (nearestGraph == -1) {
			return new NNInfo();
		}

		// Check if a constrained node has already been set
		if (nearestNode.constrainedNode != null) {
			nearestNode.node = nearestNode.constrainedNode;
			nearestNode.clampedPosition = nearestNode.constClampedPosition;
		}

		if (!fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable(nearestNode.node)) {
			// Otherwise, perform a check to force the graphs to check for a suitable node
			NNInfoInternal nnInfo = graphs[nearestGraph].GetNearestForce(position, constraint);

			if (nnInfo.node != null) {
				nearestNode = nnInfo;
			}
		}

		if (!constraint.Suitable(nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr)) {
			return new NNInfo();
		}

		// Convert to NNInfo which doesn't have all the internal fields
		return new NNInfo(nearestNode);
	}

	/// <summary>
	/// Returns the node closest to the ray (slow).
	/// Warning: This function is brute-force and very slow, use with caution
	/// </summary>
	public GraphNode GetNearest (Ray ray) {
		if (graphs == null) return null;

		float minDist = Mathf.Infinity;
		GraphNode nearestNode = null;

		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;

		for (int i = 0; i < graphs.Length; i++) {
			NavGraph graph = graphs[i];

			graph.GetNodes(node => {
				Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin, lineDirection)*lineDirection);

				float tmp = Mathf.Abs(p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) return;

				tmp = Mathf.Abs(p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) return;

				float dist = (p-pos).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
				return;
			});
		}

		return nearestNode;
	}
}
