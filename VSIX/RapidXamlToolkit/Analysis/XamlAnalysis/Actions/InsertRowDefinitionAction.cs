﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Text.Editor;
using RapidXamlToolkit.Resources;
using RapidXamlToolkit.VisualStudioIntegration;
using RapidXamlToolkit.XamlAnalysis.Processors;
using RapidXamlToolkit.XamlAnalysis.Tags;

namespace RapidXamlToolkit.XamlAnalysis.Actions
{
    public class InsertRowDefinitionAction : BaseSuggestedAction
    {
        private readonly InsertRowDefinitionTag tag;

        private InsertRowDefinitionAction(string file, InsertRowDefinitionTag tag)
            : base(file)
        {
            this.tag = tag;
            this.DisplayText = StringRes.UI_InsertNewDefinitionForRow.WithParams(this.tag.RowId);
        }

        public List<(string find, string replace)> Replacements { get; private set; }

        public Dictionary<int, int> Exclusions { get; private set; }

        public string PreviewText { get; private set; }

        public override bool HasPreview => true;

        public override ImageMoniker IconMoniker => KnownMonikers.InsertClause;

        public static InsertRowDefinitionAction Create(InsertRowDefinitionTag tag, string file, ITextView view)
        {
            var text = view.TextSnapshot.GetText(tag.GridStartPos, tag.GridLength);
            var replacements = GetReplacements(tag.RowId, tag.RowCount);
            var exclusions = GetExclusions(text);

            var previewText = GetPreviewText(text, replacements, exclusions, tag);

            var result = new InsertRowDefinitionAction(file, tag)
            {
                View = view,
                Replacements = replacements,
                Exclusions = exclusions,
                PreviewText = previewText,
            };

            return result;
        }

        public static Dictionary<int, int> GetExclusions(string xaml)
        {
            // Find other nested grids
            return XamlElementProcessor.GetExclusions(xaml, Elements.Grid);
        }

        public static List<(string find, string replace)> GetReplacements(int rowNumber, int totalRows)
        {
            var result = new List<(string, string)>();

            if (rowNumber > -1)
            {
                // subtract 1 from total rows to allow for zero indexing
                for (int i = totalRows - 1; i >= rowNumber; i--)
                {
                    result.Add(($" Grid.Row=\"{i}\"", $" Grid.Row=\"{i + 1}\""));
                }
            }

            return result;
        }

        public static string GetPreviewText(string original, List<(string find, string replace)> replacements, Dictionary<int, int> exclusions, InsertRowDefinitionTag tag)
        {
            var withReplacements = SwapReplacements(original, replacements, exclusions);

            var insertLineStart = withReplacements.Substring(tag.GridStartPos, tag.InsertPoint - tag.GridStartPos).LastIndexOf('\n') + 1;

            var toInsert = tag.XamlTag + Environment.NewLine + new string(' ', tag.InsertPoint - tag.GridStartPos - insertLineStart);
            var withInsertion = withReplacements.Insert(tag.InsertPoint, toInsert);

            return withInsertion;
        }

        public static string SwapReplacements(string originalXaml, List<(string find, string replace)> replacements, Dictionary<int, int> exclusions = null)
        {
            var result = originalXaml;

            // Have to implement search and replace directly as built-in functionality doesn't provide the control to only replace outside of exclusion areas.
            // Mimics the process in VisualStudioTextManipulation.ReplaceInActiveDoc
            foreach (var (find, replace) in replacements)
            {
                var pos = 0;

                while (true)
                {
                    var next = result.Substring(pos).IndexOf(find, StringComparison.Ordinal);

                    if (next < 0)
                    {
                        break; // while
                    }

                    pos += next;

                    var searchAgain = false;

                    // if in exclusion area then search again
                    if (exclusions != null)
                    {
                        foreach (var exclusion in exclusions)
                        {
                            if (pos >= exclusion.Key && pos <= exclusion.Value)
                            {
                                searchAgain = true;
                                break; // Foreach
                            }
                        }
                    }

                    if (searchAgain)
                    {
                        pos += find.Length;
                    }
                    else
                    {
                        var before = result.Substring(0, pos);
                        var after = result.Substring(pos + find.Length);
                        result = before + replace + after;
                    }
                }
            }

            return result;
        }

        public override Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            var textBlock = new TextBlock
            {
                Padding = new Thickness(5),
            };
            textBlock.Inlines.Add(new Run() { Text = this.PreviewText });

            return Task.FromResult<object>(textBlock);
        }

        public override void Execute(CancellationToken cancellationToken)
        {
            var vs = new VisualStudioTextManipulation(ProjectHelpers.Dte);
            vs.StartSingleUndoOperation(StringRes.UI_UndoContextInsertRowDef);
            try
            {
                vs.ReplaceInActiveDoc(this.Replacements, this.tag.GridStartPos, this.tag.GridStartPos + this.tag.GridLength, this.Exclusions);
                vs.InsertIntoActiveDocumentOnNextLine(this.tag.XamlTag, this.tag.InsertPoint);
            }
            finally
            {
                vs.EndSingleUndoOperation();
            }
        }
    }
}
