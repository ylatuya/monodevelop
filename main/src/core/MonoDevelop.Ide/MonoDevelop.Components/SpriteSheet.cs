//
// SpriteSheet.cs
//
// Author:
//       lluis <>
//
// Copyright (c) 2015 lluis
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Xwt.Drawing;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Gtk;

namespace Xwt.Drawing
{
	public class SpriteSheet
	{
		Dictionary<string,SpriteInfo> sprites = new Dictionary<string, SpriteInfo> ();
		Image spriteSheet;
		string imgFile;

		public SpriteSheet ()
		{
		}

		public void RegisterSprite (string name, Rectangle rect, Point offset = default(Point), Size originalSize = default(Size))
		{
			sprites [name] = new SpriteInfo {
				Rect = rect,
				Offset = offset,
				OriginalSize = originalSize
			};
		}

		public void Load (string file, SpriteSheetFileFormat format)
		{
			XDocument doc = XDocument.Load (file);
			imgFile = (string) doc.Root.Attribute ("imagePath");

			foreach (var sp in doc.Elements ("sprite")) {
				var s = new SpriteInfo {
					Rect = new Rectangle () {
						X = int.Parse ((string)sp.Attribute("x")),
						Y = int.Parse ((string)sp.Attribute("y")),
						Width = int.Parse ((string)sp.Attribute("w")),
						Height = int.Parse ((string)sp.Attribute("h"))
					},
					Offset = new Point {
						X = int.Parse ((string)sp.Attribute("oX")),
						Y = int.Parse ((string)sp.Attribute("oY"))
					},
					OriginalSize = new Size {
						Width = int.Parse ((string)sp.Attribute("oW")),
						Height = int.Parse ((string)sp.Attribute("oH"))
					}
				};
				sprites [(string)sp.Attribute("n")] = s;
			}
		}

//		public void Load (Stream stream, SpriteSheetFileFormat format)
//		{
//		}

		public Image GetImage (string name)
		{
			if (spriteSheet == null)
				spriteSheet = LoadSpriteSheetImage ();
			SpriteInfo sprite;
			if (sprites.TryGetValue (name, out sprite)) {
				if (sprite.Image == null)
					sprite.Image = new SpriteSheetImage {
						SpriteInfo = sprite,
						Image = spriteSheet
					};
				return sprite.Image;
			}
			return null;
		}

		protected virtual Image LoadSpriteSheetImage ()
		{
			if (imgFile != null)
				return Image.FromFile (imgFile);
			throw new InvalidOperationException ();
		}
	}

	class SpriteInfo
	{
		public Rectangle Rect { get; set; }
		public Point Offset { get; set; }
		public Size OriginalSize { get; set; }
		public SpriteSheetImage Image { get; set; }
	}

	class SpriteSheetImage: DrawingImage
	{
		public SpriteInfo SpriteInfo { get; set; }
		public Image Image { get; set; }
		public double Scale = 2d;

		protected override void OnDraw (Context ctx, Rectangle bounds)
		{
			var rx = bounds.Width / ((double)SpriteInfo.OriginalSize.Width / Scale);
			var ry = bounds.Height / ((double)SpriteInfo.OriginalSize.Height / Scale);
			bounds = bounds.Offset (SpriteInfo.Offset.X * rx, SpriteInfo.Offset.Y * ry);
			bounds.Height = SpriteInfo.Rect.Width * rx;
			bounds.Width = SpriteInfo.Rect.Height * ry;
			ctx.DrawImage (Image, SpriteInfo.Rect, bounds);
		}
	}

	public enum SpriteSheetFileFormat
	{
		TexturePackerXml,
		TexturePackerJSon
	}
}

