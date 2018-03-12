﻿using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableWorkspaceViewModel : MvxNotifyPropertyChanged
    {
        public long WorkspaceId { get; set; }

        public string WorkspaceName { get; set; }

        public bool Selected { get; set; }

        public SelectableWorkspaceViewModel(IDatabaseWorkspace workspace, bool selected)
        {
            Ensure.Argument.IsNotNull(workspace, nameof(workspace));

            Selected = selected;
            WorkspaceId = workspace.Id;
            WorkspaceName = workspace.Name;
        }
    }
}
