// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
    using Microsoft.VisualStudio.Text.Formatting;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Text.Projection;
    using Microsoft.VisualStudio.Text.Utilities;
    using Microsoft.VisualStudio.Utilities;

    internal partial class WpfTextView : IWpfTextView
    {
        ITextBuffer _textBuffer;
        ITextSnapshot _textSnapshot;

        ITextBuffer _visualBuffer;
        ITextSnapshot _visualSnapshot;

        IBufferGraph _bufferGraph;
        ITextViewRoleSet _roles;
        ConnectionManager _connectionManager;
        WpfTextEditorFactoryService _factoryService;

        WpfTextSelection _selection;
        private IEditorOptions _editorOptions;
        CaretElement _caretElement;

        private IViewScroller _viewScroller;

        private PropertyCollection _properties = new PropertyCollection();

       private readonly MonoDevelop.Ide.Editor.TextEditor _textEditor;


        public WpfTextView(MonoDevelop.Ide.Editor.TextEditor textEditor, ITextViewModel textViewModel, ITextViewRoleSet roles, WpfTextEditorFactoryService factoryService)
        {
            _roles = roles;
            _factoryService = factoryService;
            this.TextViewModel = textViewModel;

            _textBuffer = textViewModel.EditBuffer;
            _visualBuffer = textViewModel.VisualBuffer;

            _textSnapshot = _textBuffer.CurrentSnapshot;
            _visualSnapshot = _visualBuffer.CurrentSnapshot;

            _editorOptions = _factoryService.EditorOptionsFactoryService.GetOptions(this);

            _bufferGraph = _factoryService.BufferGraphFactoryService.CreateBufferGraph(textViewModel.VisualBuffer);

            // Create selection and make sure it's created before the caret as the caret relies on the selection being
            // available in its constructor
            _selection = new WpfTextSelection(this, _factoryService.GuardedOperations);

            // Create caret
            _caretElement = new CaretElement(this, _selection,
                                             _factoryService.GuardedOperations);

            _properties.AddProperty(typeof(MonoDevelop.Ide.Editor.TextEditor), textEditor);

            this.BindContentTypeSpecificAssets(null, textViewModel.DataModel.ContentType);
        }

        private void BindContentTypeSpecificAssets(IContentType beforeContentType, IContentType afterContentType)
        {
            // Notify the Text view creation listeners
            var extensions = UIExtensionSelector.SelectMatchingExtensions(_factoryService.TextViewCreationListeners, afterContentType, beforeContentType, _roles);
            foreach (var extension in extensions)
            {
                var instantiatedExtension = _factoryService.GuardedOperations.InstantiateExtension(extension, extension);
                if (instantiatedExtension != null)
                {
                    _factoryService.GuardedOperations.CallExtensionPoint(instantiatedExtension,
                                                                         () => instantiatedExtension.TextViewCreated(this));
                }
            }
        }

        public IBufferGraph BufferGraph
        {
            get; private set;
        }

        public ITextCaret Caret
        {
            get
            {
                return _caretElement;
            }
        }

        public bool InLayout
        {
            get; private set;
        }

        public bool IsClosed
        {
            get; private set;
        }

        public IEditorOptions Options
        {
            get { return _editorOptions; }
        }

        public PropertyCollection Properties
        {
            get { return _properties; }
        }

        public ITextViewRoleSet Roles
        {
            get { return _roles; }
        }

        public ITextSelection Selection
        {
            get
            {
                return _selection;
            }
        }

        public ITextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }

        public ITextDataModel TextDataModel
        {
            get { return this.TextViewModel.DataModel; }
        }

        public ITextSnapshot TextSnapshot
        {
            get { return _textSnapshot; }
        }

        public ITextViewModel TextViewModel
        {
            get; private set;
        }

        public IViewScroller ViewScroller
        {
            get { return _viewScroller; }
        }

        public ITextSnapshot VisualSnapshot
        {
            get { return _visualSnapshot; }
        }

        public event EventHandler Closed;
        public event EventHandler GotAggregateFocus;
        public event EventHandler<TextViewLayoutChangedEventArgs> LayoutChanged;
        public event EventHandler LostAggregateFocus;
        public event EventHandler<MouseHoverEventArgs> MouseHover;
        public event EventHandler ViewportHeightChanged;
        public event EventHandler ViewportLeftChanged;
        public event EventHandler ViewportWidthChanged;

        public void Close()
        {
            this.IsClosed = true;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        public bool HasAggregateFocus
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public bool IsMouseOverViewOrAdornments
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double LineHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double MaxTextRightCoordinate
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public ITrackingSpan ProvisionalTextHighlight
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public ITextViewLineCollection TextViewLines
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportBottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportLeft
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportRight
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportTop
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public double ViewportWidth
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public void DisplayTextLineContainingBufferPosition(SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo)
        {
            throw new NotImplementedException();
        }
        public void DisplayTextLineContainingBufferPosition(SnapshotPoint bufferPosition, double verticalDistance, ViewRelativePosition relativeTo, double? viewportWidthOverride, double? viewportHeightOverride)
        {
            throw new NotImplementedException();
        }
        public SnapshotSpan GetTextElementSpan(SnapshotPoint point)
        {
            throw new NotImplementedException();
        }
        public ITextViewLine GetTextViewLineContainingBufferPosition(SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }
        public void QueueSpaceReservationStackRefresh()
        {
            throw new NotImplementedException();
        }
    }
}