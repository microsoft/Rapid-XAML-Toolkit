﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using RapidXamlToolkit.ErrorList;

namespace RapidXamlToolkit.XamlAnalysis
{
    public class RapidXamlTagger : ITagger<IErrorTag>
    {
        private ITextBuffer _buffer;
        private string _file;

        public RapidXamlTagger(ITextBuffer buffer, string file)
        {
            _buffer = buffer;
            _file = file;

            RapidXamlDocumentCache.Parsed += this.OnXamlDocParsed;

            // Docs may have already been parsed when we get here (will happen if document was opened with the project) so process what has been parsed
            this.OnXamlDocParsed(this, new RapidXamlParsingEventArgs(null, _file, _buffer.CurrentSnapshot, ParsedAction.Unknown));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // This handles adding and removing things from the error list
        private void OnXamlDocParsed(object sender, RapidXamlParsingEventArgs e)
        {
            var visibleErrors = RapidXamlDocumentCache.ErrorListTags(_file);

            var result = new FileErrorCollection { Project = "a-project", FilePath = _file };

            foreach (var viewTag in visibleErrors)
            {
                result.Errors.Add(viewTag.AsErrorRow());
            }

            ErrorListService.Process(result);

            // As the tags that are shown in the error list might have changed, trigger that to be updated too.
            if (e != null)
            {
                var span = new SnapshotSpan(e.Snapshot, 0, e.Snapshot.Length);
                this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }

        // This creates the underlying tags that are shown on the designer.
        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var errors = RapidXamlDocumentCache.AdornmentTags(_file);

            foreach (var viewTag in errors)
            {
                foreach (var span in spans)
                {
                    if (span.IntersectsWith(viewTag.Span))
                    {
                        yield return viewTag.AsErrorTag();
                    }
                }
            }
        }
    }
}