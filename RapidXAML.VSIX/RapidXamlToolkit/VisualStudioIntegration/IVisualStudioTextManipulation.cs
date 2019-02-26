﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace RapidXamlToolkit.VisualStudioIntegration
{
    public interface IVisualStudioTextManipulation
    {
        void ReplaceInActiveDocOnLine(string find, string replace, int lineNumber);

        // TODO: remove this?
        void ReplaceInActiveDoc(string find, string replace, int startIndex, int endIndex);

        // TODO: see if can/should remove this and use line numbers instead of indexes
        void ReplaceInActiveDoc(List<(string find, string replace)> replacements, int startIndex, int endIndex, Dictionary<int, int> exclusions);

        void InsertIntoActiveDocumentOnNextLine(string text, int pos);

        void InsertAtEndOfLine(int lineNumber, string toInsert);

        void DeleteFromEndOfLine(int lineNumber, int charsToDelete);

        void StartSingleUndoOperation(string name);

        void EndSingleUndoOperation();
    }
}
