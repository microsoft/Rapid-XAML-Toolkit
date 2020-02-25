﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Language.Xml;
using RapidXaml;

namespace RapidXamlToolkit.XamlAnalysis
{
    // This is internal as also linked into the CustomAnalysis project.
    internal static class RapidXamlElementExtractor
    {
        // This doesn't have the extra input checking that is in the caller in the TestHelper.
        // Extra checks excluded here as input is already known good based on original doc parsing.
        public static RapidXamlElement GetElement(string xamlElement)
        {
            RapidXamlElement GetElement(string xaml, int startOffset)
            {
                var docSyntax = Parser.ParseText(xaml);

                var xdoc = docSyntax?.RootSyntax;

                if (xdoc == null)
                {
                    return null;
                }

                var elementName = xdoc.Name;

                var result = RapidXamlElement.Build(elementName, startOffset + docSyntax.SpanStart, docSyntax.Width);

                var content = (docSyntax.Body as IXmlElement).Value;

                foreach (var attr in xdoc.Attributes)
                {
                    result.AddAttribute(attr.Name, attr.Value);
                }

                foreach (var child in docSyntax.Body.ChildNodes)
                {
                    if (child == null | child is XmlElementStartTagSyntax | child is XmlElementEndTagSyntax)
                    {
                        continue;
                    }

                    if (child is XmlElementSyntax childElement)
                    {
                        if (childElement.Name.StartsWith($"{elementName}."))
                        {
                            var fullspan = childElement.Content.FullSpan;
                            var attrString = xaml.Substring(fullspan.Start, fullspan.Length);

                            if (attrString.TrimStart().StartsWith("<"))
                            {
                                var startingWhiteSpaceLength = attrString.IndexOf("<");

                                foreach (var innerChild in childElement.ChildNodes)
                                {
                                    if (innerChild == null | innerChild is XmlElementStartTagSyntax | innerChild is XmlElementEndTagSyntax)
                                    {
                                        continue;
                                    }

                                    if (innerChild is SyntaxList childList)
                                    {
                                        foreach (SyntaxNode listItem in childList.ChildNodes)
                                        {
                                            var listItemString = xaml.Substring(listItem.SpanStart, listItem.Width);

                                            result.AddAttribute(
                                                childElement.Name.Substring(elementName.Length + 1),
                                                GetElement(listItemString, startOffset + startingWhiteSpaceLength + listItem.Start));
                                        }
                                    }
                                    else
                                    {
                                        var innerString = xaml.Substring(innerChild.SpanStart, innerChild.Width);

                                        result.AddAttribute(
                                            childElement.Name.Substring(elementName.Length + 1),
                                            GetElement(innerString, startOffset + startingWhiteSpaceLength + innerChild.Start));
                                    }
                                }
                            }
                            else
                            {
                                result.AddAttribute(
                                    childElement.Name.Substring(elementName.Length + 1),
                                    attrString);
                            }

                            var childAsString = xaml.TrimStart().Substring(childElement.Start, childElement.Width);

                            if (content.TrimStart().StartsWith(childAsString.TrimStart()))
                            {
                                content = content.TrimStart().Substring(childAsString.Length).Trim();
                            }
                        }
                        else
                        {
                            result.AddChild(GetElement(content, startOffset + child.Start));
                        }
                    }
                    else if (child is XmlEmptyElementSyntax selfClosingChild)
                    {
                        var toAdd = RapidXamlElement.Build(selfClosingChild.Name, startOffset + child.SpanStart, child.Width);

                        foreach (var attr in selfClosingChild.AttributesNode)
                        {
                            toAdd.AddAttribute(attr.Name, attr.Value);
                        }

                        result.AddChild(toAdd);
                    }
                    else if (child is SyntaxNode node)
                    {
                        foreach (var nodeChild in node.ChildNodes)
                        {
                            if (nodeChild is XmlTextTokenSyntax)
                            {
                                continue;
                            }

                            if (nodeChild is XmlElementSyntax ncElement)
                            {
                                if (ncElement.Name.StartsWith($"{elementName}."))
                                {
                                    var attrString = xaml.Substring(ncElement.Content.Span.Start, ncElement.Content.Span.Length);

                                    if (attrString.StartsWith("<"))
                                    {
                                        result.AddAttribute(
                                            ncElement.Name.Substring(elementName.Length + 1),
                                            GetElement(attrString, startOffset + ncElement.Content.Span.Start));
                                    }
                                    else
                                    {
                                        result.AddAttribute(
                                            ncElement.Name.Substring(elementName.Length + 1),
                                            attrString);
                                    }

                                    var childAsString = xaml.Substring(ncElement.Start, ncElement.Width);

                                    if (content.TrimStart().StartsWith(childAsString.TrimStart()))
                                    {
                                        content = content.TrimStart().Substring(childAsString.Length).Trim();
                                    }
                                }
                                else
                                {
                                    result.AddChild(GetElement(xaml.Substring(nodeChild.SpanStart, nodeChild.Width), startOffset + nodeChild.SpanStart));
                                }
                            }
                            else if (nodeChild is XmlEmptyElementSyntax ncSelfClosing)
                            {
                                var nodeToAdd = RapidXamlElement.Build(ncSelfClosing.Name, startOffset + nodeChild.SpanStart, nodeChild.Width);

                                foreach (var attr in ncSelfClosing.AttributesNode)
                                {
                                    nodeToAdd.AddAttribute(attr.Name, attr.Value);
                                }

                                result.AddChild(nodeToAdd);
                            }
                        }
                    }
                }

                result.SetContent(content);

                return result;
            }

            //// TODO: Cache these responses to avoid unnecessary repeated parsing
            return GetElement(xamlElement, 0);
        }
    }
}