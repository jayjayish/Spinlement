using System;
using UnityEngine;

namespace Utility
{
	/// <summary>
	/// Class that simply encapsulates all the 6 Unity collision and trigger events
	/// as System.Action events
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class CollisionTriggerEvents : MonoBehaviour
	{
		// ------------------------------------------------------------------------------------------------CACHED COMPONENTS

		private Collider _collider;
		public Collider Collider => _collider;

		// ----------------------------------------------------------------------------------------------------PUBLIC EVENTS

		/// <summary>
		/// CollisionTriggerEventsArgs: All the physics components from
		///		this object (Collider and rigidbody)
		/// Collider-Collision: All the collision data from the other object
		///		(the colliding one)
		/// </summary>
		public Action<CollisionTriggerEventsArgs, Collision> OnCollisionEntering;
		public Action<CollisionTriggerEventsArgs, Collision> OnCollisionStaying;
		public Action<CollisionTriggerEventsArgs, Collision> OnCollisionExiting;

		public Action<CollisionTriggerEventsArgs, Collider> OnTriggerEntering;
		public Action<CollisionTriggerEventsArgs, Collider> OnTriggerStaying;
		public Action<CollisionTriggerEventsArgs, Collider> OnTriggerExiting;

		// ------------------------------------------------------------------------------------------------PRIVATE VARIABLES

		/// <summary>
		/// Encapsulates all the physics components from this object that will
		/// be sent as argument to all the physics events. This is to avoid looking
		/// for them when a collision happened
		/// </summary>
		private CollisionTriggerEventsArgs _eventArgs;

		// ----------------------------------------------------------------------------------------------------UNITY METHODS

		private void Awake()
		{
			_collider = GetComponent<Collider>();

			// Create a cached struct with all the needed components of this class
			_eventArgs = new CollisionTriggerEventsArgs(_collider);
		}

		// --------------------------------------------------------------------------------------------------PRIVATE METHODS
		private void OnCollisionEnter(Collision collision)
		{
			OnCollisionEntering?.Invoke(_eventArgs, collision);
		}

		private void OnCollisionStay(Collision collision)
		{
			OnCollisionStaying?.Invoke(_eventArgs, collision);
		}

		private void OnCollisionExit(Collision collision)
		{
			OnCollisionExiting?.Invoke(_eventArgs, collision);
		}

		private void OnTriggerEnter(Collider other)
		{
			OnTriggerEntering?.Invoke(_eventArgs, other);
		}

		private void OnTriggerStay(Collider other)
		{
			OnTriggerStaying?.Invoke(_eventArgs, other);
		}

		private void OnTriggerExit(Collider other)
		{
			OnTriggerExiting?.Invoke(_eventArgs, other);
		}
		
		/// <summary>
		/// Used to encapsulate all required components from events
		/// triggered in "CollisionTriggerEvents"
		/// </summary>
		public struct CollisionTriggerEventsArgs
		{
			public readonly Collider Collider;

			public CollisionTriggerEventsArgs(Collider collider)
			{
				Collider = collider;
			}
		}
	}
}
