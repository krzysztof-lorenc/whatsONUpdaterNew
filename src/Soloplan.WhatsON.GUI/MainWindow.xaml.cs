﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Soloplan GmbH">
//  Copyright (c) Soloplan GmbH. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Soloplan.WhatsON.GUI
{
  using System;
  using System.ComponentModel;
  using System.Windows;
  using Soloplan.WhatsON.GUI.Config.View;
  using Soloplan.WhatsON.Serialization;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    /// <summary>
    /// The configuration.
    /// </summary>
    private ApplicationConfiguration config;

    private ObservationScheduler scheduler;

    protected override void OnClosed(EventArgs e)
    {
      this.scheduler.Stop();
      base.OnClosed(e);
    }

    public MainWindow()
    {
      this.InitializeComponent();
      this.config = SerializationHelper.LoadOrCreateConfiguration();
      this.scheduler = new ObservationScheduler();

      foreach (var subjectConfiguration in this.config.SubjectsConfiguration)
      {
        var subject = PluginsManager.Instance.CreateNewSubject(subjectConfiguration);
        this.scheduler.Observe(subject);
      }

      this.mainTreeView.Init(this.scheduler, this.config);
      this.scheduler.Start();

      var themeHelper = new ThemeHelper();
      themeHelper.Initialize();
      themeHelper.ApplyLightDarkMode(this.config.DarkThemeEnabled);
    }

    private void OpenConfig(object sender, RoutedEventArgs e)
    {
      var configWindow = new ConfigWindow(this.config);
      configWindow.ConfigurationApplied += (s, ev) => this.config = ev.Value;
      configWindow.ShowDialog();
    }
  }
}