// CacheableContentProvider.cs - dEngine
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
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using dEngine.Data;
using dEngine.Instances.Materials;
using dEngine.Settings.Global;
using dEngine.Utility;

namespace dEngine.Services
{
    /// <summary>
    /// A resource manager for asset types.
    /// </summary>
    public static class CacheableContentProvider<TAsset> where TAsset : AssetBase, new()
    {
        private static readonly ConcurrentDictionary<string, Resource> _resources;

        /// <summary>
        /// Determines if this content provider can download content.
        /// </summary>
        /// <remarks>
        /// If the engine is in server mode, or <see cref="RenderSettings.GraphicsMode" /> is set to
        /// <see cref="GraphicsMode.NoGraphics" />, it will not download certain types of assets.
        /// </remarks>
        /// <seealso cref="Engine.Mode" />
        // ReSharper disable once StaticMemberInGenericType
        internal static bool CanDownload = true;

        static CacheableContentProvider()
        {
            _resources = new ConcurrentDictionary<string, Resource>();

            var assetType = typeof(TAsset);

            // prevent non-graphics app from downloading unnecessary assets.
            if (RenderSettings.GraphicsMode == GraphicsMode.NoGraphics)
            {
                if (assetType == typeof(Texture) || assetType == typeof(Material) || assetType == typeof(Cubemap) || assetType == typeof(AnimationData))
                    CanDownload = false;
            }

            if (Engine.Mode == EngineMode.Server) // prevent server from downloading unnecessary assets.
            {
                if (assetType == typeof(AudioData))
                    CanDownload = false;
            }
        }

        /// <summary>
        /// Clears the resource cache.
        /// </summary>
        public static void ClearResources()
        {
            _resources.Clear();
        }

        private static void RemoveResource(Resource resource)
        {
            _resources.TryRemove(resource.Id);
        }

        /// <summary>
        /// Deserializes the given stream into an asset.
        /// </summary>
        private static TAsset GetAssetFromStream(Stream stream)
        {
            if (stream == null) return null;
            var asset = new TAsset();
            asset.Load(stream);
            return asset;
        }

        /// <summary>
        /// Redownloads the resource for the given content ID.
        /// </summary>
        /// <param name="contentId"></param>
        public static void RefreshResource(string contentId)
        {
            Resource resource;
            if (_resources.TryGetValue(contentId, out resource))
            {
                resource.Download();
            }
        }

        /// <summary>
        /// Returns a reference without waiting for the resource to load.
        /// </summary>
        internal static Reference<TAsset> GetAsync(string contentId)
        {
            Resource resource;
            if (!_resources.TryGetValue(contentId, out resource))
            {
                _resources[contentId] = resource = new Resource(contentId);
            }
            return new Reference<TAsset>(resource);
        }

        /// <summary>
        /// Returns a reference after waiting for the resource to load.
        /// </summary>
        internal static Reference<TAsset> Get(string contentId)
        {
            var reference = GetAsync(contentId);
            reference.Resource.WaitAsync().Wait();
            return reference;
        }


        internal static void Cache(string contentId, Stream stream)
        {
            _resources[contentId] = new Resource(contentId);
        }

        internal class Resource : IDisposable
        {
            private bool _disposed;
            private TaskCompletionSource<bool> _downloadCompletionSource;
            private int _referenceCount;

            /// <summary>
            /// Creates a new resource.
            /// </summary>
            /// <param name="contentId">The content id url.</param>
            public Resource(string contentId)
            {
                Id = contentId;
                Download();
            }

            public string Id { get; }
            public TAsset Content { get; private set; }

            public int ReferenceCount
            {
                get { return _referenceCount; }
                set
                {
                    if (value == _referenceCount) return;
                    _referenceCount = value;
                    if (value == 0)
                        Dispose(true);
                }
            }

            public bool IsDownloaded { get; private set; }

            /// <summary>
            /// Disposes of the resource.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public event Action Downloaded;

            ~Resource()
            {
                Dispose(false);
            }

            /// <summary>
            /// Waits for the resource to be downloaded.
            /// </summary>
            public async Task WaitAsync()
            {
                await _downloadCompletionSource.Task.ConfigureAwait(false);
            }

            public static implicit operator TAsset(Resource resource)
            {
                return resource?.Content;
            }

            /// <summary>
            /// Disposes of the object.
            /// </summary>
            /// <param name="disposing">True if Dispose() was invoked, False if destructor was invoked.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    Content?.Dispose();
                }

                Content = null;
                ReferenceCount = 0;
                RemoveResource(this);

                _disposed = true;
            }

            public void Download()
            {
                if (!CanDownload)
                    return;

                IsDownloaded = false;

                _downloadCompletionSource = new TaskCompletionSource<bool>();
                ContentProvider.DownloadStream(Id)
                    .ContinueWith(t =>
                    {
                        Content = GetAssetFromStream(t.Result);
                        _downloadCompletionSource.SetResult(true);
                        IsDownloaded = true;
                        Downloaded?.Invoke();
                    }, TaskContinuationOptions.None);
            }
        }
    }

    /// <summary>
    /// A reference to a resource.
    /// </summary>
    public class Reference<TAsset> : IDisposable where TAsset : AssetBase, new()
    {
        private bool _disposed;

        internal Reference(CacheableContentProvider<TAsset>.Resource resource)
        {
            Resource = resource;
            resource.ReferenceCount++;
        }

        internal CacheableContentProvider<TAsset>.Resource Resource { get; }

        /// <summary>
        /// Decrements the reference count.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary />
        ~Reference()
        {
            Dispose(false);
        }

        /// <summary>
        /// Implicitly returns the content object.
        /// </summary>
        public static implicit operator TAsset(Reference<TAsset> reference)
        {
            return reference?.Resource?.Content;
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing">True if Dispose() was invoked, False if destructor was invoked.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Resource.ReferenceCount--;

            _disposed = true;
        }
    }
}