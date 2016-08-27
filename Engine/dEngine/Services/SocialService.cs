// SocialService.cs - dEngine
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
using dEngine.Graphics;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using Neo.IronLua;
using NLog;

using SharpDX.WIC;
using Steamworks;

namespace dEngine.Services
{
	/// <summary>
	/// A service for using the social features of Steam.
	/// </summary>
	[TypeId(91), ExplorerOrder(-1)]
	public partial class SocialService : Service
	{
        /// <inheritdoc />
        public SocialService()
		{
		    Service = this;
        }
        
		internal static object GetExisting()
		{
			return DataModel.GetService<SocialService>();
		}
	}

	public partial class SocialService
	{
		private static Logger LoggerInternal = LogManager.GetLogger(nameof(SocialService));
		internal static SocialService Service;
		internal static bool IsInitialized { get; private set; }

		internal static void Init()
		{
		    LoggerInternal = LogManager.GetLogger(nameof(SocialService));

			IsInitialized = true;

            LoggerInternal.Info("SocialService initialized.");
		}

		internal static Stream GetAvatar(AvatarSize avatarSize, CSteamID steamId)
		{
			int avatar;

			switch (avatarSize)
			{
				case AvatarSize.Small:
					avatar = SteamFriends.GetSmallFriendAvatar(steamId);
					break;
				case AvatarSize.Medium:
					avatar = SteamFriends.GetMediumFriendAvatar(steamId);
					break;
				case AvatarSize.Large:
					avatar = SteamFriends.GetLargeFriendAvatar(steamId);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(avatarSize), avatarSize, null);
			}

			uint w;
			uint h;
			bool ret = SteamUtils.GetImageSize(avatar, out w, out h);
			var width = (int)w;
			var height = (int)h;

			if (!ret || width <= 0 || height <= 0)
				throw new InvalidDataException();

			var length = width * height * 4;
			var bytes = new byte[length];
			SteamUtils.GetImageRGBA(avatar, bytes, length);

			var outputStream = new MemoryStream();
			var formatGuid = ContainerFormatGuids.Png;
			var stride = PixelFormat.GetStride(PixelFormat.Format32bppPRGBA, width);

			using (var encoder = new BitmapEncoder(Renderer.ImagingFactory, formatGuid))
			{
				encoder.Initialize(outputStream);

				var bitmapFrameEncode = new BitmapFrameEncode(encoder);
				bitmapFrameEncode.Initialize();
				bitmapFrameEncode.SetSize(width, height);
				bitmapFrameEncode.SetPixelFormat(ref formatGuid);
				bitmapFrameEncode.WritePixels(height, stride, bytes);

				bitmapFrameEncode.Commit();
				encoder.Commit();
			}

			return outputStream;
		}
	}
}