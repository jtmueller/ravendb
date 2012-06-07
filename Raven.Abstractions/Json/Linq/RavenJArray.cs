﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Json.Utilities;

namespace Raven.Json.Linq
{
	/// <summary>
	/// Represents a JSON array.
	/// </summary>
	public class RavenJArray : RavenJToken, IEnumerable<RavenJToken>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RavenJArray"/> class.
		/// </summary>
		public RavenJArray()
		{
			Items = new List<RavenJToken>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RavenJArray"/> class with the specified content.
		/// </summary>
		/// <param name="content">The contents of the array.</param>
		public RavenJArray(IEnumerable content)
		{
			Items = new List<RavenJToken>();
			var ravenJToken = content as RavenJToken;
			if (ravenJToken != null)
			{
				Items.Add(ravenJToken);
			}
			else
			{
				foreach (var item in content)
				{
					ravenJToken = item as RavenJToken;
					Items.Add(ravenJToken ?? new RavenJValue(item));
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RavenJArray"/> class with the specified content.
		/// </summary>
		/// <param name="content">The contents of the array.</param>
		public RavenJArray(params object[] content)
			: this((IEnumerable)content)
		{

		}

		public RavenJArray(IEnumerable<RavenJToken> content)
		{
			Items = new List<RavenJToken>();
			Items.AddRange(content);
		}

		/// <summary>
		/// Gets the node type for this <see cref="RavenJToken"/>.
		/// </summary>
		/// <value>The type.</value>
		public override JTokenType Type
		{
			get { return JTokenType.Array; }
		}

		/// <summary>
		/// Gets or sets the <see cref="RavenJToken"/> at the specified index.
		/// </summary>
		/// <value></value>
		public RavenJToken this[int index]
		{
			get { return Items[index]; }
			set { Items[index] = value; }
		}

		public override RavenJToken CloneToken()
		{
			return CloneTokenImpl(new RavenJArray());
		}

		public int Length { get { return Items.Count; } }

		private List<RavenJToken> Items { get; set; }

		public new static RavenJArray Load(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				if (!reader.Read())
					throw new Exception("Error reading RavenJArray from JsonReader.");
			}

			if (reader.TokenType != JsonToken.StartArray)
				throw new Exception("Error reading RavenJArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

			if (reader.Read() == false)
				throw new Exception("Unexpected end of json array");

			var ar = new RavenJArray();
			RavenJToken val = null;
			do
			{
				switch (reader.TokenType)
				{
					case JsonToken.Comment:
						// ignore comments
						break;
					case JsonToken.EndArray:
						return ar;
					case JsonToken.StartObject:
						val = RavenJObject.Load(reader);
						ar.Items.Add(val);
						break;
					case JsonToken.StartArray:
						val = RavenJArray.Load(reader);
						ar.Items.Add(val);
						break;
					default:
						val = RavenJValue.Load(reader);
						ar.Items.Add(val);
						break;
				}
			} while (reader.Read());

			throw new Exception("Error reading RavenJArray from JsonReader.");
		}

		/// <summary>
		/// Load a <see cref="RavenJArray"/> from a string that contains JSON.
		/// </summary>
		/// <param name="json">A <see cref="String"/> that contains JSON.</param>
		/// <returns>A <see cref="RavenJArray"/> populated from the string that contains JSON.</returns>
		public static new RavenJArray Parse(string json)
		{
			try
			{
				JsonReader jsonReader = new JsonTextReader(new StringReader(json));

				return Load(jsonReader);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Could not parse json:" + Environment.NewLine + json, e);
			}
		}

		/// <summary>
		/// Writes this token to a <see cref="JsonWriter"/>.
		/// </summary>
		/// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
		/// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
		public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
		{
			writer.WriteStartArray();

			if (Items != null)
			{
				foreach (var token in Items)
				{
					token.WriteTo(writer, converters);
				}
			}

			writer.WriteEndArray();
		}

		#region IEnumerable<RavenJToken> Members

		public IEnumerator<RavenJToken> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		#endregion

		internal override IEnumerable<KeyValuePair<string, RavenJToken>> GetCloningEnumerator()
		{
			return Items.Select(i => new KeyValuePair<string, RavenJToken>(null, i));
		}

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion


		public void Add(RavenJToken token)
		{
			Items.Add(token);
		}

		public bool Remove(RavenJToken token)
		{
			return Items.Remove(token);
		}

		public void RemoveAt(int index)
		{
			Items.RemoveAt(index);
		}

		public void Insert(int index, RavenJToken token)
		{
			Items.Insert(index, token);
		}

		public override IEnumerable<T> Values<T>()
		{
			return Items.Convert<T>();
		}

		public override IEnumerable<RavenJToken> Values()
		{
			return Items;
		}

		internal override void AddForCloning(string key, RavenJToken token)
		{
			Add(token);
		}
	}
}
