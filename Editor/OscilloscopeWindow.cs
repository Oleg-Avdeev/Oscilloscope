using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace Oscilloscope.Editor
{
	public class OscilloscopeWindow : EditorWindow
	{
		private OscilloscopeView _oscilloscope;
		private DropdownField _channelsDropdownField;

		[MenuItem("Window/Analysis/Oscilloscope")]
		public static void Create()
		{
			OscilloscopeWindow wnd = GetWindow<OscilloscopeWindow>();
			wnd.titleContent = new GUIContent("Oscilloscope");
		}

		public void CreateGUI()
		{
			VisualElement root = rootVisualElement;

			_channelsDropdownField = new DropdownField("Channels");
			_channelsDropdownField.RegisterValueChangedCallback(
				evt => _oscilloscope.CurrentSelectedChannel = evt.newValue);
			root.Add(_channelsDropdownField);

			_oscilloscope = new OscilloscopeView()
			{
				style = {
				position = Position.Absolute,
				left = 0, right = 0, bottom = 0, top = 90
			}
			};
			root.Add(_oscilloscope);

			var sampleWindowLength = new TextField("Sample Window (seconds)");
			sampleWindowLength.RegisterValueChangedCallback(e => {
				if (int.TryParse(e.newValue, out var v))
					Oscilloscope.ResizeBuffers(v);
			});
			sampleWindowLength.value = Oscilloscope.DefaultBufferSize.ToString();
			root.Add(sampleWindowLength);

			var scopeMin = new TextField("Min Y");
			scopeMin.RegisterValueChangedCallback(e => {
				if (float.TryParse(e.newValue, out var v))
					_oscilloscope.Scope = (min: v, max: _oscilloscope.Scope.max);
			});
			scopeMin.value = _oscilloscope.Scope.min.ToString();
			root.Add(scopeMin);

			var scopeMax = new TextField("Max Y");
			scopeMax.RegisterValueChangedCallback(e => {
				if (float.TryParse(e.newValue, out var v))
					_oscilloscope.Scope = (min: _oscilloscope.Scope.min, max: v);
			});
			scopeMax.value = _oscilloscope.Scope.max.ToString();
			root.Add(scopeMax);
		}

		private void OnGUI()
		{
			_oscilloscope.MarkDirtyRepaint();
			//TODO: Optimize
			_channelsDropdownField.choices = Oscilloscope.Channels.Keys.ToList();
		}
	}
}