using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xllusion.Utils {
	
	/// <summary>
	/// DrawLine2D
	///
	/// Draw 2d line with fade out options
	/// </summary>
	public class DrawLine2D : MonoBehaviour {
		
		public delegate void FadeOutListener();
		public FadeOutListener OnFadeOutlistener;
		public bool fadeOut = false;
		public float fadeOutTime = 1f;
		
		private LineRenderer lineRenderer;
		private EdgeCollider2D edgeCollider2D;
		private List<Vector2> points;
		private IEnumerator fadeTo;
		
		void Awake() {
			if(lineRenderer == null) {
				lineRenderer = GetComponent<LineRenderer>();
				lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
				lineRenderer.SetColors(Color.white, Color.white);
				lineRenderer.startWidth = 0.1f;
				lineRenderer.endWidth = 0.1f;
				lineRenderer.useWorldSpace = true;
			}
			
			// EdgeCollider2D
			if(edgeCollider2D == null) edgeCollider2D = gameObject.GetComponent<EdgeCollider2D> ();
			
			// Points
			points = new List<Vector2> ();
		}
		
		void Update() {
			if (Input.GetMouseButtonDown(0)) {
				Reset();
				Vector2 touchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				AddStartPoint(touchPoint);
				
				if(fadeOut) StopFadeOut();
			}else if (Input.GetMouseButton(0)) {
				Vector2 touchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				AddEndPoint(touchPoint);
			}else if (Input.GetMouseButtonUp(0)) {
				AddCollider();
				if(fadeOut) StartFadeOut();
			}
		}
		
		public void SetFadeOutListener(FadeOutListener fadeOutlistener) {
			if(OnFadeOutlistener != null) OnFadeOutlistener = null;
			OnFadeOutlistener = fadeOutlistener;
		}
		
		private void AddStartPoint(Vector2 touchPosition) {
			if(points.Count == 0) {
				points.Add(touchPosition);
				lineRenderer.positionCount = points.Count;
				lineRenderer.SetPosition(lineRenderer.positionCount - 1, touchPosition);
			}
		}
		
		private void AddEndPoint(Vector2 touchPosition) {
			if(points.Count == 2) points.RemoveAt(1);
			points.Add(touchPosition);
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPosition(lineRenderer.positionCount - 1, touchPosition);
		}
		
		private void AddCollider() {
			if(edgeCollider2D != null && points.Count > 1 ) {
				edgeCollider2D.enabled = true;
				edgeCollider2D.points = points.ToArray ();
			}
		}
		
		private void StopFadeOut() {
			if(fadeTo != null) StopCoroutine(fadeTo);
		}
		
		private void StartFadeOut() {
			fadeTo = FadeTo(lineRenderer, 1f, 0f, 0.5f, fadeOutTime);
			StartCoroutine(fadeTo);
		}
		
		private void Reset() {
			if(lineRenderer != null ) {
				lineRenderer.positionCount = 0;
				lineRenderer.SetColors(Color.white, Color.white);
			}
			if(points != null) points.Clear();
			if(edgeCollider2D != null) {
				edgeCollider2D.Reset();
				edgeCollider2D.enabled = false;
			}
		}
		
		IEnumerator FadeTo(LineRenderer lineRenderer, float startOpacity, float targetOpacity, float wait, float duration) {

			// Set initial opacity.
			Color color = new Color(1f,1f,1f,1f);
			color.a = startOpacity;
			lineRenderer.SetColors(color, color);
			
			// Wait for seconds
			yield return new WaitForSeconds(wait);

			// Track how many seconds we've been fading.
			float t = 0;

			while(t < duration) {
				// Step the fade forward one frame.
				t += Time.deltaTime;
				// Turn the time into an interpolation factor between 0 and 1.
				float blend = Mathf.Clamp01(t/duration);

				// Blend to the corresponding opacity between start & target.
				color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);

				// Apply the resulting color.
				lineRenderer.SetColors(color, color);

				// Wait one frame, and repeat.
				yield return null;
			}
			
			Reset();
			
			if(OnFadeOutlistener != null) OnFadeOutlistener();
		}
	}

}
