﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.MvvmCross
{
    public sealed class TrackingNavigationService : MvxNavigationService
    {
        private readonly IAnalyticsService analyticsService;

        public TrackingNavigationService(
            IMvxNavigationCache navigationCache,
            IMvxViewModelLoader viewModelLoader,
            IAnalyticsService analyticsService) : base(navigationCache, viewModelLoader)
        {
            this.analyticsService = analyticsService;
        }

        public override Task Navigate(IMvxViewModel viewModel, IMvxBundle presentationBundle = null)
        {
            analyticsService.TrackCurrentPage(viewModel.GetType());
            return base.Navigate(viewModel, presentationBundle);
        }

        public override Task Navigate<TParameter>(IMvxViewModel<TParameter> viewModel, TParameter param, IMvxBundle presentationBundle = null)
        {
            analyticsService.TrackCurrentPage(viewModel.GetType());
            return base.Navigate<TParameter>(viewModel, param, presentationBundle);
        }

        public override Task<TResult> Navigate<TResult>(IMvxViewModelResult<TResult> viewModel, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            analyticsService.TrackCurrentPage(viewModel.GetType());
            return base.Navigate<TResult>(viewModel, presentationBundle, cancellationToken);
        }

        public override Task<TResult> Navigate<TParameter, TResult>(IMvxViewModel<TParameter, TResult> viewModel, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            analyticsService.TrackCurrentPage(viewModel.GetType());
            return base.Navigate<TParameter, TResult>(viewModel, param, presentationBundle, cancellationToken);
        }

        public override Task Navigate(Type viewModelType, IMvxBundle presentationBundle = null)
        {
            analyticsService.TrackCurrentPage(viewModelType);
            return base.Navigate(viewModelType, presentationBundle);
        }

        public override Task Navigate<TParameter>(Type viewModelType, TParameter param, IMvxBundle presentationBundle = null)
        {
            analyticsService.TrackCurrentPage(viewModelType);
            return base.Navigate<TParameter>(viewModelType, param, presentationBundle);
        }

        public override Task<TResult> Navigate<TResult>(Type viewModelType, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            analyticsService.TrackCurrentPage(viewModelType);
            return base.Navigate<TResult>(viewModelType, presentationBundle, cancellationToken);
        }

        public override Task<TResult> Navigate<TParameter, TResult>(Type viewModelType, TParameter param, IMvxBundle presentationBundle = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            analyticsService.TrackCurrentPage(viewModelType);
            return base.Navigate<TParameter, TResult>(viewModelType, param, presentationBundle, cancellationToken);
        }
    }
}
