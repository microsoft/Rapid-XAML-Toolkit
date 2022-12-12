﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using RapidXamlToolkit.Resources;
using RapidXamlToolkit.VisualStudioIntegration;

namespace RapidXamlToolkit.XamlAnalysis.Processors
{
    public class PasswordBoxProcessor : XamlElementProcessor
    {
        public PasswordBoxProcessor(ProcessorEssentials essentials)
            : base(essentials)
        {
        }

        public override void Process(string fileName, int offset, string xamlElement, string linePadding, ITextSnapshotAbstraction snapshot, TagList tags, List<TagSuppression> suppressions = null, Dictionary<string, string> xlmns = null)
        {
            if (!this.ProjectType.Matches(ProjectType.Uwp))
            {
                return;
            }

            var (uidExists, uid) = this.GetOrGenerateUid(xamlElement, Attributes.Header);

            this.CheckForHardCodedAttribute(
                fileName,
                Elements.PasswordBox,
                Attributes.Header,
                AttributeType.InlineOrElement,
                StringRes.UI_XamlAnalysisHardcodedStringPasswordboxHeaderMessage,
                xamlElement,
                snapshot,
                offset,
                uidExists,
                uid,
                Guid.Empty,
                tags,
                suppressions,
                this.ProjectType);

            this.CheckForHardCodedAttribute(
                fileName,
                Elements.PasswordBox,
                Attributes.Description,
                AttributeType.InlineOrElement,
                StringRes.UI_XamlAnalysisHardcodedStringPasswordboxDescriptionMessage,
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
