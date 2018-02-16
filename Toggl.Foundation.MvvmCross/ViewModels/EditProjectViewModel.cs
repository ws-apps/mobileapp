﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.UI;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using static Toggl.Foundation.Helper.Constants;
using static Toggl.Multivac.Extensions.StringExtensions;
using Toggl.Multivac.Extensions;
using Toggl.Foundation.MvvmCross.Services;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class EditProjectViewModel : MvxViewModel<string, long?>
    {
        private readonly Random random = new Random();
        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IMvxNavigationService navigationService;

        private bool isPro;
        private long? clientId;
        private long workspaceId;
        private long initialWorkspaceId;
        private HashSet<string> projectNames = new HashSet<string>();

        public bool IsPrivate { get; set; }

        public bool IsNameAlreadyTaken { get; set; }

        [DependsOn(nameof(Name), nameof(IsNameAlreadyTaken))]
        public bool SaveEnabled => 
            !IsNameAlreadyTaken
            && !string.IsNullOrWhiteSpace(Name) 
            && Name.LengthInBytes() <= MaxProjectNameLengthInBytes;

        public string Name { get; set; } = "";

        public string TrimmedName => Name.Trim();

        public MvxColor Color { get; set; }

        public string Title { get; private set; } = "";

        public string ClientName { get; private set; } = "";

        public string WorkspaceName { get; private set; } = "";
        
        public string DoneButtonText { get; private set; } = "";

        public IMvxAsyncCommand DoneCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand PickClientCommand { get; }

        public IMvxAsyncCommand PickColorCommand { get; }

        public IMvxAsyncCommand PickWorkspaceCommand { get; }

        public IMvxCommand TogglePrivateProjectCommand { get; }

        public EditProjectViewModel(
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            DoneCommand = new MvxAsyncCommand(done);
            CloseCommand = new MvxAsyncCommand(close);
            PickColorCommand = new MvxAsyncCommand(pickColor);
            PickClientCommand = new MvxAsyncCommand(pickClient);
            PickWorkspaceCommand = new MvxAsyncCommand(pickWorkspace);
            TogglePrivateProjectCommand = new MvxCommand(togglePrivateProject);
        }

        public override void Prepare(string parameter)
        {
            Name = parameter;
            Title = Resources.NewProject;
            DoneButtonText = Resources.Create;
            pickRandomColor();
        }

        public override async Task Initialize()
        {
            var workspace = await dataSource.Workspaces.GetDefault();
            isPro = await dataSource.Workspaces.WorkspaceHasFeature(workspace.Id, WorkspaceFeatureId.Pro);
            workspaceId = initialWorkspaceId = workspace.Id;
            WorkspaceName = workspace.Name;

            await setupNameAlreadyTakenError();
        }

        private async Task setupNameAlreadyTakenError() 
        {
            var existingProjectNames = await dataSource.Projects
                                        .GetAll(project => project.WorkspaceId == workspaceId)
                                        .Select(projects => projects.Select(p => p.Name));

            projectNames.Clear();
            projectNames.AddRange(existingProjectNames);

            IsNameAlreadyTaken = projectNames.Contains(TrimmedName);
        }

        private void OnNameChanged() 
            => IsNameAlreadyTaken = projectNames.Contains(TrimmedName);

        private async Task pickColor()
        {
            Color = await navigationService.Navigate<SelectColorViewModel, ColorParameters, MvxColor>(
                ColorParameters.Create(Color, isPro));
        }

        private void togglePrivateProject()
        {
            IsPrivate = !IsPrivate;
        }

        private async Task done()
        {
            if (!SaveEnabled) return;

            if (initialWorkspaceId != workspaceId)
            {
                var shouldContinue = await dialogService.Confirm(
                    Resources.WorkspaceChangedAlertTitle,
                    Resources.WorkspaceChangedAlertMessage,
                    Resources.Ok,
                    Resources.Cancel
                );

                if (!shouldContinue) return;
            }

            var workspace = 
                await (isPro 
                ? dataSource.Workspaces.GetById(workspaceId) 
                : Observable.Return(default(IDatabaseWorkspace)));
            
            var createdProject = await dataSource.Projects.Create(new CreateProjectDTO
            {
                Name = TrimmedName,
                Color = $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}",
                IsPrivate = IsPrivate,
                ClientId = clientId,
                Billable = workspace?.ProjectsBillableByDefault,
                WorkspaceId = workspaceId
            });

            await navigationService.Close(this, createdProject.Id);
        }

        private Task close()
            => navigationService.Close(this, null);

        private async Task pickWorkspace()
        {
            var parameters = WorkspaceParameters.Create(workspaceId, Resources.Workspaces, allowQuerying: false);
            var selectedWorkspaceId = 
                await navigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(parameters);

            if (selectedWorkspaceId == workspaceId) return;

            var workspace = await dataSource.Workspaces.GetById(selectedWorkspaceId);

            clientId = null;
            ClientName = "";
            workspaceId = selectedWorkspaceId;
            WorkspaceName = workspace.Name;

            await setupNameAlreadyTakenError();

            isPro = await dataSource.Workspaces.WorkspaceHasFeature(workspaceId, WorkspaceFeatureId.Pro);
            if (isPro || Array.IndexOf(Helper.Color.DefaultProjectColors, Color) >= 0) return;

            pickRandomColor();
        }
        
        private async Task pickClient()
        {
            var parameter = SelectClientParameters.WithIds(workspaceId, clientId);
            var selectedClientId = 
                await navigationService.Navigate<SelectClientViewModel, SelectClientParameters, long?>(parameter);
            if (selectedClientId == null) return;

            if (selectedClientId.Value == 0)
            {
                selectedClientId = null;
                ClientName = "";
                return;
            }

            var client = await dataSource.Clients.GetById(selectedClientId.Value);
            clientId = client.Id;
            ClientName = client.Name;
        }
        
        private void pickRandomColor()
        {
            var randomColorIndex = random.Next(0, Helper.Color.DefaultProjectColors.Length);
            Color = Helper.Color.DefaultProjectColors[randomColorIndex];
        }
    }
}
