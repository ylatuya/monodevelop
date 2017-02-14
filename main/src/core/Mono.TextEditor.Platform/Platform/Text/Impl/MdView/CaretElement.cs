// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;

    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
    using Microsoft.VisualStudio.Text.Formatting;
    using Microsoft.VisualStudio.Text.Utilities;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Defines the caret that's placed on the text editor
    /// </summary>
    internal class CaretElement : ITextCaret
    {
        VirtualSnapshotPoint _insertionPoint;
        PositionAffinity _caretAffinity;
        WpfTextView _wpfTextView;
        WpfTextSelection _selection;
        GuardedOperations _guardedOperations;

        /// <summary>
        /// Constructs a new selection Element that is bound to the specified editor canvas
        /// </summary>
        /// <param name="wpfTextView">
        /// The WPF Text View that hosts this caret
        /// </param>
        public CaretElement(
                WpfTextView wpfTextView, WpfTextSelection selection,
                GuardedOperations guardedOperations)
        {
            _wpfTextView = wpfTextView;
            _selection = selection;
            _guardedOperations = guardedOperations;

            // Set up initial values
            _caretAffinity = PositionAffinity.Successor;
            _insertionPoint = new VirtualSnapshotPoint(new SnapshotPoint(_wpfTextView.TextSnapshot, 0));
        }

        #region ITextCaret Members

        public void EnsureVisible()
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreferredCoordinates()
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(ITextViewLine textLine)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(ITextViewLine textLine, double xCoordinate, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(VirtualSnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveTo(SnapshotPoint bufferPosition, PositionAffinity caretAffinity, bool captureHorizontalPosition)
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToNextCaretPosition()
        {
            throw new NotImplementedException();
        }

        public CaretPosition MoveToPreviousCaretPosition()
        {
            throw new NotImplementedException();
        }

        public bool InVirtualSpace
        {
            get
            {
                return _insertionPoint.IsInVirtualSpace;
            }
        }

        public bool OverwriteMode
        {
            get; private set;
        }

        public double Left
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Width
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Right
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Top
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Height
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Bottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the caret's current position
        /// </summary>
        public CaretPosition Position
        {
            get
            {
                //In theory the _insertion point is always at the same snapshot at the _wpfTextView but there could be cases
                //where someone is using the position in a classifier that is using the caret position in the classificaiton changed event.
                //In that case return the old insertion point.
                return new CaretPosition(_insertionPoint,
                                         _wpfTextView.BufferGraph.CreateMappingPoint(_insertionPoint.Position, PointTrackingMode.Positive),
                                         _caretAffinity);
            }
        }

        public ITextViewLine ContainingTextViewLine
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double PreferredYCoordinate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsHidden
        {
            get; set;
        }

        /// <summary>
        /// An event that fires whenever the caret's position has been explicitly changed.  The event doesn't 
        /// fire if the caret was tracking normal text edits.
        /// </summary>
        public event EventHandler<CaretPositionChangedEventArgs> PositionChanged;

        #endregion // ITextCaret Members
    }
}
