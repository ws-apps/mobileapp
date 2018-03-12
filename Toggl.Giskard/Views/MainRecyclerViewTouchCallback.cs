using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Toggl.Giskard.Extensions;
using static Android.Support.V7.Widget.RecyclerView;

namespace Toggl.Giskard.Views
{
    public sealed class MainRecyclerViewTouchCallback : ItemTouchHelper.SimpleCallback
    {
        private readonly Drawable background;
        private readonly Drawable continueDrawable;
        private readonly int continueDrawableMargin;
        private readonly MainRecyclerView recyclerView;

        public MainRecyclerViewTouchCallback(Context context, MainRecyclerView recyclerView)
            : base(0, ItemTouchHelper.Left)
        {
            this.recyclerView = recyclerView;

            var color = new Color(ContextCompat.GetColor(context, Resource.Color.playButtonGreen));
            background = new ColorDrawable(color);
            continueDrawable = ContextCompat.GetDrawable(context, Resource.Drawable.play_white);
            continueDrawableMargin = (int)16.DpToPixels(context);
        }

        public override int GetSwipeDirs(RecyclerView recyclerView, ViewHolder viewHolder)
        {
            if (viewHolder is MainRecyclerViewLogViewHolder timeEntriesLogViewHolder && timeEntriesLogViewHolder.CanSync)
                return base.GetSwipeDirs(recyclerView, viewHolder);

            return 0;
        }

        public override bool OnMove(RecyclerView recyclerView, ViewHolder viewHolder, ViewHolder target)
            => false;

        public override void OnSwiped(ViewHolder viewHolder, int direction)
        {
            var swipedPosition = viewHolder.AdapterPosition;
            recyclerView.MainRecyclerAdapter.ContinueTimeEntry(swipedPosition);
            recyclerView.MainRecyclerAdapter.NotifyItemChanged(swipedPosition);
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView,
                                         ViewHolder viewHolder,
                                         float dX, float dY,
                                         int actionState, bool isCurrentlyActive)
        {
            var isInvalidIndex = viewHolder.AdapterPosition == -1;
            var isTimeEntryCell = viewHolder is MainRecyclerViewLogViewHolder;
            if (isInvalidIndex || !isTimeEntryCell) return;

            var itemView = viewHolder.ItemView;

            background.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
            background.Draw(c);

            int itemHeight = itemView.Bottom - itemView.Top;
            int intrinsicWidth = continueDrawable.IntrinsicWidth;
            int intrinsicHeight = continueDrawable.IntrinsicWidth;

            int iconLeft = itemView.Right - continueDrawableMargin - intrinsicWidth;
            int iconRight = itemView.Right - continueDrawableMargin;
            int icontTop = itemView.Top + (itemHeight - intrinsicHeight) / 2;
            int iconBottom = icontTop + intrinsicHeight;
            continueDrawable.SetBounds(iconLeft, icontTop, iconRight, iconBottom);

            continueDrawable.Draw(c);

            base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }
    }
}
