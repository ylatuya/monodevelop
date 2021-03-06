//
// TextSegmentMarker.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
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
using System.Text;
using Cairo;
using Mono.TextEditor.Highlighting;
using System.Collections.Generic;
using MonoDevelop.Core.Text;
using MonoDevelop.Components;

namespace Mono.TextEditor
{
	class TextSegmentMarker : TreeSegment
	{
		internal int insertId;
		public virtual TextLineMarkerFlags Flags {
			get;
			set;
		}
		
		
		bool isVisible = true;
		public virtual bool IsVisible {
			get { return isVisible; }
			set { isVisible = value; }
		}
		
		public TextSegmentMarker (int offset, int length) : base (offset, length)
		{
		}

		public TextSegmentMarker (ISegment textSegment) : base (textSegment)
		{
		}
		
		public virtual void Draw (MonoTextEditor editor, Context cr, LineMetrics metrics, int startOffset, int endOffset)
		{
		}

		public virtual void DrawBackground (MonoTextEditor editor, Context cr, LineMetrics metrics, int startOffset, int endOffset)
		{
		}
		
		internal virtual MonoDevelop.Ide.Editor.Highlighting.ChunkStyle GetStyle (MonoDevelop.Ide.Editor.Highlighting.ChunkStyle baseStyle)
		{
			return baseStyle;
		}
	}

	internal interface IChunkMarker
	{
		void TransformChunks (List<MonoDevelop.Ide.Editor.Highlighting.ColoredSegment> chunks);

		void ChangeForeColor (MonoTextEditor editor, MonoDevelop.Ide.Editor.Highlighting.ColoredSegment chunk, ref Cairo.Color color);
	}

	class UnderlineTextSegmentMarker : TextSegmentMarker
	{
		public UnderlineTextSegmentMarker (Cairo.Color color, ISegment textSegment) : base (textSegment)
		{
			this.Color = color;
			this.Wave = true;
		}

		public UnderlineTextSegmentMarker (string colorName, ISegment textSegment) : base (textSegment)
		{
			this.ColorName = colorName;
			this.Wave = true;
		}

		public string ColorName { get; set; }
		public Cairo.Color Color { get; set; }
		public bool Wave { get; set; }
		
		public override void Draw (MonoTextEditor editor, Cairo.Context cr, LineMetrics metrics, int startOffset, int endOffset)
		{
			int markerStart = Segment.Offset;
			int markerEnd = Segment.EndOffset;
			if (markerEnd < startOffset || markerStart > endOffset) 
				return; 
			var layout = metrics.Layout.Layout;
			double startXPos = metrics.TextRenderStartPosition;
			double endXPos = metrics.TextRenderEndPosition;
			double y = metrics.LineYRenderStartPosition;
			if (editor.IsSomethingSelected) {
				var range = editor.SelectionRange;
				if (range.Contains (markerStart)) {
					int end = System.Math.Min (markerEnd, range.EndOffset);
					InternalDraw (markerStart, end, editor, cr, metrics, true, startOffset, endOffset, y, startXPos, endXPos);
					InternalDraw (range.EndOffset, markerEnd, editor, cr, metrics, false, startOffset, endOffset, y, startXPos, endXPos);
					return;
				}
				if (range.Contains (markerEnd)) {
					InternalDraw (markerStart, range.Offset, editor, cr, metrics, false, startOffset, endOffset, y, startXPos, endXPos);
					InternalDraw (range.Offset, markerEnd, editor, cr, metrics, true, startOffset, endOffset, y, startXPos, endXPos);
					return;
				}
				if (markerStart <= range.Offset && range.EndOffset <= markerEnd) {
					InternalDraw (markerStart, range.Offset, editor, cr, metrics, false, startOffset, endOffset, y, startXPos, endXPos);
					InternalDraw (range.Offset, range.EndOffset, editor, cr, metrics, true, startOffset, endOffset, y, startXPos, endXPos);
					InternalDraw (range.EndOffset, markerEnd, editor, cr, metrics, false, startOffset, endOffset, y, startXPos, endXPos);
					return;
				}
				
			}
			
			InternalDraw (markerStart, markerEnd, editor, cr, metrics, false, startOffset, endOffset, y, startXPos, endXPos);
		}
		
		void InternalDraw (int markerStart, int markerEnd, MonoTextEditor editor, Cairo.Context cr, LineMetrics metrics, bool selected, int startOffset, int endOffset, double y, double startXPos, double endXPos)
		{
			if (markerStart > markerEnd)
				return;
			var layout = metrics.Layout.Layout;
			double @from;
			double to;
			if (markerStart < startOffset && endOffset < markerEnd) {
				@from = startXPos;
				to = endXPos;
			} else {
				int start = startOffset < markerStart ? markerStart : startOffset;
				int end = endOffset < markerEnd ? endOffset : markerEnd;
				int /*lineNr,*/ x_pos;
				uint curIndex = 0;
				var byteIndex = metrics.Layout.TranslateToUTF8Index ((uint)(start - startOffset), ref curIndex);
				
				x_pos = layout.IndexToPos (System.Math.Max (0, (int)byteIndex)).X;
				@from = startXPos + (int)(x_pos / Pango.Scale.PangoScale);

				byteIndex = metrics.Layout.TranslateToUTF8Index ((uint)(end - startOffset), ref curIndex);
				
				x_pos = layout.IndexToPos (System.Math.Max (0, (int)byteIndex)).X;
				
				to = startXPos + (int)(x_pos / Pango.Scale.PangoScale);
				var line = editor.GetLineByOffset (endOffset);
				if (markerEnd > endOffset || @from == to) {
					to += editor.TextViewMargin.CharWidth;
					if (@from >= to) {
						@from = to - editor.TextViewMargin.CharWidth;
					}
				}
			}
			@from = System.Math.Max (@from, editor.TextViewMargin.XOffset);
			to = System.Math.Max (to, editor.TextViewMargin.XOffset);
			if (@from >= to) {
				return;
			}
			double height = editor.LineHeight / 5;
			// TODO : EditorTheme does that look ok ?

			// if (selected) {
			//	cr.SetSourceColor (editor.EditorTheme.SelectedText.Foreground);
			// } else 

			if (ColorName == null) {
				cr.SetSourceColor (Color);
			} else {
				HslColor color;
				editor.EditorTheme.TryGetColor (ColorName, out color);
				cr.SetSourceColor (color);
			}
			// }
			if (Wave) {	
				Pango.CairoHelper.ShowErrorUnderline (cr, @from, y + editor.LineHeight - height, to - @from, height);
			} else {
				cr.LineWidth = 1;
				cr.MoveTo (@from, y + editor.LineHeight - 1.5);
				cr.LineTo (to, y + editor.LineHeight - 1.5);
				cr.Stroke ();
			}
		}
	}
}
