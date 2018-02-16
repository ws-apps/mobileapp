﻿using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Suggestions;
using Xunit;

namespace Toggl.Foundation.Tests.Autocomplete
{
    public sealed class QueryInfoTests
    {
        public sealed class TheParseQueryMethod
        {
            public sealed class None
            {
                [Theory, LogIfTooSlow]
                [InlineData("")]
                [InlineData(null)]
                public void DoesNotSuggestAnythingWhenTheTextIsEmpty(string text)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, 0);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.None);
                    parsed.Text.Should().Be(String.Empty);
                }

                [Property]
                public void DoesNotSuggestAnythingWhenThereIsOnlyOneCharacter(char letter)
                {
                    if (letter == '#' || letter == '@')
                        return;

                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(letter.ToString(), 1);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.None);
                    parsed.Text.Should().Be(String.Empty);
                }
            }

            public sealed class TimeEntries : AutocompleteProviderTests.AutocompleteProviderTest
            {
                [Property]
                public void ExtractsTheProjectNameWhileTyping(NonEmptyString nonEmptyString)
                {
                    var text = nonEmptyString.Get;
                    if (text.Length < 2 || text.Contains("#") || text.Contains("@"))
                        return;

                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, text.Length);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.TimeEntries);
                    parsed.Text.Should().Be(text);
                }

                [Property]
                public void DoesNotSuggestAnyMoreProjectsWhenSomeProjectIsAlreadySelected(NonEmptyString nonEmptyString)
                {
                    var text = nonEmptyString.Get;
                    if (text.Length < 2 || text.Contains("#"))
                        return;

                    var textFieldInfo = TextFieldInfo.Empty
                        .WithTextAndCursor(text, text.Length)
                        .WithProjectInfo(WorkspaceId, ProjectId, ProjectName, ProjectColor);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.TimeEntries);
                    parsed.Text.Should().Be(text);
                }
            }

            public sealed class Projects : AutocompleteProviderTests.AutocompleteProviderTest
            {
                [Theory, LogIfTooSlow]
                [InlineData("@", "")]
                [InlineData("abcde @", "")]
                [InlineData("@abcde", "abcde")]
                [InlineData("@abcde fgh ijk", "abcde fgh ijk")]
                [InlineData("abcde @fgh ijk", "fgh ijk")]
                [InlineData("abcde #fgh @ijk", "ijk")]
                [InlineData("meeting with someone@gmail.com @meetings", "meetings")]
                public void ExtractsTheProjectNameWhileTyping(string text, string expectedProjectName)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, text.Length);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Projects);
                    parsed.Text.Should().Be(expectedProjectName);
                }

                [Theory, LogIfTooSlow]
                [InlineData("@", 0)]
                [InlineData("abcde @", 3)]
                [InlineData("@abcde", 0)]
                [InlineData("@abcde fgh ijk", 0)]
                [InlineData("abcde @fgh ijk", 5)]
                [InlineData("abcde #fgh @ijk", 8)]
                [InlineData("meeting with someone@gmail.com @meetings", 10)]
                public void DoesNotExtractTheProjectNameWhenCursorIsMovedBeforeTheAtSymbol(string text, int cursorPosition)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, cursorPosition);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().NotBe(AutocompleteSuggestionType.Projects);
                }

                [Theory, LogIfTooSlow]
                [InlineData("@@@", 1, "@@")]
                [InlineData("abcde @fgh @ijk", 8, "fgh @ijk")]
                [InlineData("meeting with @meetings with someone@gmail.com", 20, "meetings with someone@gmail.com")]
                public void ExtractTheProjectNameFromTheFirstAtSymbolPrecedingTheCursor(string text, int cursorPosition, string expectedProjectName)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, cursorPosition);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Projects);
                    parsed.Text.Should().Be(expectedProjectName);
                }

                [Theory, LogIfTooSlow]
                [InlineData("@", "")]
                [InlineData("@@@", "@@")]
                [InlineData("@abc@def", "abc@def")]
                [InlineData("abcde @@fgh", "@fgh")]
                [InlineData("abcde @fgh @ijk", "ijk")]
                [InlineData("abcde @fgh#ijk", "fgh#ijk")]
                [InlineData("meeting with @meetings with someone@gmail.com", "meetings with someone@gmail.com")]
                public void ExtractTheProjectNameFromTheFirstAtSymbolWithPreceededWithAWhiteSpaceOrAtTheVeryBeginningOfTheText(string text, string expectedProjectName)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, text.Length);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Projects);
                    parsed.Text.Should().Be(expectedProjectName);
                }
            }

            public sealed class Tags : AutocompleteProviderTests.AutocompleteProviderTest
            {
                [Theory, LogIfTooSlow]
                [InlineData("#", "")]
                [InlineData("abcde #", "")]
                [InlineData("#abcde", "abcde")]
                [InlineData("#abcde fgh ijk", "abcde fgh ijk")]
                [InlineData("abcde #fgh ijk", "fgh ijk")]
                [InlineData("abcde @fgh #ijk", "ijk")]
                [InlineData("meeting with someone@gmail.com #meetings", "meetings")]
                public void ExtractsTheTagNameWhileTyping(string text, string expectedTagName)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, text.Length);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Tags);
                    parsed.Text.Should().Be(expectedTagName);
                }

                [Theory, LogIfTooSlow]
                [InlineData("#@", "@")]
                [InlineData("#abcde@fgh.ijk", "abcde@fgh.ijk")]
                [InlineData("abcde #fgh @ijk", "fgh @ijk")]
                public void ExtractsTheTagNameIncludingTheAtSymbolsWhenAProjectIsSelected(string text, string expectedTagName)
                {
                    var textFieldInfo = TextFieldInfo.Empty
                        .WithTextAndCursor(text, text.Length)
                        .WithProjectInfo(WorkspaceId, ProjectId, ProjectName, ProjectColor);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Tags);
                    parsed.Text.Should().Be(expectedTagName);
                }

                [Theory, LogIfTooSlow]
                [InlineData("#", "")]
                [InlineData("###", "##")]
                [InlineData("#abc#def", "abc#def")]
                [InlineData("abcde ##fgh", "#fgh")]
                [InlineData("abcde #fgh #ijk", "ijk")]
                [InlineData("meeting with #meetings with someone@gmail.com", "meetings with someone@gmail.com")]
                public void ExtractTheTagtNameFromTheFirstHashSymbolWithPreceededWithAWhiteSpaceOrAtTheVeryBeginningOfTheText(string text, string expectedTagName)
                {
                    var textFieldInfo = TextFieldInfo.Empty.WithTextAndCursor(text, text.Length);

                    var parsed = QueryInfo.ParseFieldInfo(textFieldInfo);

                    parsed.SuggestionType.Should().Be(AutocompleteSuggestionType.Tags);
                    parsed.Text.Should().Be(expectedTagName);
                }
            }
        }
    }
}
