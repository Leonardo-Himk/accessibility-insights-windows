﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.CommonUxComponents.Dialogs;
using AccessibilityInsights.Extensions.Helpers;
using AccessibilityInsights.Extensions.Interfaces.IssueReporting;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace AccessibilityInsights.Extensions.GitHub
{
    /// <summary>
    /// GitHub Issue Reprting
    /// </summary>
    [Export(typeof(IIssueReporting))]
    public class IssueReporter : IIssueReporting
    {
        private IssueReporter _instance;
        private ConfigurationModel configurationControl;

        private IssueReporter()
        {
            configurationControl = new ConfigurationModel();
            configurationControl.IsConfigured = SetIsConfigured;
            configurationControl.Config = new ConnectionConfiguration(string.Empty);
        }

        private void SetIsConfigured(bool isConfigured)
        {
            this.IsConfigured = isConfigured;
        }

        public IssueReporter GetDefaultInstance()
        {
            if (_instance == null)
            {
                _instance = new IssueReporter();
            }
            return _instance;
        }

        public string ServiceName => Properties.Resources.extensionName;

        public Guid StableIdentifier => new Guid ("bbdf3582-d4a6-4b76-93ea-ef508d1fd4b8");
        public bool IsConfigured { get; private set; } = false;

        public ReporterFabricIcon Logo => ReporterFabricIcon.GitHubLogo;

        public string LogoText => Properties.Resources.extensionName;

        public IssueConfigurationControl ConfigurationControl => configurationControl;

        public bool CanAttachFiles => false;

        public Task<IIssueResult> FileIssueAsync(IssueInformation issueInfo)
        {
            return Task.Run<IIssueResult>(() => FileIssueAsyncAction(issueInfo));
        }

        private IIssueResult FileIssueAsyncAction(IssueInformation issueInfo)
        {
            if (this.IsConfigured)
            {
                try
                {
                    string url = IssueFormatterFactory.GetNewIssueLink(this.configurationControl.Config.RepoLink, issueInfo);
                    System.Diagnostics.Process.Start(url);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    e.ReportException();
                    MessageDialog.Invoke(Properties.Resources.InvalidLink);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            return null;
        }

        public Task RestoreConfigurationAsync(string serializedConfig)
        {
            return Task.Run(() => RestoreConfigurationAsyncAction(serializedConfig));
        }

        private void RestoreConfigurationAsyncAction(string serializedConfig)
        {
            ConnectionConfiguration config = JsonConvert.DeserializeObject<ConnectionConfiguration>(serializedConfig);
            if (config!=null && !string.IsNullOrEmpty(config.RepoLink) && LinkValidator.IsValidGitHubRepoLink(config.RepoLink))
            {
                this.configurationControl.Config = config;
                this.IsConfigured = true;
            }
            else
            {
                this.configurationControl.Config = new ConnectionConfiguration(string.Empty);
                this.IsConfigured = false;
                MessageDialog.Show(Properties.Resources.InvalidLink);
            }
        }

        public IssueConfigurationControl RetrieveConfigurationControl(Action UpdateSaveButton)
        {
            this.ConfigurationControl.UpdateSaveButton = UpdateSaveButton;
            return ConfigurationControl;
        }
    }
}
