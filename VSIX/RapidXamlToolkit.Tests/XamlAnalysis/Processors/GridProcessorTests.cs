﻿// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RapidXaml;
using RapidXamlToolkit.Tests.XamlAnalysis.CustomAnalyzers;
using RapidXamlToolkit.XamlAnalysis;
using RapidXamlToolkit.XamlAnalysis.Processors;
using RapidXamlToolkit.XamlAnalysis.Tags;

namespace RapidXamlToolkit.Tests.XamlAnalysis.Processors
{
    [TestClass]
    public class GridAnalyzerTests : AnalyzerTestsBase
    {
        [TestMethod]
        public void MissingRowDefinition_NoDefinitions_Detected()
        {
            var xaml = @"<Grid><TextBlock Grid.Row=""1""></Grid>";

            //  var outputTags = this.GetTags<GridProcessor>(xaml);

            //Assert.AreEqual(1, outputTags.OfType<MissingRowDefinitionTag>().Count());

            var actual = this.Act<GridAnalyzer>(xaml, ProjectFramework.Uwp);

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(1, actual.Count(a => a.Action == ActionType.AddAttribute));
            Assert.AreEqual(Attributes.RowDefinitions, actual[0].Name);
            Assert.AreEqual("*,*", actual[0].Value);
        }
    }

    [TestClass]
    public class GridProcessorTests : ProcessorTestsBase
    {
        [TestMethod]
        public void MissingColumnDefinition_NoDefinitions_Detected()
        {
            var xaml = @"<Grid><TextBlock Grid.Column=""1""></Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(1, outputTags.OfType<MissingColumnDefinitionTag>().Count());
        }

        [TestMethod]
        public void MissingRowDefinition_SomeDefinitions_Detected()
        {
            var xaml = @"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition Height=""Auto"" />
    </Grid.RowDefinitions>
    <TextBlock Grid.Row=""4"">
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(1, outputTags.OfType<MissingRowDefinitionTag>().Count());
        }

        [TestMethod]
        public void MissingColumnDefinition_SomeDefinitions_Detected()
        {
            var xaml = @"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""Auto"" />
        <ColumnDefinition Width=""Auto"" />
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Column=""4"">
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(1, outputTags.OfType<MissingColumnDefinitionTag>().Count());
        }

        [TestMethod]
        public void MissingColumnDefinition_InComment_NotDetected()
        {
            var xaml = @"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""Auto"" />
        <ColumnDefinition Width=""Auto"" />
    </Grid.ColumnDefinitions>

    <!--<TextBlock Grid.Column=""4"">-->
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(0, outputTags.OfType<MissingColumnDefinitionTag>().Count());
        }

        [TestMethod]
        public void MissingRowDefinition_InComment_NotDetected()
        {
            var xaml = @"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition Height=""Auto"" />
    </Grid.RowDefinitions>
    <!--<TextBlock Grid.Row=""4"">-->
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(0, outputTags.OfType<MissingRowDefinitionTag>().Count());
        }

        [TestMethod]
        public void RowSpan_OverFlow_Detected()
        {
            var xaml = @"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition Height=""Auto"" />
    </Grid.RowDefinitions>
    <TextBlock Grid.Row=""1"" Grid.RowSpan=""2"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(1, outputTags.OfType<RowSpanOverflowTag>().Count());
        }

        [TestMethod]
        public void RowSpan_OverFlow_IgnoredIfInComment()
        {
            var xaml = @"<Grid>
    <!--<TextBlock Grid.Row=""1"" Grid.RowSpan=""2"" />-->
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(0, outputTags.OfType<RowSpanOverflowTag>().Count());
        }

        [TestMethod]
        public void ColumnSpan_OverFlow_IgnoredIfInComment()
        {
            var xaml = @"<Grid>
    <!--<TextBlock Grid.Row=""1"" Grid.ColumnSpan=""2"" />-->
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(0, outputTags.OfType<ColumnSpanOverflowTag>().Count());
        }

        [TestMethod]
        public void ColumnSpan_OverFlow_Detected()
        {
            var xaml = @"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""Auto"" />
        <ColumnDefinition Width=""Auto"" />
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Column=""1"" Grid.ColumnSpan=""2"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml);

            Assert.AreEqual(1, outputTags.OfType<ColumnSpanOverflowTag>().Count());
        }

        [TestMethod]
        public void DetectConcise_RowDefinitions()
        {
            var xaml = @"<Grid RowDefinitions=""*,*"">
    <Label Grid.Row=""1"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml, ProjectType.XamarinForms);

            Assert.AreEqual(0, outputTags.OfType<MissingRowDefinitionTag>().Count());
        }

        [TestMethod]
        public void DetectConcise_ColumnDefinitions()
        {
            var xaml = @"<Grid ColumnDefinitions=""*,*"">
    <Label Grid.Column=""1"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml, ProjectType.XamarinForms);

            Assert.AreEqual(0, outputTags.OfType<MissingColumnDefinitionTag>().Count());
        }

        [TestMethod]
        public void DetectOutsideConcise_RowDefinitions()
        {
            var xaml = @"<Grid RowDefinitions=""*,*"">
    <Label Grid.Row=""2"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml, ProjectType.XamarinForms);

            Assert.AreEqual(1, outputTags.OfType<MissingRowDefinitionTag>().Count());
        }

        [TestMethod]
        public void DetectOutsideConcise_ColumnDefinitions()
        {
            var xaml = @"<Grid ColumnDefinitions=""*,*"">
    <Label Grid.Column=""2"" />
</Grid>";

            var outputTags = this.GetTags<GridProcessor>(xaml, ProjectType.XamarinForms);

            Assert.AreEqual(1, outputTags.OfType<MissingColumnDefinitionTag>().Count());
        }
    }
}
