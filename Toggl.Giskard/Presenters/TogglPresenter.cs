﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Giskard.Activities;

namespace Toggl.Giskard.Presenters
{
    public sealed class TogglPresenter : MvxAppCompatViewPresenter
    {
        private readonly HashSet<Type> clearBackStackTypes = new HashSet<Type>
        {
            typeof(MainViewModel),
            typeof(OnboardingViewModel)
        };
        
        public TogglPresenter(IEnumerable<Assembly> androidViewAssemblies) 
            : base(androidViewAssemblies)
        {
        }

        protected override Intent CreateIntentForRequest(MvxViewModelRequest request)
        {
            var intent = base.CreateIntentForRequest(request);

            if (clearBackStackTypes.Contains(request.ViewModelType))
            {
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
            }

            return intent;
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            switch (hint)
            {
                case CardVisibilityHint cardHint:
                    if(CurrentActivity is MainActivity mainActivity)
                        mainActivity.OnTimeEntryCardVisibilityChanged(cardHint.Visible);
                    
                    return;

                case ToggleCalendarVisibilityHint calendarHint:
                    return;
            }

            base.ChangePresentation(hint);
        }
    }
}
