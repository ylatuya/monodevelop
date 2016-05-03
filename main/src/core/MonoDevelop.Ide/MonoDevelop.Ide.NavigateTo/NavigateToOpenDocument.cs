//
// NavigateToOpenDocument.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.FindInFiles;
using Xwt;

namespace MonoDevelop.Ide.NavigateTo
{
	public abstract class ScopeSearcher
	{
		internal static List<ScopeSearcher> searchers = new List<ScopeSearcher> ();

		static ScopeSearcher ()
		{
			AddinManager.AddExtensionNodeHandler ("/MonoDevelop/Ide/NavigateToSearcher", delegate (object sender, ExtensionNodeEventArgs args) {
				switch (args.Change) {
				case ExtensionChange.Add:
					searchers.Add ((ScopeSearcher)args.ExtensionObject);
					break;
				}
			});
		}

		public abstract Task<IEnumerable<Components.MainToolbar.SearchResult>> GetSearchResults (Document doc);
	}

	class NavigateToOpenDocument : CommandHandler
	{
		protected override async void Run ()
		{
			Console.WriteLine (1);
			var doc = IdeApp.Workbench.ActiveDocument;
			if (doc?.AnalysisDocument == null)
				return;
			Console.WriteLine (2);
			var root = doc.AnalysisDocument.GetSyntaxRootAsync ();
			if (root == null)
				return;
			Console.WriteLine (3);
			var window = new NavigateToDocumentWindow ();
			window.ShowAll ();
		}
	}


	public class NavigateToDocumentWindow :  Xwt.Window
	{
		SearchTextEntry searchResults;

		public NavigateToDocumentWindow ()
		{
			searchResults = new SearchTextEntry ();
			searchResults.LostFocus += SearchResults_FocusOutEvent;
			searchResults.Changed += SearchResults_StateChanged;
			var topHBox = new HBox ();
			topHBox.PackStart (searchResults); 
			this.Content = topHBox;
			this.WindowPosition = Gtk.WindowPosition.Center;

		}

		void SearchResults_FocusOutEvent (object sender, EventArgs e)
		{
			this.Dispose ();
		}

		void SearchResults_StateChanged (object sender, EventArgs e)
		{
			Console.WriteLine ("changed !!!!");
		}
	}
}