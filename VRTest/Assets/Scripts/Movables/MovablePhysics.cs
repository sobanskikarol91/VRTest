using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WIUtils;
using DarkTonic.MasterAudio;

[SelectionBase]
public class MovablePhysics : MonoBehaviour
{
	private const float MinVelocitySqrMagnitude = 0.1f;
	private const float MaxAngularVelocitySqrMagnitude = 0.1f;

	public float throwForceMod = 5f, throwCollisionVelocity = 4f;
	public float additionalMass;

	private float baseMass;
	private bool init;

    public bool CannotBeAtached;

	[HideInInspector]
	public bool ShouldBeTiedToSpawningContainer;

	[SerializeField]
	protected Rigidbody moveRigidbody;

	public virtual Rigidbody MoveRigidbody
	{
		get
		{
			if(moveRigidbody == null)
			{
				moveRigidbody = GetComponent<Rigidbody>();

				if(moveRigidbody == null && gameObject != null)
				{
					moveRigidbody = gameObject.AddComponent<Rigidbody>();
					SetupRigibody();
				}
			}

			if(!init)
			{
				baseMass = moveRigidbody.mass;

				init = true;
			}

			return moveRigidbody;
		}
	}

	protected virtual void SetupRigibody()
	{
		if (moveRigidbody != null)
		{
			moveRigidbody.isKinematic = true;
			moveRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
	}

	[SerializeField]
	private Rigidbody throwRigidbody;

	public virtual Rigidbody ThrowRigidbody
	{
		get
		{
			return throwRigidbody != null ? throwRigidbody : MoveRigidbody;
		}
	}

	public float customVolume = -1;
	public float cachedVolume = -1;

	public List<Renderer> Renderers = new List<Renderer>();

	public Renderer CurrentRenderer
	{
		get
		{
			if(Renderers == null)
			{
				Renderers = new List<Renderer>();
			}

			if(Renderers.Count == 0)
			{
				return null;
			}
			else if(Renderers[0] == null)
			{
				MeshRenderer mr = GetComponent<MeshRenderer>();

				if(mr == null)
				{
					Debug.LogError("Problem with detecting bounds on movable");
					return null;
				}

				Renderers.Add(mr);
			}

			Renderers.RemoveAll(value => value == null);

			return Renderers.FirstOrDefault();
		}
	}

	public Mesh CurrentMesh
	{
		get
		{
			var meshFilter = CurrentRenderer.GetComponent<MeshFilter>();

			if(!meshFilter)
			{
				var skinnedMeshRenderer = CurrentRenderer as SkinnedMeshRenderer;

				return skinnedMeshRenderer ? skinnedMeshRenderer.sharedMesh : null;
			}

			return meshFilter ? meshFilter.mesh : null;
		}
	}

	[ContextMenu("Find renderers")]
	public void FindRenderers()
	{
		var allRenderers = GetComponentsInChildren<Renderer>();

		Renderers.Clear();
		Renderers.AddRange(allRenderers);
	}

	[ContextMenu("Setup predicting collisions")]
	public void SetupPredictingCollisions()
	{
		if(CPController != null)
		{
			DestroyImmediate(CPController.gameObject);
		}

		var predictingGameObject = new GameObject();

		predictingGameObject.name = "PredictCollisionCollider";

		var cpController = predictingGameObject.AddComponent<CollisionPredictionController>();
		MeshCollider meshCollider = predictingGameObject.GetComponent<MeshCollider>();
		if(meshCollider == null)
		{
			meshCollider = predictingGameObject.AddComponent<MeshCollider>();
		}

		Rigidbody rb = predictingGameObject.GetComponent<Rigidbody>();
		if(rb == null)
		{
			rb = predictingGameObject.AddComponent<Rigidbody>();
		}

		meshCollider.convex = true;
		meshCollider.isTrigger = true;
		meshCollider.inflateMesh = true;
		meshCollider.skinWidth = 0.0f;
		meshCollider.gameObject.layer = LayerMask.NameToLayer("PredictCollision");
		rb.isKinematic = true;

		var meshFilter = Renderers.FirstOrDefault().GetComponent<MeshFilter>();

		if(meshFilter != null)
		{
			meshCollider.sharedMesh = meshFilter.sharedMesh;
		}

		predictingGameObject.transform.SetParent(transform);
		predictingGameObject.transform.localPosition = Vector3.zero;
		predictingGameObject.transform.localRotation = Quaternion.identity;
		predictingGameObject.transform.localScale = Vector3.one;

		CPController = cpController;
	}

	public float Volume
	{
		get
		{
			if(customVolume > 0)
			{
				return customVolume;
			}

			if(cachedVolume > 0)
			{
				return cachedVolume;
			}

			if(Application.isPlaying)
			{
				var renderer = GetComponentInChildren<MeshRenderer>();
				var skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

				if(renderer != null || skinnedRenderer != null)
				{
					Vector3 scale = Vector3.one;
					Mesh mesh = null;

					if(renderer != null)
					{
						mesh = renderer.GetComponent<MeshFilter>().mesh;
						scale = renderer.transform.localScale;
					}
					else if(skinnedRenderer != null)
					{
						mesh = skinnedRenderer.sharedMesh;
						scale = skinnedRenderer.transform.localScale;
					}

					var bounds = mesh.bounds;

					cachedVolume = 1.333333f * 0.125f * Mathf.PI * 0.9f * bounds.size.x * bounds.size.y * bounds.size.z * scale.x * scale.y * scale.z;
				}
				else if(VolumeCollider.IsNot<BoxCollider>())
				{
					bool isKinematic = false;

					if(MoveRigidbody != null)
					{
						isKinematic = MoveRigidbody.isKinematic;
						MoveRigidbody.isKinematic = true;
					}
					Quaternion rot = gameObject.transform.rotation;
					gameObject.transform.rotation = Quaternion.identity;

					BoxCollider bc = gameObject.AddComponent<BoxCollider>();
					bc.isTrigger = true;
					if(MoveRigidbody != null)
					{
						MoveRigidbody.isKinematic = isKinematic;
					}
					//Ellipsid inside the box collider, decreased by 10%
					cachedVolume = 1.3333333f * 0.125f * Mathf.PI * 0.9f * bc.size.x * transform.lossyScale.x * bc.size.y * transform.lossyScale.y * bc.size.z * transform.lossyScale.z;
					Destroy(bc);
					gameObject.transform.rotation = rot;
				}
				else
				{
					BoxCollider bc = (BoxCollider)VolumeCollider;
					cachedVolume = 1.3333333f * 0.125f * Mathf.PI * 0.9f * bc.size.x * transform.lossyScale.x * bc.size.y * transform.lossyScale.y * bc.size.z * transform.lossyScale.z;
				}

				return cachedVolume;
			}
			else
			{
				return cachedVolume;
			}
		}
	}

	public bool kinematicInHand = false, kinematicPartsInHand = false,
		kinematicWhenMove = true, kinematicPartsWhenMove = true,
		kinematicOnTable = false, kinematicPartsOnTable = false;

	public Transform hookPoint;

	private IMovable movable;

	public IMovable Movable
	{
		get
		{
			if(movable == null)
			{
				movable = GetComponent<IMovable>();
			}

			return movable;
		}
	}

	private MovableBackup movableBackup;

	public MovableBackup MovableBackup
	{
		get
		{
			if(movableBackup == null)
			{
				movableBackup = GetComponent<MovableBackup>();
			}

			return movableBackup;
		}
	}

	public Collider VolumeCollider
	{
		get
		{
			if(volumeCollider == null)
			{
				volumeCollider = GetComponent<Collider>();
			}

			return volumeCollider;
		}
	}

	public List<ContainsTrigger> containsTriggers = new List<ContainsTrigger>();

	public Container ContainsTriggerContainer
	{
		get
		{
			if(containsTriggers.Count > 0)
			{
				return containsTriggers[0].Container;
			}

			return null;
		}
	}

	public List<Container> ContainsTriggerContainers
	{
		get
		{
			return containsTriggers.Select(ct => ct.Container).Distinct().ToList();
		}
	}

	public ContainsTrigger FirstContainsTrigger
	{
		get
		{
			if(containsTriggers.Count > 0)
			{
				return containsTriggers[0];
			}

			return null;
		}
	}

	public HeatTrigger CurrentHeatTrigger
	{
		get
		{
            if(heatTriggers.Count == 0)
            {
                return null;
            }

			return heatTriggers.Where(value => !Extensions.IsNull(value))
				.FirstOrDefault(d => d.Triggers.Count > 0 && d.Triggers.Any(o => o.enabled));
		}
	}

	public void Clear()
	{
		for(int i = colliders.Count - 1; i >= 0; i--)
		{
			if(colliders[i] == null)
			{
				colliders.RemoveAt(i);
			}
		}
		if(MovableBackup != null)
		{
			MovableBackup.FindChildren();
		}
		FindRenderers();
		if(CPController != null)
		{
			var physicsMesh = GetComponent<MeshCollider>();
			if(physicsMesh != null)
			{
				CPController.GetComponent<MeshCollider>().sharedMesh = physicsMesh.sharedMesh;
			}
		}
	}

	[SerializeField]
	List<HeatTrigger> heatTriggers = new List<HeatTrigger>();

	//HEAT TRIGGERS HIERARCHY:		1 - MAIN, 2 - GLOBAL, 3 - CONTAINER TRIGGER, 4 - OTHERS

	public bool AddHeatTrigger(HeatTrigger ht)
	{
		if(!heatTriggers.Contains(ht))
		{
			if(ht.main)
			{
				heatTriggers.Insert(0, ht);
				return true;
			}
			else if(ht.global)
			{
				var globals = heatTriggers.FindLastIndex(d => d.global);
				if(globals != -1)
				{
					heatTriggers.Insert(globals, ht);
					return true;
				}
				else
				{
					var others = heatTriggers.FindIndex(d => !d.global && !d.main);
					if(others != -1)
					{
						heatTriggers.Insert(others, ht);
						return true;
					}
					else
					{
						heatTriggers.Add(ht);
						return true;
					}
				}
			}
			else
			{
				heatTriggers.Add(ht);
				return true;
			}
		}
		return false;
	}

	public bool RemoveHeatTrigger(HeatTrigger ht)
	{
		if(heatTriggers.Contains(ht))
		{
			heatTriggers.Remove(ht);
			return true;
		}
		return false;
	}

	public List<Collider> colliders = new List<Collider>();

	private Collider volumeCollider;
	public bool createContainsTrigger;
	public Collider myContainsTrigger;
	public CollisionPredictionController CPController;

	private Dictionary<Collider, CollidersTriggers> collidersDictionary;
	public bool usePlayerRotation = true;

	internal class CollidersTriggers
	{
		internal List<ContainsTrigger> containsTriggers = new List<ContainsTrigger>();
		internal HeatTrigger heatTriggers;
	}

	public float temperatureCoolSpeed0_20 = -0.1f;
	public float temperatureCoolSpeed20_75 = 0.1f;
	public float temperatureCoolSpeed75_100 = 0.05f;

	public float GetTemperatureCoolSpeed(float currenttemp)
	{
		var result = temperatureCoolSpeed0_20;
		if(currenttemp > 75)
		{
			result = temperatureCoolSpeed75_100;
		}
		else if(currenttemp > 20)
		{
			result = temperatureCoolSpeed20_75;
		}
		return result* (1f + temperatureCoolMod);

	}
	[HideInInspector]
	public bool temperatureReduction = true;

	private static Vector2 temperatureCap = new Vector2(20, 100);

	public float Temperature {
        get
        {
			if(movable != null && movable is ProductPart) {
				var pp = movable as ProductPart;
				if(pp.stats != null)
					return pp.stats.temperature;
			}
            return temperature;
        }
        set
        {
			if(movable != null && movable is ProductPart)
			{
				var pp = movable as ProductPart;
				if(pp.stats != null)
					pp.stats.temperature = value;
				return;
			}
			temperature = value;
            if(explosive != null && temperature > explosive.explodeTemperature)
            {
                explosive.Explode();
            }
        }
    }
    private float temperature;

    private Explosive explosive;

    public float temperatureCoolMod;

	public float linearLimit = 0.001f, linearSpring = 1000f, linearDamper = 100f, linearDriveSpring = 0f, linearDriveDamper = 100f,
		angularLimit = 10f, angularSpring = 100f, angularDamper = 10f, angularDriveSpring = 100f, angularDriveDamper = 10f, massScale = 1f;

	[SerializeField]
	private bool _canBreak;

	public GameObject FracturedObject;

	public bool CanBreak
	{
		get
		{
			return _canBreak && FracturedObject != null;
		}

		set
		{
			if(FracturedObject != null)
			{
				_canBreak = value;
			}
			else
			{
				_canBreak = false;
			}
		}
	}

	public void Break(string breakSound = null, float strength = 3.0f)
	{
		if(ActionsManager.Me.SomethingIsMoving && Player.Me.ActiveMovable == movable)
		{
			return;
		}
		if(movable != null)
		{
			if(movable is Container)
			{
				PlayerStats.Me.dailyBrokenPlates += 1;
				//PERK_UNBREAKABLE_PLATES
				if(SkillSystem.GetPerk<UnbreakablePlates>().IsUnlock)
				{
					return;
				}

				PlateCounter.Me.ReportDestroy(movable as Container);

				var plate = movable as Container;
				var movables = plate.MovablesRecursive;

				for(int i = movables.Count - 1; i >= 0; i--)
				{
					if(movables[i] != null && movables[i].PhysicsController != null)
					{
						movables[i].PhysicsController.Detach();
					}
				}
			}
			else if(movable is InfiniteFluidProduct)
			{
				PlayerStats.Me.dailyBrokenBottles += 1;
			}

			if(movable.OwnerSpawnPoint != null)
			{
				movable.OwnerSpawnPoint.UnregisterMovable(movable);
			}

			if(movable.PhysicsController.ContainsTriggerContainer != null)
			{
				movable.PhysicsController.Detach();
			}
			else
			{
				MovableBackup.Detach(movable.gameObject);
			}

			if(!string.IsNullOrEmpty(breakSound))
			{
				MasterAudio.PlaySound3DAtTransform(breakSound, transform);
			}

			StandsManager.Me.SetFracturedObject(FracturedObject, strength);
			MovableBackup.CollapseMovable(movable);

			//CHECK IF IN CONTEX AND EXIT
			if(ContextStrategyController.Me.IsUseContextControll)
			{
				ContextStrategyController.Me.CloseCurrentContext(() => Destroy(movable.gameObject));
			}
			else
			{
				Destroy(movable.gameObject);
			}
		}
	}

	private void Awake()
	{
		movable = null;
		volumeCollider = null;
		containsTriggers = new List<ContainsTrigger>();
		movableBackup = null;

		var collidersInChildren = GetComponentsInChildren<Collider>();
		if(!colliders.Any())
		{
			colliders = collidersInChildren
				.Where(x => !x.isTrigger)
				.ToList();
		}

		collidersDictionary = new Dictionary<Collider, CollidersTriggers>();
		foreach(var c in collidersInChildren)
		{
			if(c != null)
			{
				if(!collidersDictionary.ContainsKey(c))
				{
					collidersDictionary.Add(c, new CollidersTriggers());
				}
			}
		}

		if(Volume > 0) { }

		Temperature = 20;

		AddTrigger(this);


        explosive = GetComponentInChildren<Explosive>();
    }

	private void Update()
	{
		float heat;
		GoodnesType type;
		if(StandsManager.Me.SimTick)
		{
			if(HeatTaken(out heat, out type))
			{
				Temperature += heat * StandsManager.Me.SimTickTime;
			}
			else
			{
				if(temperatureReduction)
				{
					Temperature -= (Temperature < 75 ? (Temperature > 20 ? 
						temperatureCoolSpeed20_75 * SkillSystem.GetSkill<SlowerGettingCold>().GetMultipler()
						: temperatureCoolSpeed0_20) : temperatureCoolSpeed75_100 * SkillSystem.GetSkill<SlowerGettingCold>().GetMultipler()) * StandsManager.Me.SimTickTime * (1f + temperatureCoolMod);
				}
			}

			Temperature = Mathf.Clamp(Temperature, temperatureCap.x, temperatureCap.y);
		}

		if(transform.position.y < -10f)
		{
			MovableBackup.CollapseMovable(movable);
			if(movable == null)
			{
				return;
			}
			if(movable.PhysicsController.movableBackup != null)
				movable.PhysicsController.MovableBackup.Detach();
			Destroy(movable.gameObject);
		}

		if(containsTriggers.Count > 0 && containsTriggers[0] != null)
		{
			if(Vector3.Distance(transform.position, containsTriggers[0].gameObject.transform.position) > 0.5f)
			{
				MovableBackup.CollapseMovable(movable);
				if(movable == null)
				{
					return;
				}
				if(movable.PhysicsController.movableBackup != null)
					movable.PhysicsController.MovableBackup.Detach();
			}
		}

		if (Movable is ProductPart && !Movable.IsMoving && Movable.AttachedMovable == null)
			PreserveFromInfiniteSpinning();
	}

	private void PreserveFromInfiniteSpinning()
	{
		if(Time.renderedFrameCount % 10 != 0)
			return;

		var isVelocitySmallEnough = MoveRigidbody.velocity.sqrMagnitude < MinVelocitySqrMagnitude;
		var isMovableSpinning = MoveRigidbody.angularVelocity.sqrMagnitude > MaxAngularVelocitySqrMagnitude;

		if (isVelocitySmallEnough && isMovableSpinning)
			MoveRigidbody.angularVelocity = Vector3.zero;
	}

	public bool HeatTaken(out float heat, out GoodnesType type)
	{
		heat = 0;
		type = GoodnesType.ThermalFrying;

		var trigger = CurrentHeatTrigger;
		if(trigger != null)
		{
			heat = trigger.Heat;
			type = trigger.ThermalGoodnesType;
		}
		return heat > 0;
	}

	private Collider[] checkHits = new Collider[100];

	public void CheckDetach()
	{
		if(!Movable.IsMoving && containsTriggers.Count > 0 && colliders.Count > 0 && !CheckContainsTriggers())
		{
			Detach();
		}
	}

	private bool CheckContainsTriggers()
	{
		var atLeastOneNonMeshCollider = false;
		var collidersToRemovie = new List<Collider>();
		for(int i = colliders.Count - 1; i >= 0; i--)
		{
			if(colliders[i] == null)
			{
				colliders.RemoveAt(i);
				continue;
			}
			else
			{
				var c = colliders[i];
				if(!(c is MeshCollider))
				{
					atLeastOneNonMeshCollider = true;
				}

				if(c is SphereCollider)
				{
					var sc = (SphereCollider)c;

					float maxs = Mathf.Max(c.transform.lossyScale.x, c.transform.lossyScale.y, c.transform.lossyScale.z);

					int h = Physics.OverlapSphereNonAlloc(c.transform.position + c.transform.rotation * sc.center, maxs * sc.radius, checkHits, ActionsManager.Me.ingredientAreaLayer, QueryTriggerInteraction.Collide);
					for(int j = 0; j < h; j++)
					{
						var triggerColliders = FirstContainsTrigger.Triggers;

						for(int k = 0; k < triggerColliders.Length; k++)
						{
							if(triggerColliders[k] == checkHits[j])
							{
								return true;
							}
						}
					}
				}
				else if(c is BoxCollider)
				{
					var bc = (BoxCollider)c;
					var sc = bc.size;
					sc.Scale(c.transform.lossyScale);
					int h = Physics.OverlapBoxNonAlloc(c.transform.position + c.transform.rotation * bc.center, sc / 2f, checkHits, c.transform.rotation, ActionsManager.Me.ingredientAreaLayer, QueryTriggerInteraction.Collide);
					for(int j = 0; j < h; j++)
					{
						var triggerColliders = FirstContainsTrigger.Triggers;

						for(int k = 0; k < triggerColliders.Length; k++)
						{
							if(triggerColliders[k] == checkHits[j])
							{
								return true;
							}
						}
					}
				}
				else if(c is CapsuleCollider)
				{
					var cc = (CapsuleCollider)c;
					Vector3 point0, point1;

					Utils.GetCapsuleColliderPoints(cc, out point0, out point1);

					int h = Physics.OverlapCapsuleNonAlloc(point0, point1, cc.radius, checkHits, ActionsManager.Me.ingredientAreaLayer, QueryTriggerInteraction.Collide);
					for(int j = 0; j < h; j++)
					{
						var triggerColliders = FirstContainsTrigger.Triggers;

						for(int k = 0; k < triggerColliders.Length; k++)
						{
							if(triggerColliders[k] == checkHits[j])
							{
								return true;
							}
						}
					}
				}
			}
		}

		if(atLeastOneNonMeshCollider)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public void UpdateMass(float additionalMass)
	{
		this.additionalMass = additionalMass;

		MoveRigidbody.mass = baseMass + additionalMass;
	}

	public void ColliderContainsAttached(Collider collider, ContainsTrigger containsTrigger)
	{
		if(collider != null && collidersDictionary.ContainsKey(collider))
		{
			collidersDictionary[collider].containsTriggers.Add(containsTrigger);
		}
	}

	public bool ColliderContainsDetached(Collider c, ContainsTrigger ct)
	{
		if(c == null)
		{
			foreach(var kvp in collidersDictionary)
			{
				kvp.Value.containsTriggers.Remove(ct);
			}

			return true;
		}
		else
		{
			if(collidersDictionary.ContainsKey(c))
			{
				collidersDictionary[c].containsTriggers.Remove(ct);
			}

			return !collidersDictionary.Any(p => p.Value.containsTriggers.Any(pp => pp == ct));
		}
	}

	public bool ColliderContainsDetached(Collider c, HeatTrigger ht)
	{
		if(c == null)
		{
			foreach(var kvp in collidersDictionary)
			{
				kvp.Value.heatTriggers = null;
			}
		}
		else
		{
			if(collidersDictionary.ContainsKey(c))
			{
				collidersDictionary[c].heatTriggers = null;
			}
		}
		return collidersDictionary.Any(p => p.Value.heatTriggers == ht);
	}

	public void ColliderHeatAttached(Collider c, HeatTrigger ht)
	{
		if(c != null && collidersDictionary.ContainsKey(c))
		{
			collidersDictionary[c].heatTriggers = ht;
		}
		else if(movable is ProductPart && (movable as ProductPart).thermalAreaCheckers.Count > 0)
		{
		}
	}

	public void ColliderHeatDetached(Collider c)
	{
		if(c == null)
		{
			foreach(var kvp in collidersDictionary)
			{
				kvp.Value.heatTriggers = null;
			}
		}
		else
		{
			if(collidersDictionary.ContainsKey(c))
			{
				collidersDictionary[c].heatTriggers = null;
			}
		}
	}
	public void AddCollider(Collider c)
	{
		if(c != null && !collidersDictionary.ContainsKey(c))
		{
			collidersDictionary.Add(c, new CollidersTriggers());
		}
	}

	public void SetMoveRigidbody(Rigidbody rb)
	{
		moveRigidbody = rb;
	}

	public void SetKinematicBonesWhenMove(bool value, Transform parent)
	{
		var joints = GetComponent<JointCorrections>();

		if(joints != null)
		{
			foreach(var boneRigidbody in joints.BonesRigidbodies)
			{
				boneRigidbody.IsKinematicExtended(value);
				boneRigidbody.transform.SetParent(parent);
			}
		}
	}

	public virtual void OnMoveStart()
	{
		SetKinematicBonesWhenMove(kinematicWhenMove, MoveRigidbody.transform);
		MoveRigidbody.IsKinematicExtended(kinematicWhenMove);
	}

	public virtual void OnMoveEnd()
	{
	}

	public void OnRegisterToSpawnPoint(SpawnPoint spawnPoint)
	{
		if(spawnPoint is PlayerHandSpawnPoint)
		{
			OnRegisterToPlayerHand((PlayerHandSpawnPoint)spawnPoint);
		}
		else
		{
			OnRegisterToTable(spawnPoint);
		}
	}

	public virtual void OnUnregisterFromSpawnPoint(SpawnPoint spawnPoint)
	{
		if(spawnPoint is PlayerHandSpawnPoint)
		{
			OnUnregisterFromPlayerHand((PlayerHandSpawnPoint)spawnPoint);
		}
		else
		{
			OnUnregisterFromTable(spawnPoint);
		}
	}

	public virtual void OnRegisterToTable(SpawnPoint spawnPoint)
	{
		if(spawnPoint.Owner is KnifeBlockStand || spawnPoint.Owner is SliceContainer)
		{
			if(Movable is KnifeBetter)
			{
				kinematicOnTable = true;
			}
		}
		else
		{
			kinematicOnTable = false;
		}
		MoveRigidbody.IsKinematicExtended(kinematicOnTable);
	}

	public virtual void OnUnregisterFromTable(SpawnPoint spawnPoint)
	{
	}

	public virtual void OnRegisterToPlayerHand(PlayerHandSpawnPoint hand)
	{
		MoveRigidbody.IsKinematicExtended(kinematicInHand);
	}

	public virtual void OnUnregisterFromPlayerHand(PlayerHandSpawnPoint hand)
	{
	}

	private void OnJointBreak(float breakForce)
	{
		if(Player.Me.ActiveMovable == movable)
		{
			PlayerHandJoint.OnPlayerHandJointBreak(Movable);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		var container = Movable as Container;
		var collisionMovable = MovableBackup.TryGetComponent<IMovable>(collision.gameObject);

		if(Movable.IsThrow)
		{
			Movable.IsThrow = false;
		}

		if (Player.Me.ActiveMovable == Movable && (container == null || !HasContainerCollisionWithSelf(container, collisionMovable)))
		{
			KitchenInput.Me.ForceFeedbackImpulse(0f, 0.4f, 0.2f);
		}

		float velocityMagnitude = collision.relativeVelocity.magnitude;

		if(velocityMagnitude > throwCollisionVelocity)
		{
			velocityMagnitude *= baseMass + additionalMass;

			if(velocityMagnitude > throwCollisionVelocity * 1.3f)
			{
				KitchenInput.Me.ForceFeedbackImpulse(collision.relativeVelocity.magnitude * 0.11f, 0.3f, 0.2f);
			}

			if(container != null && !HasContainerCollisionWithSelf(container, collisionMovable))
			{
				Movable.OnThrowCollision(collision);
			}
			else
			{
				Movable.OnThrowCollision(collision);
			}
		}
	}

	private bool HasContainerCollisionWithSelf(Container container, IMovable movable)
	{
		if(container == null || movable == null)
		{
			return false;
		}

		return container.MovablesRecursive.Contains(movable);
	}

	public void Detach()
	{
		foreach(var c in containsTriggers.ToList())
		{
			c.Detach(this, MovableBackup);
		}
	}

	public void CreateContainsTrigger()
	{
		SphereCollider sc = gameObject.AddComponent<SphereCollider>();
		BoxCollider bc = gameObject.AddComponent<BoxCollider>();
		Collider finalCollider = null;
		float bcVolume = bc.size.x * bc.size.y * bc.size.z; //A*B*C
		float scVolume = (4.0f / 3.0f) * sc.radius * sc.radius * sc.radius * Mathf.PI; //(4/3)-Pi*r^3

		if(bcVolume <= scVolume)
		{
			finalCollider = bc;
			Destroy(sc);
		}
		else
		{
			finalCollider = sc;
			Destroy(bc);
		}

		if(finalCollider != null)
		{
			if(myContainsTrigger != null)
			{
				Destroy(myContainsTrigger);
			}
			finalCollider.isTrigger = true;
			colliders.Add(finalCollider);
			myContainsTrigger = null;
			myContainsTrigger = finalCollider;
		}
	}

	public void AddTrigger(MovablePhysics mp)
	{
		if(mp.myContainsTrigger == null && mp.createContainsTrigger)
		{
			SphereCollider sc = mp.gameObject.AddComponent<SphereCollider>();

			sc.isTrigger = true;
			sc.radius *= 0.2f;

			mp.myContainsTrigger = sc;
		}

		if(mp.myContainsTrigger != null && !mp.colliders.Contains(mp.myContainsTrigger))
		{
			mp.colliders.Add(mp.myContainsTrigger);
		}
	}

#if UNITY_EDITOR

	[ContextMenu("GetContainsCollider")]
	public void GetContainsCollider()
	{
		List<Collider> col = gameObject.GetComponents<Collider>().Where(p => p.isTrigger).ToList();
		if(col.Count > 0)
		{
			myContainsTrigger = col[0];
		}
	}

#endif
}