// ContentProvider.cs - dEngine
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Assimp;
using dEngine.Data;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using Dynamitey;
using Neo.IronLua;

using Steamworks;

namespace dEngine.Services
{
    /// <summary>
    /// A service for managing content.
    /// </summary>
    [TypeId(8), ExplorerOrder(-1)]
    public partial class ContentProvider : Service
    {
        /// <summary />
        public ContentProvider()
        {
            Service = this;
        }

        /// <summary>
        /// If an item exceeds this size, it will not be cached.
        /// </summary>
        [InstMember(1)]
        public int CacheMaxItemSize
        {
            get { return (int)_cacheMaxItemSize; }
            set { _cacheMaxItemSize = value; }
        }

        /// <summary>
        /// Sets the client ID for the Soundcloud API.
        /// </summary>
        [ScriptSecurity(ScriptIdentity.CoreScript | ScriptIdentity.Editor)]
        public void SetSoundcloudClientId(string clientId)
        {
            ScriptService.AssertIdentity(ScriptIdentity.CoreScript | ScriptIdentity.Editor);
            SoundcloudClientId = clientId;
        }

        /// <summary>
        /// Yields untill all the given content urls have been downloaded.
        /// </summary>
        /// <param name="contentUrls">A table of content URLs to download.</param>
        [YieldFunction]
        public void PreloadAsync(LuaTable contentUrls)
        {
            var thread = ScriptService.CurrentThread;
            ScriptService.YieldThread();

            var count = contentUrls.Length;
            var tasks = new Task[count];

            for (int i = 0; i < count; i++)
            {
                var contentUrl = (string)contentUrls[count];
                tasks[i] = DownloadStream(contentUrl).ContinueWith(t => Cache(contentUrl, t));
            }

            Task.WhenAll(tasks).ContinueWith(t => ScriptService.ResumeThread(thread));
        }


        /// <summary>
        /// Preloads content from the given url.
        /// </summary>
        public void Preload(string contentUrl)
        {
            DownloadStream(contentUrl)
                .ContinueWith(x => Cache(contentUrl, x), TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public partial class ContentProvider
    {
        internal const int TimeoutMiliseconds = 30 * 1000;

        /// <summary>Image Format string for file dialogs.</summary>
        public const string SupportedImageFormats =
            "Supported Formats (*.png;*.tiff;*.bmp*.jpg;*.jpeg;*.gif)|*.png;*.tiff;*.bmp*.jpg;*.jpeg;*.gif|All files (*.*)|*.*";

        /// <summary>Audio Format string for file dialogs.</summary>
        public const string SupportedAudioFormats =
            "Supported Formats (*.mp3;*.mpeg3;*.wav;*.wave;*.flac;*.fla;*.aiff;*aif;*.aifc;*.aac;*.adt;*.adts;*.m2ts;*.mp2;*.3g2;*.3gp2;*.m4a;*.mp4v;*.mp4v;*.mp4;*.mov;*.asf;*.wm;*.wmv;*.wma;*.mp1;*.avi*.ac3;*.ec3;*.cue;*.m3u;*.pls)|*.mp3;*.mpeg3;*.wav;*.wave;*.flac;*.fla;*.aiff;*aif;*.aifc;*.aac;*.adt;*.adts;*.m2ts;*.mp2;*.3g2;*.3gp2;*.m4a;*.mp4v;*.mp4v;*.mp4;*.mov;*.asf;*.wm;*.wmv;*.wma;*.mp1;*.avi*.ac3;*.ec3;*.cue;*.m3u;*.pls;|All files (*.*)|*.*";

        /// <summary>RenderMesh Format string for file dialogs.</summary>
        public const string SupportedMeshFormats =
            "Supported Formats (*.3d;*.3ds;*.ac;*.ac3d;*.acc;*.ase;*.ask;*.b3d;*.blend;*.bvh;*.cob;*.csm;*.dae;*.dxf;*.enff;*.fbx;*.hmp;*.ifc;*.ifczip;*.irr;*.irrmesh;*.lwo;*.lws;*.lxo;*.md2;*.md3;*.md5anim;*.md5camera;*.md5mesh;*.mdc;*.mdl;*.mesh;*.RenderMesh.xml;*.mot;*.ms3d;*.ndo;*.nff;*.obj;*.off;*.pk3;*.ply;*.prj;*.q3o;*.q3s;*.raw;*.scn;*.smd;*.stl;*.ter;*.uc;*.vta;*.x;*.xgl;*.xml;*.zgl;)|*.3d;*.3ds;*.ac;*.ac3d;*.acc;*.ase;*.ask;*.b3d;*.blend;*.bvh;*.cob;*.csm;*.dae;*.dxf;*.enff;*.fbx;*.hmp;*.ifc;*.ifczip;*.irr;*.irrmesh;*.lwo;*.lws;*.lxo;*.md2;*.md3;*.md5anim;*.md5camera;*.md5mesh;*.mdc;*.mdl;*.RenderMesh;*.RenderMesh.xml;*.mot;*.ms3d;*.ndo;*.nff;*.obj;*.off;*.pk3;*.ply;*.prj;*.q3o;*.q3s;*.raw;*.scn;*.smd;*.stl;*.ter;*.uc;*.vta;*.task;*.xgl;*.xml;*.zgl;|All files (*.*)|*.*";

        internal static ContentProvider Service;

        private static readonly IEnumerable<Type> _assetTypes = typeof(AssetBase).GetDescendantTypes();
        private static long _cacheMaxItemSize;

        static ContentProvider()
        {
            LoggerInternal = LogService.GetLogger();
            AssimpContext = new AssimpContext();
        }

        /// <summary>
        /// Allows the host application to add support for custom content uri protocols.
        /// </summary>
        [LevelEditorRelated]
        public static Func<string, string, Stream> CustomFetchHandler { get; set; }

        internal static ILogger LoggerInternal { get; }
        internal static AssimpContext AssimpContext { get; }

        internal static string SoundcloudClientId { get; set; }

        internal static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        private static void Cache(string contentUrl, Task<Stream> task)
        {
            var stream = task.Result;
            var contentType = Inst.PeekContent(task.Result);

            switch (contentType)
            {
                case ContentType.StaticMesh:
                case ContentType.SkeletalMesh:
                    CacheableContentProvider<Geometry>.Cache(contentUrl, stream);
                    break;
                case ContentType.Video:
                case ContentType.Texture:
                    CacheableContentProvider<Texture>.Cache(contentUrl, stream);
                    break;
                case ContentType.Sound:
                    CacheableContentProvider<AudioData>.Cache(contentUrl, stream);
                    break;
                case ContentType.Animation:
                    CacheableContentProvider<AnimationData>.Cache(contentUrl, stream);
                    break;
                case ContentType.Cubemap:
                    CacheableContentProvider<Cubemap>.Cache(contentUrl, stream);
                    break;
            }
        }
        
        /// <summary>
        /// Returns a stream of data from the given content id.
        /// </summary>
        /// <param name="contentId">The content id url.</param>
        internal static string DownloadString(string contentId)
        {
            return DownloadStream(contentId).Result.ReadString();
        }

        /// <summary>
        /// Returns a stream of data from the given content id.
        /// </summary>
        /// <param name="contentId">The content id url.</param>
        internal static async Task<Stream> DownloadStream(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return null;

            return await Task.Factory.StartNew(() =>
            {
                var slashes = contentId.IndexOf("://", StringComparison.Ordinal);
                var scheme = slashes < 0 ? "file" : contentId.Substring(0, slashes);
                var path = slashes < 0 ? contentId : contentId.Substring(slashes + 3);
                var segments = contentId.Split(new[] {"://", "/"}, StringSplitOptions.RemoveEmptyEntries);

                Stream data;

                try
                {
                    switch (scheme)
                    {
                        case "internal":
                            data = GetEmbeddedFile(path);
                            break;
                        case "temp":
                            data = File.OpenRead(Path.Combine(Engine.TempPath, path));
                            break;
                        case "file":
                            data = File.Open(path, FileMode.Open);
                            break;
                        case "desktop":
                            data =
                                File.Open(
                                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), path),
                                    FileMode.Open);
                            break;
                        case "https":
                        case "http":
                            data = HttpService.Get(contentId).Result;
                            break;
                        case "rbxassetid":
                            data = HttpService.Get("http://www.roblox.com/asset/?id=" + path).Result;
                            break;
                        case "rbxasset":
                            throw new NotSupportedException("The rbxasset content protocol is not supported.");
                        case "avatar":
                            var size = (AvatarSize)Enum.Parse(typeof(AvatarSize), segments[2].UppercaseFirst());
                            data = SocialService.GetAvatar(size,
                                segments[1] == "0"
                                    ? LoginService.SteamId
                                    : new CSteamID(new AccountID_t(uint.Parse(segments[1])), EUniverse.k_EUniversePublic,
                                        EAccountType.k_EAccountTypeIndividual));
                            break;
                        case "soundcloud":
                            var trackId = int.Parse(segments[1]);
                            var track = SoundService.Service.GetSoundcloudTrackInfo(trackId);
                            if ((bool)track["downloadable"])
                            {
                                var url = track["download_url"] + "?client_id={ContentProvider.GetSoundcloudClientId()}";
                                data = HttpService.Get(url).Result;
                            }
                            else if ((bool)track["streamable"])
                            {
                                var url = track["stream_url"] + "?client_id={ContentProvider.GetSoundcloudClientId()}";
                                data = HttpService.Get(url).Result;
                            }
                            else
                            {
                                throw new InvalidDataException(
                                    $"Cannot download or stream soundcloud track {trackId} ({track["title"]})");
                            }
                            break;
                        default:
                            if ((data = CustomFetchHandler?.Invoke(scheme, path)) == null)
                                throw new NotSupportedException($"Unknown content protocol \"{scheme}\"");
                            break;
                    }

                    if (data != null)
                    {
                        var mem = data as MemoryStream;

                        if (mem == null) // if returned data was not a MemoryStream, replace it with one.
                        {
                            using (data)
                            {
                                mem = new MemoryStream((int)data.Length);
                                data.Position = 0;
                                data.CopyTo(mem);
                            }
                            data = mem;
                        }

                        data.Position = 0;
                    }
                }
                catch (Exception e)
                {
                    LoggerInternal.Error($"Content fetch failed for \"{contentId}\": {e.Message}");
                    return null;
                }

                return data;
            }).ConfigureAwait(false);
        }

        internal static object GetExisting()
        {
            return DataModel.GetService<ContentProvider>();
        }

        /// <summary>
        /// Forces a cached resource to be redownloaded.
        /// </summary>
        /// <param name="resourceName"></param>
        public void RefreshResource(string resourceName)
        {
            var genericType = typeof(CacheableContentProvider<>);
            foreach (var del in from assetType in _assetTypes
                where !assetType.IsAbstract
                select genericType.MakeGenericType(assetType)
                into type
                select type.GetMethod("RefreshResource")
                into method
                select method.CreateDelegate(typeof(Action<string>)))
            {
                del.FastDynamicInvoke(resourceName);
            }
        }

        /// <summary>
        /// Returns true if the given format is a supported mesh format.
        /// </summary>
        public static bool IsMeshImportFormatSupported(string format)
        {
            return AssimpContext.IsImportFormatSupported(format);
        }

        private static Stream GetEmbeddedFile(string resource)
        {
            resource = resource.Replace("/", "."); // remove leading slash if exists
            resource = resource.Replace("content.", "");
            resource = resource.ToLower();
            var assembly = Assembly.GetExecutingAssembly();

            var resourceNames = assembly.GetManifestResourceNames();

            return (from name in resourceNames
                let matchName = name.ToLower().Replace("dengine.content.", "")
                where matchName == resource
                select assembly.GetManifestResourceStream(name)).FirstOrDefault();
        }

        internal static bool ValidateUri(string uri)
        {
            Uri _;
            return Uri.TryCreate(uri, UriKind.Absolute, out _);
        }
    }
}