﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JenkinsStatusViewModel.cs" company="Soloplan GmbH">
//   Copyright (c) Soloplan GmbH. All rights reserved.
//   Licensed under the MIT License. See License-file in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Soloplan.WhatsON.Jenkins.GUI
{
  using System;
  using System.Windows.Input;
  using Soloplan.WhatsON.GUI.Common.BuildServer;

  class JenkinsStatusViewModel : BuildStatusViewModel
  {
    private OpenWebPageCommandData parentCommandData;

    public JenkinsStatusViewModel(JenkinsProjectViewModel model)
      : base(model)
    {
    }

    /// <summary>
    /// Command for opening builds webPage.
    /// </summary>
    public override ICommand OpenBuildPage { get; } = new OpenWebPageCommand();

    public override object OpenBuildPageCommandData
    {
      get
      {
        if (this.parentCommandData == null)
        {
          return null;
        }

        return new OpenWebPageCommandData
        {
          Address = this.parentCommandData.Address + "/" + this.BuildNumber,
          Redirect = this.parentCommandData.Redirect
        };
      }
    }

    public override void Update(Status newStatus)
    {
      base.Update(newStatus);
      var jenkinsStatus = newStatus as JenkinsStatus;
      if (jenkinsStatus == null)
      {
        this.BuildNumber = null;
        return;
      }

      this.BuildNumber = jenkinsStatus.BuildNumber;
      this.Building = jenkinsStatus.Building;
      this.Duration = jenkinsStatus.Duration;
      this.EstimatedDuration = jenkinsStatus.EstimatedDuration;

      if (this.State == ObservationState.Running)
      {
        var elapsedSinceStart = (DateTime.Now - this.Time).TotalSeconds;
        this.RawProgres = (int)((100 * elapsedSinceStart) / this.EstimatedDuration.TotalSeconds);
        this.Duration = DateTime.Now - this.Time;
      }
      else
      {
        this.RawProgres = 0;
      }

      this.Culprits.Clear();
      foreach (var culprit in jenkinsStatus.Culprits)
      {
        var culpritModle = new JenkinsCulpritViewModel();
        culpritModle.Init(culprit);
        this.Culprits.Add(culpritModle);
      }

      this.UpdateCalculatedFields();
    }

    public void SetJobAddress(OpenWebPageCommandData parentData)
    {
      this.parentCommandData = parentData;
      this.OnPropertyChanged(nameof(this.OpenBuildPageCommandData));
    }
  }
}
