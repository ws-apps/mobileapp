using System.Collections.Generic;
using System.Linq;
using Android.Views;

namespace Toggl.Giskard.Extensions
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> GetChildren(this ViewGroup view) 
        {
            for (int i = 0; i < view.ChildCount; i++)
                yield return view.GetChildAt(i);
        }

        public static IEnumerable<T> GetChildren<T>(this ViewGroup view) 
            => view.GetChildren().OfType<T>();
    }
}
