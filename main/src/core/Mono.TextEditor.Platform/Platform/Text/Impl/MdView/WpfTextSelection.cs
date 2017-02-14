// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
    using Microsoft.VisualStudio.Text.Formatting;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Text.Utilities;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Describes the selection element used to render selections
    /// </summary>
    internal class WpfTextSelection : ITextSelection
    {
        #region Private Members

        bool _activationTracksFocus;
        readonly IWpfTextView _wpfTextView;
        VirtualSnapshotPoint _activePoint;
        VirtualSnapshotPoint _anchorPoint;

        readonly GuardedOperations _guardedOperations;

        TextSelectionMode _selectionMode;

        #endregion // Private Members

        /// <summary>
        /// Constructs a new selection Element
        /// </summary>
        public WpfTextSelection(IWpfTextView wpfTextView, GuardedOperations guardedOperations)
        {
            // Verify
            Debug.Assert(wpfTextView != null);
            _wpfTextView = wpfTextView;
            ActivationTracksFocus = true;

            // Initialize members
            _activePoint = _anchorPoint = new VirtualSnapshotPoint(_wpfTextView.TextSnapshot, 0);

            _selectionMode = TextSelectionMode.Stream;

            _guardedOperations = guardedOperations;

            SubscribeToEvents();
        }

        public bool ActivationTracksFocus
        {
            set
            {
                if (_activationTracksFocus != value)
                {
                    _activationTracksFocus = value;

                    if (_activationTracksFocus)
                        IsActive = _wpfTextView.HasAggregateFocus;
                }
            }
            get
            {
                return _activationTracksFocus;
            }
        }

        public bool IsActive
        {
            get; set;
        }

        public IWpfTextView WpfTextView
        {
            get { return _wpfTextView; }
        }

        #region ITextSelection Members

        public ITextView TextView
        {
            get
            {
                return _wpfTextView;
            }
        }

        public void Select(SnapshotSpan selectionSpan, bool isReversed)
        {
            VirtualSnapshotPoint start = new VirtualSnapshotPoint(selectionSpan.Start);
            VirtualSnapshotPoint end = new VirtualSnapshotPoint(selectionSpan.End);

            if (isReversed)
            {
                this.Select(end, start);
            }
            else
            {
                this.Select(start, end);
            }
        }

        public void Select(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint)
        {
            if (anchorPoint.Position.Snapshot != _wpfTextView.TextSnapshot)
            {
                throw new ArgumentException("Invalid snapshot for anchorPoint");
            }
            if (activePoint.Position.Snapshot != _wpfTextView.TextSnapshot)
            {
                throw new ArgumentException("Invalid snapshot for activePoint");
            }

            if (anchorPoint == activePoint)
            {
                //For an empty selection, don't worry about text elements (since we expose the caret position
                //which handles the problem for us).
                this.Clear(false);
            }
            else
            {
                var newAnchorPoint = this.NormalizePoint(anchorPoint);
                var newActivePoint = this.NormalizePoint(activePoint);

                if (newAnchorPoint == newActivePoint)
                {
                    this.Clear(false);
                }
                else
                {
                    this.InnerSelect(newAnchorPoint, newActivePoint);
                }
            }
        }

        private void InnerSelect(VirtualSnapshotPoint anchorPoint, VirtualSnapshotPoint activePoint)
        {
            Debug.Assert(anchorPoint != activePoint);
            bool selectionEmptyBeforeChange = this.IsEmpty;

            this.ActivationTracksFocus = true;

            _anchorPoint = anchorPoint;
            _activePoint = activePoint;

            VirtualSnapshotPoint start = _anchorPoint;
            VirtualSnapshotPoint end = _activePoint;
            if (_anchorPoint > _activePoint)
            {
                start = _activePoint;
                end = _anchorPoint;
            }

            this.RaiseChangedEvent(emptyBefore: selectionEmptyBeforeChange, emptyAfter: this.IsEmpty, moved: true);
        }

        private void Clear(bool resetMode)
        {
            bool selectionEmptyBeforeChange = this.IsEmpty;

            //Move the anchor point to the active point (creating a zero-length selection).
            _anchorPoint = _activePoint;

            this.ActivationTracksFocus = true;

            if (resetMode)
                this.Mode = TextSelectionMode.Stream;


            this.RaiseChangedEvent(emptyBefore: selectionEmptyBeforeChange, emptyAfter: true, moved: false);
        }

        public void Clear()
        {
            this.Clear(true);
        }

        public NormalizedSnapshotSpanCollection SelectedSpans
        {
            get
            {
                var selectedSpans = new NormalizedSnapshotSpanCollection(new SnapshotSpan(this.Start.Position, this.End.Position));
                return selectedSpans;
            }
        }

        public ReadOnlyCollection<VirtualSnapshotSpan> VirtualSelectedSpans
        {
            get
            {
                var virtualSelectedSpans = new List<VirtualSnapshotSpan>();
                virtualSelectedSpans.Add(new VirtualSnapshotSpan(this.Start, this.End));
                return new ReadOnlyCollection<VirtualSnapshotSpan>(virtualSelectedSpans);
            }
        }

        public VirtualSnapshotSpan StreamSelectionSpan
        {
            get
            {
                return new VirtualSnapshotSpan(this.Start, this.End);
            }
        }

        public VirtualSnapshotSpan? GetSelectionOnTextViewLine(ITextViewLine line)
        {
            throw new NotImplementedException();
        }

        public TextSelectionMode Mode
        {
            get
            {
                return _selectionMode;
            }
            set
            {
                if (value != TextSelectionMode.Stream)
                {
                    throw new ArgumentException("Only TextSelectionMode.Stream selection is supported");
                }

                if (_selectionMode != value)
                {
                    _selectionMode = value;

                    if (!this.IsEmpty)
                    {
                        // Re-select the existing anchor->active (we don't need to do this if selection was empty).
                        this.Select(this.AnchorPoint, this.ActivePoint);
                    }
                }
            }
        }

        public bool IsReversed
        {
            get
            {
                return (_activePoint < _anchorPoint);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (_activePoint == _anchorPoint);
            }
        }

        public VirtualSnapshotPoint ActivePoint
        {
            get { return this.IsEmpty ? _wpfTextView.Caret.Position.VirtualBufferPosition : _activePoint; }
        }

        public VirtualSnapshotPoint AnchorPoint
        {
            get { return this.IsEmpty ? _wpfTextView.Caret.Position.VirtualBufferPosition : _anchorPoint; }
        }

        public VirtualSnapshotPoint Start { get { return this.IsReversed ? this.ActivePoint : this.AnchorPoint; } }

        public VirtualSnapshotPoint End { get { return this.IsReversed ? this.AnchorPoint : this.ActivePoint; } }

        public event EventHandler SelectionChanged;

        #endregion // ITextSelection Members

        #region Private Helpers

        internal void RaiseChangedEvent(bool emptyBefore, bool emptyAfter, bool moved)
        {
            if (moved || !(emptyBefore && emptyAfter))
            {
                // Inform listeners of this change
                _guardedOperations.RaiseEvent(this, SelectionChanged);
            }
        }

        private VirtualSnapshotPoint NormalizePoint(VirtualSnapshotPoint point)
        {
            ITextSnapshotLine line = point.Position.GetContainingLine();
            if (point.Position.Position > line.End.Position)
            {
                // We're past the end of the line, which means were between a \r\n, which is invalid.
                return new VirtualSnapshotPoint(line.End, point.VirtualSpaces);
            }
            else if ((line.End.Position == point.Position.Position) || (point.VirtualSpaces == 0))
            {
                // If we're at the end of the line then point is valid and we can return it.
                return point;
            }

            return new VirtualSnapshotPoint(point.Position, 0);
        }

        /// <summary>
        /// Subscribes to interesting events(those that might cause the selection to be redrawn or when the view closes)
        /// </summary>
        private void SubscribeToEvents()
        {
            // Sign up for events that might trigger a redraw of our selection geometries
            _wpfTextView.Options.OptionChanged += OnEditorOptionChanged;

            // When the view is closed unsubscribe from the format map changed event and editor option changed event
            _wpfTextView.Closed += OnViewClosed;
        }

        /// <summary>
        /// Unsubscribes from all event subscriptions
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            _wpfTextView.Options.OptionChanged -= OnEditorOptionChanged;

            _wpfTextView.Closed -= OnViewClosed;
        }

        /// <summary>
        /// Event Handler: Text View's Layout Changed event
        /// </summary>
        internal void LayoutChanged(bool visualSnapshotChange, ITextSnapshot newEditSnapshot)
        {
            if (this.IsEmpty)
            {
                //For an empty selection, bring the active and anchor points to the new snapshot (so we don't pin the old one)
                //but any old point in the new snapshot will do.
                _activePoint = new VirtualSnapshotPoint(newEditSnapshot, 0);
                _anchorPoint = _activePoint;
            }
            else if (visualSnapshotChange)
            {
                //Even though the selection may not have changed the snapshots for these are out of date. Delete and let them be lazily recreated.
                var newActivePoint = _activePoint.TranslateTo(newEditSnapshot);
                var newAnchorPoint = _anchorPoint.TranslateTo(newEditSnapshot);

                var normalizedAnchorPoint = this.NormalizePoint(newAnchorPoint);
                var normalizedActivePoint = this.NormalizePoint(newActivePoint);

                if (normalizedActivePoint == normalizedAnchorPoint)
                {
                    //Something happened to collapse the selection (perhaps both endpoints were contained in an outlining region that was collapsed).
                    //Treat this as clearing the selection.
                    this.Clear(false);
                    return;
                }
                else if ((normalizedActivePoint != newActivePoint) || (normalizedAnchorPoint != newAnchorPoint) || (this.Mode == TextSelectionMode.Box))
                {
                    //Something happened to move one endpoint of the selection (outlining region collapsed?).
                    //Treat this as setting the selection (but use InnerSelect since we have the properly normalized endpoints).
                    //
                    //For box selection, we always assume a layout caused something to change (since a classification change could cause the x-coordinate
                    //of one of the endpoints to change, which would change the entire selection). Trying to determine if that is really the case is
                    //expensive (since you need to check each line of the selection). Instead, we pretend it changes so we'll always redraw a box selection
                    //(but the cost there is proportional to the number of visible lines, not selected lines) and hope none of the consumers of the
                    //selection change event do anything expensive.
                    this.InnerSelect(normalizedAnchorPoint, normalizedActivePoint);
                    return;
                }
                else
                {
                    //The selection didn't "change" but the endpoints need to be brought current with the new snapshot.
                    _anchorPoint = normalizedAnchorPoint;
                    _activePoint = normalizedActivePoint;
                }
            }
        }

        #endregion // Private Helpers

        #region Event Subscriptions

        /// <summary>
        /// Fired when the view is closed.
        /// </summary>
        private void OnViewClosed(object sender, EventArgs e)
        {
            UnsubscribeFromEvents();
        }

        void OnEditorOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            // If virtual space was just turned off, reselect and remove
            // virtual space
            if (e.OptionId == DefaultTextViewOptions.UseVirtualSpaceId.Name)
            {
                if (!(_wpfTextView.Options.IsVirtualSpaceEnabled() || (this.Mode == TextSelectionMode.Box)))
                {
                    VirtualSnapshotPoint newAnchorPoint, newActivePoint;
                    newAnchorPoint = new VirtualSnapshotPoint(AnchorPoint.Position);
                    newActivePoint = new VirtualSnapshotPoint(_wpfTextView.Caret.Position.BufferPosition);

                    // This may send out a change event, which is expected.
                    this.Select(newAnchorPoint, newActivePoint);
                }
            }
        }

        #endregion // Event Subscriptions

    }
}