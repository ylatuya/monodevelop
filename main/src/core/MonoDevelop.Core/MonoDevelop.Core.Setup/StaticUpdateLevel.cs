//
// StaticUpdateLevel.cs
//
// Author:
//       Qiu <${AuthorEmail}>
//
// Copyright (c) 2014 Qiu
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

namespace MonoDevelop.Core.Setup
{
	public sealed class StaticUpdateLevel: UpdateLevel
	{
		public static readonly StaticUpdateLevel Stable;
		public static readonly StaticUpdateLevel Beta;
		public static readonly StaticUpdateLevel Alpha;
		public static readonly StaticUpdateLevel Test;
		public static readonly StaticUpdateLevel[] DefaultLevels;

		public StaticUpdateLevel () {}

		public StaticUpdateLevel (string name, int idx) : base (name, idx){
		}

		static StaticUpdateLevel() {
			Stable = new StaticUpdateLevel("Stable", 0);
			Beta = new StaticUpdateLevel ("Beta", 1);
			Alpha = new StaticUpdateLevel ("Alpha", 2);
			Test = new StaticUpdateLevel ("Test", 100);
			DefaultLevels = new StaticUpdateLevel[] { Stable, Beta, Alpha };
		}
	}
}

