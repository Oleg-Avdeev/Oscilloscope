using System.Collections.Generic;
using Oscilloscope.Utilities;
using UnityEngine;

namespace Oscilloscope
{
	public static class Oscilloscope
	{
		public const int DefaultBufferSize = 10 * 60;

		public static readonly Dictionary<string, Channel> Channels = new();

		private static readonly Dictionary<string, Dictionary<string, string>> _channelSubtypeMap = new();
		private static readonly Dictionary<GameObject, string> _channelOwnerMap = new();

		public static void Sample(GameObject channelOwner, float value)
		{
			Sample(GetOwnerFullName(channelOwner), value);
		}

		public static void Sample(GameObject channelOwner, string subChannel, float value)
		{
			Sample(GetOwnerFullName(channelOwner), subChannel, value);
		}

		public static void Sample(string channelName, string subChannel, float value)
		{
			Sample(GetChannelFullName(channelName, subChannel), value);
		}

		public static void Sample(string channelName, float value)
		{
			var channel = Channels.GetOrAdd(channelName, _ => new Channel(DefaultBufferSize));
			channel.RegisterValue(value);
		}

		public static void ResizeBuffers(int bufferSize)
		{
			foreach (var channel in Channels)
				channel.Value.ResizeBuffer(bufferSize);
		}

		private static string GetChannelFullName(string channelName, string subChannel)
		{
			var mapping = _channelSubtypeMap.GetOrAdd(channelName, _ => new Dictionary<string, string>());

			if (mapping.TryGetValue(subChannel, out var fullChannelName))
				return fullChannelName;

			fullChannelName = $"{channelName}/{subChannel}";
			mapping.Add(subChannel, fullChannelName);
			return fullChannelName;
		}

		private static string GetOwnerFullName(GameObject channelOwner)
		{
			if (_channelOwnerMap.TryGetValue(channelOwner, out var fullName))
				return fullName;

			const int maxDepth = 10;

			var originalOwner = channelOwner;
			fullName = channelOwner.name;
			for (var i = 0; i < maxDepth; i++)
			{
				if (channelOwner.transform.parent == null) break;
				channelOwner = channelOwner.transform.parent.gameObject;
				fullName = $"{channelOwner.name}.{fullName}";
			}

			_channelOwnerMap.Add(originalOwner, fullName);
			return fullName;
		}

		public class Channel
		{
			public float[] Values;
			public int RingIndex = 0;

			public Channel(int bufferSize)
			{
				ResizeBuffer(bufferSize);
			}

			public void ResizeBuffer(int bufferSize)
			{
				var buffer = new float[bufferSize];

				if (Values != null)
				{
					for (var i = 0; i < bufferSize && i < Values.Length; i++)
						buffer[i] = Values[i];

					if (Values.Length > bufferSize)
						RingIndex = 0;
				}

				Values = buffer;
			}

			public void RegisterValue(float value)
			{
				Values[Next(ref RingIndex)] = value;
			}

			private int Next(ref int index)
			{
				var lastIndex = index;
				index = (index + 1) % Values.Length;
				return index;
			}
		}
	}


}