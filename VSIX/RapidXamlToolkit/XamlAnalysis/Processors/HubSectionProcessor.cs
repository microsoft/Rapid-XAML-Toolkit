﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using RapidXamlToolkit.Resources;
using RapidXamlToolkit.XamlAnalysis.Actions;
using RapidXamlToolkit.XamlAnalysis.Tags;

namespace RapidXamlToolkit.XamlAnalysis.Processors
{
    public class HubSectionProcessor : XamlElementProcessor
    {
        public override void Process(string fileName, int offset, string xamlElement, string linePadding, ITextSnapshot snapshot, List<IRapidXamlAdornmentTag> tags)
        {
            var (uidExists, uid) = this.GetOrGenerateUid(xamlElement, Attributes.Header);

            this.CheckForHardCodedAttribute(
                fileName,
                Elements.HubSection,
                Attributes.Header,
                AttributeType.InlineOrElement,
                StringRes.Info_XamlAnalysisHardcodedStringHubSectionHeaderMessage,
                xamlElement,
                snapshot,
                offset,
                uidExists,
                uid,
                Guid.Empty,
                tags);
        }
    }
}
