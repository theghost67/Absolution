using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyBox
{
    public static class MyExtensions
	{
		/// <summary>
		/// Swap two elements in array
		/// </summary>
		public static void Swap<T>(this T[] array, int a, int b)
		{
			T x = array[a];
			array[a] = array[b];
			array[b] = x;
		}

		public static bool IsWorldPointInViewport(this Camera camera, Vector3 point)
		{
			var position = camera.WorldToViewportPoint(point);
			return position.x > 0 && position.y > 0;
		}

		/// <summary>
		/// Gets a point with the same screen point as the source point,
		/// but at the specified distance from camera.
		/// </summary>
		public static Vector3 WorldPointOffsetByDepth(this Camera camera,
			Vector3 source,
			float distanceFromCamera,
			Camera.MonoOrStereoscopicEye eye = Camera.MonoOrStereoscopicEye.Mono)
		{
			var screenPoint = camera.WorldToScreenPoint(source, eye);
			return camera.ScreenToWorldPoint(screenPoint.SetZ(distanceFromCamera),
				eye);
		}


		/// <summary>
		/// Sets the lossy scale of the source Transform.
		/// </summary>
		public static Transform SetLossyScale(this Transform source,
			Vector3 targetLossyScale)
		{
			source.localScale = source.lossyScale.Pow(-1).ScaleBy(targetLossyScale)
				.ScaleBy(source.localScale);
			return source;
		}

		/// <summary>
		/// Sets a layer to the source's attached GameObject and all of its children
		/// in the hierarchy.
		/// </summary>
		public static T SetLayerRecursively<T>(this T source, string layerName)
			where T : Component
		{
			source.gameObject.SetLayerRecursively(LayerMask.NameToLayer(layerName));
			return source;
		}

		/// <summary>
		/// Sets a layer to the source's attached GameObject and all of its children
		/// in the hierarchy.
		/// </summary>
		public static T SetLayerRecursively<T>(this T source, int layer)
			where T : Component
		{
			source.gameObject.SetLayerRecursively(layer);
			return source;
		}

		/// <summary>
		/// Sets a layer to the source GameObject and all of its children in the
		/// hierarchy.
		/// </summary>
		public static GameObject SetLayerRecursively(this GameObject source,
			string layerName)
		{
			source.SetLayerRecursively(LayerMask.NameToLayer(layerName));
			return source;
		}

		/// <summary>
		/// Sets a layer to the source GameObject and all of its children in the
		/// hierarchy.
		/// </summary>
		public static GameObject SetLayerRecursively(this GameObject source,
			int layer)
		{
			var allTransforms = source.GetComponentsInChildren<Transform>(true);
			foreach (var tf in allTransforms) tf.gameObject.layer = layer;
			return source;
		}


		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			var toGet = gameObject.GetComponent<T>();
			if (toGet != null) return toGet;
			return gameObject.AddComponent<T>();
		}

		public static T GetOrAddComponent<T>(this Component component) where T : Component
		{
			var toGet = component.gameObject.GetComponent<T>();
			if (toGet != null) return toGet;
			return component.gameObject.AddComponent<T>();
		}

		public static bool HasComponent<T>(this GameObject gameObject)
		{
			return gameObject.GetComponent<T>() != null;
		}



		/// <summary>
		/// Get all components of specified Layer in childs
		/// </summary>
		public static List<Transform> GetObjectsOfLayerInChilds(this GameObject gameObject, int layer)
		{
			List<Transform> list = new List<Transform>();
			CheckChildsOfLayer(gameObject.transform, layer, list);
			return list;
		}

		/// <summary>
		/// Get all components of specified Layer in childs
		/// </summary>
		public static List<Transform> GetObjectsOfLayerInChilds(this GameObject gameObject, string layer)
		{
			return gameObject.GetObjectsOfLayerInChilds(LayerMask.NameToLayer(layer));
		}

		/// <summary>
		/// Get all components of specified Layer in childs
		/// </summary>
		public static List<Transform> GetObjectsOfLayerInChilds(this Component component, string layer)
		{
			return component.GetObjectsOfLayerInChilds(LayerMask.NameToLayer(layer));
		}

		/// <summary>
		/// Get all components of specified Layer in childs
		/// </summary>
		public static List<Transform> GetObjectsOfLayerInChilds(this Component component, int layer)
		{
			return component.gameObject.GetObjectsOfLayerInChilds(layer);
		}

		private static void CheckChildsOfLayer(Transform transform, int layer, List<Transform> childsCache)
		{
			foreach (Transform t in transform)
			{
				CheckChildsOfLayer(t, layer, childsCache);

				if (t.gameObject.layer != layer) continue;
				childsCache.Add(t);
			}
		}

#if UNITY_PHYSICS_ENABLED

		/// <summary>
		/// Swap Rigidbody IsKinematic and DetectCollisions
		/// </summary>
		/// <param name="body"></param>
		/// <param name="state"></param>
		public static void SetBodyState(this Rigidbody body, bool state)
		{
			body.isKinematic = !state;
			body.detectCollisions = state;
		}

#endif

		/// <summary>
		/// Find all Components of specified interface
		/// </summary>
		public static T[] FindObjectsOfInterface<T>() where T : class
		{
			var monoBehaviours = Object.FindObjectsOfType<Transform>();

			return monoBehaviours.Select(behaviour => behaviour.GetComponent(typeof(T))).OfType<T>().ToArray();
		}

		/// <summary>
		/// Find all Components of specified interface along with Component itself
		/// </summary>
		public static ComponentOfInterface<T>[] FindObjectsOfInterfaceAsComponents<T>() where T : class
		{
			return Object.FindObjectsOfType<Component>()
				.Where(c => c is T)
				.Select(c => new ComponentOfInterface<T>(c, c as T)).ToArray();
		}

		public struct ComponentOfInterface<T>
		{
			public readonly Component Component;
			public readonly T Interface;

			public ComponentOfInterface(Component component, T @interface)
			{
				Component = component;
				Interface = @interface;
			}
		}


		#region One Per Instance

		/// <summary>
		/// Get components with unique Instance ID
		/// </summary>
		public static T[] OnePerInstance<T>(this T[] components) where T : Component
		{
			if (components == null || components.Length == 0) return null;
			return components.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
		}
        
#if UNITY_PHYSICS2D_ENABLED

		/// <summary>
		/// Get hits with unique owner Instance ID
		/// </summary>
		public static RaycastHit2D[] OneHitPerInstance(this RaycastHit2D[] hits)
		{
			if (hits == null || hits.Length == 0) return null;
			return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
		}

        /// <summary>
		/// Get colliders with unique owner Instance ID
		/// </summary>
		public static Collider2D[] OneHitPerInstance(this Collider2D[] hits)
		{
			if (hits == null || hits.Length == 0) return null;
			return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
		}

		/// <summary>
		/// Get colliders with unique owner Instance ID
		/// </summary>
		public static List<Collider2D> OneHitPerInstanceList(this Collider2D[] hits)
		{
			if (hits == null || hits.Length == 0) return null;
			return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToList();
		}
        
#endif

		#endregion
	}
}