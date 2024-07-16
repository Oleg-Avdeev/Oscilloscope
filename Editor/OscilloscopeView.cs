using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;

namespace Oscilloscope.Editor
{
	public class OscilloscopeView : VisualElement
	{
		public const int StepsCount = 10;

		public string CurrentSelectedChannel;
		
		private (float min, float max) _scope = (-3f, 3f);
		public (float min, float max) Scope 
		{
			get {
				return _scope;
			}

			set {
				_scope = value;
				UpdateLabels();
			}
		}

		private Vector2 size;
		private Painter2D painter;
		private Label[] _labels;

		private void UpdateLabels()
		{
			var scopeRange = Mathf.Abs(Scope.max - Scope.min);
			var step = scopeRange / StepsCount;

			for (var i = 0; i < StepsCount/2; i++)
			{
				var positiveDistance = ScopeValue(step * i);
				_labels[StepsCount/2 + i].text = (step * i).ToString("0.###");

				if (i > 0)
				{
					var negativeDistance = ScopeValue(-step * i);
					_labels[StepsCount/2 - i].text = (-step * i).ToString("0.###");
				}
			}
		}

		public OscilloscopeView()
		{
			VisualElement root = contentContainer;

			_labels = new Label[StepsCount];
			for (var i = 0; i < StepsCount; i++)
				root.Add(_labels[i] = new Label(i.ToString()));

			UpdateLabels();

			generateVisualContent += GenerateVisualContent;
		}

		void GenerateVisualContent(MeshGenerationContext context)
		{
			DrawOscilloscope(context);
		}

		void DrawOscilloscope(MeshGenerationContext context)
		{
			if (Scope.min == Scope.max) return;

			float width = contentRect.width;
			float height = contentRect.height;
			
			size = new Vector2(width, height);
			painter = context.painter2D;

			DrawGridLines();
			
			if (string.IsNullOrEmpty(CurrentSelectedChannel)) return;
			if (!Oscilloscope.Channels.TryGetValue(CurrentSelectedChannel, out var channel)) return;
			
			DrawChannelValues(channel);
		}

		private void DrawChannelValues(Oscilloscope.Channel channel)
		{
			painter.lineWidth = 1f;
			painter.strokeColor = new Color(1f, 1f, 1f, 1f);
			for (var i = 1; i < channel.Values.Length; i++)
			{
				var scopedValuePrev = ScopeValue(channel.Values[i - 1]);
				var scopedValue = ScopeValue(channel.Values[i]);

				var x0 = (float)(i - 1)/channel.Values.Length;
				var x = (float)i/channel.Values.Length;
				
				var age = (float)(channel.RingIndex - i) / channel.Values.Length;
				if (age < 0) age += 1f;
			
				var color = new Color(1f, 1f, 1f, 1f - age/1.5f);

				painter.strokeColor = color;
				Line(x0, scopedValuePrev, x, scopedValue);
			}
		}

		private void DrawGridLines()
		{
			var scopeRange = Mathf.Abs(Scope.max - Scope.min);
			var step = scopeRange / StepsCount;

			painter.strokeColor = new Color(1f, 1f, 1f, 0.4f);

			painter.lineWidth = 0.8f;
			var zeroScoped = ScopeValue(0);
			Line(0, zeroScoped, 1, zeroScoped);
				
			painter.lineWidth = 0.4f;

			for (var distance = step; distance < Scope.max; distance += step)
			{
				var value = ScopeValue(distance);
				Line(0, value, 1, value);
			}

			for (var distance = -step; distance > Scope.min; distance -= step)
			{
				var value = ScopeValue(distance);
				Line(0, value, 1, value);
			}
		}

		private float ScopeValue(float value) => Mathf.InverseLerp(Scope.max, Scope.min, value);

		private void Line(float x0, float y0, float x1, float y1)
		{
			painter.BeginPath();
			painter.MoveTo(new Vector2(x0, y0) * size);
			painter.LineTo(new Vector2(x1, y1) * size);
			painter.Stroke();
		}
	}
}