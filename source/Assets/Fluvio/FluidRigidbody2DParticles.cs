// 
// FluidRigidbody2DParticles.cs
//  
// Author:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2011-2014 Thinksquirrel Software, LLC
//
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2)
using UnityEngine;

namespace ThinksquirrelSoftware.Fluvio.Plugins
{
	/// <summary>
	/// FluidPlugin that creates rigidbodies for fluid particles.
	/// </summary>
	[AddComponentMenu("Fluvio/Plugins/Fluid Rigidbody2D 2D Particles")]
	public sealed class FluidRigidbody2DParticles : FluidPlugin
	{
		#region Serialized Fields
		[SerializeField] float m_ColliderSizeModifier                                           = 0.5f;
		[SerializeField] float m_MassModifier                                                   = 1f;
		[SerializeField] RigidbodyInterpolation2D m_RigidbodyInterpolation                      = RigidbodyInterpolation2D.None;
		[SerializeField] PhysicsMaterial2D m_PhysicMaterial                                     = null;
		#endregion
		
		#region Private Fields
		Transform m_ParentTransform                                                             = null;
		Rigidbody2D[] m_Rigidbodies                                                             = null;
		CircleCollider2D[] m_Colliders                                                          = null;
		bool m_Paused;
		Rigidbody2D[] m_EmptyRigidbodyArray                                                     = new Rigidbody2D[0];
		#endregion
		
		#region Private methods
		void OnAllocation(int oldCount, int newCount)
		{
			// Create parent transform
			if (!m_ParentTransform)
			{
				GameObject go = new GameObject(fluid.name + ": Rigidbody2D Particles");
				go.hideFlags |= HideFlags.HideInHierarchy;
				m_ParentTransform = go.transform;
			}
			
			if (newCount > oldCount)
			{
				// Increase the array sizes
				System.Array.Resize<Rigidbody2D>(ref m_Rigidbodies, newCount);
				System.Array.Resize<CircleCollider2D>(ref m_Colliders, newCount);
				
				// Create rigidbodies
				for(int i = oldCount; i < newCount; ++i)
				{
					// GameObject
					GameObject go = new GameObject("Particle " + i);
					go.layer = fluid.layer;
					go.transform.parent = m_ParentTransform;
					
					// Components
					CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
					Rigidbody2D body = go.AddComponent<Rigidbody2D>();
					m_Rigidbodies[i] = body;
					m_Colliders[i] = collider;
					
					// Configure rigidbody
					body.fixedAngle = true;
					body.gravityScale = 0;
					body.velocity = Vector3.zero;
				}
			}
			else if (newCount < oldCount)
			{
				// Delete additional objects
				for(int i = oldCount - 1; i >= newCount; --i)
					DestroyImmediate(m_Rigidbodies[i].gameObject);
				
				// Decrease the array sizes
				System.Array.Resize<Rigidbody2D>(ref m_Rigidbodies, newCount);
				System.Array.Resize<CircleCollider2D>(ref m_Colliders, newCount);
			}
			
			// Ignore collisions (layer-based)
			Physics.IgnoreLayerCollision(fluid.layer, fluid.layer, true);
		}
		void OnPreSolve(FluvioTimeStep timeStep)
		{   
			if (m_Rigidbodies == null)
				return;
			
			int l = Mathf.Min(m_Rigidbodies.Length, fluid.particleCount);
			
			for(int i = 0; i < l; ++i)
			{
				// Get objects
				Rigidbody2D body = m_Rigidbodies[i];
				CircleCollider2D collider = m_Colliders[i];
				FluidParticle particle = fluid.GetParticle(i);
				
				// Assign properties
				body.gameObject.SetActive(particle.enabled);
				
				if (body.isKinematic) body.isKinematic = false;
				
				body.mass = fluid.particleMass * m_MassModifier;
				body.interpolation = m_RigidbodyInterpolation;
				
				collider.radius = fluid.smoothingDistance * m_ColliderSizeModifier;
				collider.sharedMaterial = m_PhysicMaterial;
				
				if (particle.lifetime < 1.0f)
				{
					// Send physics information to particles (collisions)
					particle.position = body.transform.position;
					particle.velocity = body.velocity * timeStep.dt;
					
					// Assign particle
					fluid.SetParticle(ref particle, i);
				}
			}
		}
		void OnPostSolve(FluvioTimeStep timeStep)
		{
			if (m_Rigidbodies == null)
				return;
			
			int l = Mathf.Min(m_Rigidbodies.Length, fluid.particleCount);
			
			for(int i = 0; i < l; ++i)
			{
				// Get objects
				Rigidbody2D body = m_Rigidbodies[i];
				FluidParticle particle = fluid.GetParticle(i);
				
				if (body.isKinematic) body.isKinematic = false;
				body.transform.position = particle.position;
				body.velocity = particle.velocity * timeStep.inv_dt;
			}
		}
		
		void Update()
		{
			if (m_Rigidbodies == null)
				return;
			
			if (!fluid.isPlaying && !m_Paused)
			{
				m_Paused = true;
				
				int l = Mathf.Min(m_Rigidbodies.Length, fluid.particleCount);
				
				for(int i = 0; i < l; ++i)
				{
					// Get objects
					Rigidbody2D body = m_Rigidbodies[i];
					FluidParticle particle = fluid.GetParticle(i);
					
					body.transform.position = particle.position;
					body.velocity = Vector3.zero;
					body.isKinematic = true;
					body.Sleep();
				}
			}
			else if (fluid.isPlaying && m_Paused)
			{
				m_Paused = false;
			}
		}
		#endregion
		
		#region Protected, abstract, and virtual members
		protected override void Initialize()
		{
			fluid.OnAllocation += OnAllocation;
			fluid.OnPreSolve += OnPreSolve;
			fluid.OnPostSolve += OnPostSolve;
		}
		protected override void OnDestroy()
		{
			// Destroy rigidbodies
			if (m_ParentTransform)
				DestroyImmediate(m_ParentTransform.gameObject);
			
			if (fluid)
			{
				// Unsubscribe from events
				fluid.OnAllocation -= OnAllocation;
				fluid.OnPreSolve -= OnPreSolve;
				fluid.OnPostSolve -= OnPostSolve;
			}
			
			base.OnDestroy();
		}
		#endregion
		
		#region Public API
		/// <summary>
		/// Controls the size of physics colliders, as a multiple of smoothing distance.
		/// </summary>
		public float colliderSizeModifier { get { return m_ColliderSizeModifier; } set { m_ColliderSizeModifier = value; } }
		/// <summary>
		/// Controls the mass of rigidbodies, as a multiple of particle mass.
		/// </summary>
		public float massModifier { get { return m_MassModifier; } set { m_MassModifier = value; } }
		/// <summary>
		/// The rigidbody interpolation to use for each particle.
		/// </summary>
		public RigidbodyInterpolation2D rigidbodyInterpolation { get { return m_RigidbodyInterpolation; } set { m_RigidbodyInterpolation = value; } }
		/// <summary>
		/// The physic material to use for each particle.
		/// </summary>
		public PhysicsMaterial2D physicMaterial { get { return m_PhysicMaterial; } set { m_PhysicMaterial = value; } }
		/// <summary>
		/// Provides access to the rigidbody array used for fluid particles. The index of the rigidbody in the array corresponds to the index of the particle in the fluid's particle array.
		/// </summary>
		/// <remarks>
		/// Modifying rigidbodies directly may produce non-physical behaviour. Do not destroy these objects!
		/// </remarks>
		public Rigidbody2D[] rigidbodies { get { return m_Rigidbodies ?? m_EmptyRigidbodyArray; } }
		#endregion
	}
}
#endif