// Content.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Data;
using dEngine.Services;


namespace dEngine
{
	/// <summary>
	/// A reference to a content resource.
	/// </summary>
	public class Content<TAsset> : IEquatable<Content<TAsset>>, IEquatable<string>
		where TAsset : AssetBase, new()
	{
		private string _contentId = "";

		/// <summary>
		/// Fired when the new content is fetched.
		/// </summary>
		private Action<string, TAsset> _onGot;

		private Reference<TAsset> _reference;

		private Content()
		{
		}

		/// <summary>
		/// Creates a new content object with optional default uri.
		/// </summary>
		public Content(string contentId = "")
		{
			ContentId = contentId;
		}

		/// <summary>
		/// The asset.
		/// </summary>
		public TAsset Asset => _reference;

		/// <summary>
		/// The content URL to download from. When set will delete current data and start downloading.
		/// </summary>
		[InstMember(1)]
		public string ContentId
		{
			get { return _contentId; }
			private set
			{
				if (string.IsNullOrWhiteSpace(value))
					value = "";

				_contentId = value;

				_reference = CacheableContentProvider<TAsset>.GetAsync(value);

			    if (_reference.Resource.IsDownloaded)
			    {
			        OnResourceDownloaded();

                    if (_reference.Resource.Content == null)
                        _reference.Resource.Download();
                }
			    else
			        _reference.Resource.Downloaded += OnResourceDownloaded;
			}
		}

		/// <summary>
		/// If true, Data exists and no download is happening.
		/// </summary>
		public bool IsLoaded { get; set; }

		/// <summary>
		/// Determines if two Contents have equal content IDs.
		/// </summary>
		public bool Equals(Content<TAsset> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(_contentId, other._contentId);
		}

		/// <summary>
		/// Determines if this Content has the given content id.
		/// </summary>
		public bool Equals(string contentId)
		{
			return string.Equals(_contentId, contentId);
		}

		/// <inheritdoc />
		~Content()
		{
			_reference = null;
			_onGot = null;
		}

		/// <summary>
		/// Sets the given object as the content owner, and subscribes with a callback.
		/// </summary>
		public void Subscribe(object owner, Action<string, TAsset> callback = null)
		{
			_onGot = callback;

			if (IsLoaded)
				callback?.Invoke(_contentId, Asset);
		}

		private void OnResourceDownloaded()
		{
			_onGot?.Invoke(_reference.Resource.Id, _reference);
			IsLoaded = true;
		}

        /// <summary/>
	    public override string ToString()
	    {
	        return _contentId;
	    }

	    /// <summary>
		/// Returns <see cref="ContentId" />.
		/// </summary>
		public static implicit operator string(Content<TAsset> content)
		{
			return content?.ContentId;
		}

		/// <summary>
		/// Returns a <see cref="Content{TAsset}" /> using the given content id.
		/// </summary>
		public static implicit operator Content<TAsset>(string content)
		{
			return new Content<TAsset>(content);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Content<TAsset>)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _contentId.GetHashCode();
		}

	    public void RetryDownload()
	    {
	        ContentId = _contentId;
	    }
	}
}