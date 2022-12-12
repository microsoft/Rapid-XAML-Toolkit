﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using RapidXamlToolkit.Resources;
using RapidXamlToolkit.VisualStudioIntegration;

namespace RapidXamlToolkit.XamlAnalysis.Processors
{
    public class TextBlockProcessor : XamlElementProcessor
    {
        public TextBlockProcessor(ProcessorEssentials essentials)
            : base(essentials)
        {
        }

        public override void Process(string fileName, int offset, string xamlElement, string linePadding, ITextSnapshotAbstraction snapshot, TagList tags, List<TagSuppression> suppressions = null, Dictionary<string, string> xlmns = null)
        {
            if (this.ProjectType == ProjectType.XamarinForms || this.ProjectType == ProjectType.MAUI)
            {
                return;
            }

            var (uidExists, uid) = this.GetOrGenerateUid(xamlElement, Attributes.Text);

            this.CheckForHardCodedAttribute(
                fileName,
                Elements.TextBlock,
                Attributes.Text,
                AttributeType.Any,
                StringRes.UI_XamlAnalysisHardcodedStringTextblockTextMessage,
                xamlElement,
                snapshot,
                offset,
                uidExists,
                uid,
                Guid.Empty,
                tags,
                suppressions,
                this.ProjectType);
        }
    }
}
