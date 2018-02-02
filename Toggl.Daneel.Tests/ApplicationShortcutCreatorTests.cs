using System;
using FluentAssertions;
using Xunit;

namespace Toggl.Daneel.Tests
{
    public sealed class ApplicationShortcutCreatorTests
    {
        public sealed class TheConstructor
        {
            [Fact]
            public void ThrowsIfTheArgumentIsNull()
            {
                UIKit.UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    Action tryingToConstructWithEmptyParameters =
                    () => new ApplicationShortcutCreator(null);

                    tryingToConstructWithEmptyParameters
                        .ShouldThrow<ArgumentOutOfRangeException>(); 
                });
            }
        }
    }
}
