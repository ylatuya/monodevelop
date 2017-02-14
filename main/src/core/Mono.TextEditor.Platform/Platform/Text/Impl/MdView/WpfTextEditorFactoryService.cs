// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;

    using System.ComponentModel;
    using Microsoft.VisualStudio.Platform;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Formatting;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Text.Outlining;
    using Microsoft.VisualStudio.Text.Projection;
    using Microsoft.VisualStudio.Text.Utilities;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Provides a VisualStudio Service that aids in creation of Editor Views
    /// </summary>
    [Export(typeof(ITextEditorFactoryService))]
    internal sealed class WpfTextEditorFactoryService : ITextEditorFactoryService
    {
        [Import]
        internal IEditorOperationsFactoryService EditorOperationsProvider { get; set; }

        [Import]
        internal IBufferGraphFactoryService BufferGraphFactoryService { get; set; }

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        internal GuardedOperations GuardedOperations { get; set; }

        [Import]
        internal IEditorOptionsFactoryService EditorOptionsFactoryService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [ImportMany(typeof(IWpfTextViewCreationListener))]
        internal List<Lazy<IWpfTextViewCreationListener, IDeferrableContentTypeAndTextViewRoleMetadata>> TextViewCreationListeners { get; set; }

        [ImportMany(typeof(IWpfTextViewConnectionListener))]
        internal List<Lazy<IWpfTextViewConnectionListener, IContentTypeAndTextViewRoleMetadata>> TextViewConnectionListeners { get; set; }

        public event EventHandler<TextViewCreatedEventArgs> TextViewCreated;

        public IWpfTextView CreateTextView(MonoDevelop.Ide.Editor.TextEditor textEditor, ITextViewRoleSet roles)
        {
            var textDataModel = new VacuousTextDataModel(textEditor.GetPlatformTextBuffer());
            var textViewModel = new VacuousTextViewModel(textDataModel);

            WpfTextView editor = new WpfTextView(textEditor, textViewModel, roles ?? this.DefaultRoles, this);

            this.TextViewCreated?.Invoke(this, new TextViewCreatedEventArgs(editor));

            return editor;
        }

        public ITextViewRoleSet NoRoles
        {
            get { return new TextViewRoleSet(new string[0]); }
        }

        public ITextViewRoleSet AllPredefinedRoles
        {
            get { return CreateTextViewRoleSet(PredefinedTextViewRoles.Analyzable, 
                                               PredefinedTextViewRoles.Debuggable,
                                               PredefinedTextViewRoles.Document,
                                               PredefinedTextViewRoles.Editable,
                                               PredefinedTextViewRoles.Interactive,
                                               PredefinedTextViewRoles.Structured,
                                               PredefinedTextViewRoles.Zoomable,
                                               PredefinedTextViewRoles.PrimaryDocument); }
        }

        public ITextViewRoleSet DefaultRoles
        {
            // notice that Debuggable, PrimaryDocument and Zoomable are excluded!
            get
            {
                return CreateTextViewRoleSet(PredefinedTextViewRoles.Analyzable,
                                             PredefinedTextViewRoles.Document,
                                             PredefinedTextViewRoles.Editable,
                                             PredefinedTextViewRoles.Interactive,
                                             PredefinedTextViewRoles.Structured);
            }
        }

        public ITextViewRoleSet CreateTextViewRoleSet(IEnumerable<string> roles)
        {
            return new TextViewRoleSet(roles);
        }

        public ITextViewRoleSet CreateTextViewRoleSet(params string[] roles)
        {
            return new TextViewRoleSet(roles);
        }
    }

#if TARGET_VS
    public interface IAdornmentLayersMetadata : IOrderable
    {
        [DefaultValue(false)]
        bool IsOverlayLayer { get; }
    }
#endif
}
