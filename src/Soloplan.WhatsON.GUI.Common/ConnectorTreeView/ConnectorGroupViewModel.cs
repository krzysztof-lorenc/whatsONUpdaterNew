﻿// <copyright file="ConnectorGroupViewModel.cs" company="Soloplan GmbH">
//   Copyright (c) Soloplan GmbH. All rights reserved.
//   Licensed under the MIT License. See License-file in the project root for license information.
// </copyright>

namespace Soloplan.WhatsON.GUI.Common.ConnectorTreeView
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Windows.Input;
  using NLog;
  using Soloplan.WhatsON.Composition;
  using Soloplan.WhatsON.Configuration;
  using Soloplan.WhatsON.Model;

  /// <summary>
  /// Viewmodel representing group of connectors shown as single node in <see cref="ConnectorTreeView"/>.
  /// </summary>
  public class ConnectorGroupViewModel : TreeItemViewModel
  {
    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly Logger log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType?.ToString());

    private string groupName;

    private ObservableCollection<ConnectorViewModel> statusViewModels;

    public ConnectorGroupViewModel()
    {
      this.IsNodeExpanded = true;
    }

    public event EventHandler ConfigurationChanged;

    public ObservableCollection<ConnectorViewModel> ConnectorViewModels => this.statusViewModels ?? (this.statusViewModels = new ObservableCollection<ConnectorViewModel>());

    public string GroupName
    {
      get => this.groupName;
      set
      {
        if (this.groupName != value)
        {
          this.groupName = value;
          this.OnPropertyChanged();
          this.OnConfigurationChanged(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether configuration was modified from main window.
    /// </summary>
    public override bool ConfigurationModifiedInTree
    {
      get => base.ConfigurationModifiedInTree;
      set
      {
        base.ConfigurationModifiedInTree = value;
        foreach (var connector in this.ConnectorViewModels)
        {
          connector.ConfigurationModifiedInTree = value;
        }
      }
    }

    /// <summary>
    /// Called when connector changes, ex new status.
    /// </summary>
    /// <param name="changedConnector">The connector which changed.</param>
    /// <returns>True if something was changed; false otherwise.</returns>
    public bool Update(Connector changedConnector)
    {
      var changedViewModel = this.ConnectorViewModels.FirstOrDefault(connector => connector.Identifier == changedConnector.Configuration.Identifier);
      if (changedViewModel != null)
      {
        changedViewModel.Update(changedConnector);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Should be called when configuration changed.
    /// </summary>
    /// <param name="connectorGroup">Grouping of connectors by group name.</param>
    public void Init(IGrouping<string, ConnectorConfiguration> connectorGroup)
    {
      this.GroupName = connectorGroup.Key ?? string.Empty;
      log.Debug("Initializing group {GroupName}", this.GroupName);

      var connectorsNoLongerPresent = this.ConnectorViewModels.Where(model => connectorGroup.All(configurationSubject => configurationSubject.Identifier != model.Identifier)).ToList();
      var newConnectors = connectorGroup.Where(configurationSubject => this.ConnectorViewModels.All(viewModel => configurationSubject.Identifier != viewModel.Identifier));
      foreach (var noLongerPresentConnectorViewModel in connectorsNoLongerPresent)
      {
        log.Debug("Remove no longer present connector {noLongerPresentConnectorViewModel}", new { noLongerPresentConnectorViewModel.Identifier, noLongerPresentConnectorViewModel.Name });
        noLongerPresentConnectorViewModel.EditItem -= this.OnSubItemEdit;
        this.ConnectorViewModels.Remove(noLongerPresentConnectorViewModel);
      }

      var addedIds = new List<Guid>();
      foreach (var newConnector in newConnectors)
      {
        log.Debug("Adding new connector {connectorConfiguration}", new { newConnector.Identifier, newConnector.Name });
        addedIds.Add(newConnector.Identifier);
        this.CreateViewModelForConnectorConfiguration(newConnector);
      }

      int index = 0;
      foreach (var config in connectorGroup)
      {
        log.Debug("Updating viewmodel for {connectorConfiguration}", new { config.Identifier, config.Name });
        var connectorViewModel = this.ConnectorViewModels.FirstOrDefault(model => model.Identifier == config.Identifier);
        if (connectorViewModel == null)
        {
          continue;
        }

        var oldIndex = this.ConnectorViewModels.IndexOf(connectorViewModel);
        if (oldIndex != index)
        {
          this.ConnectorViewModels.Move(oldIndex, index);
        }

        index++;
      }
    }

    public override void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
      foreach (var connectorViewModel in this.ConnectorViewModels)
      {
        connectorViewModel.OnDoubleClick(sender, e);
      }
    }

    /// <summary>
    /// Firest <see cref="ConfigurationChanged"/> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">Event args.</param>
    protected void OnConfigurationChanged(object sender, EventArgs eventArgs)
    {
      this.ConfigurationChanged?.Invoke(sender, eventArgs);
    }

    private void CreateViewModelForConnectorConfiguration(ConnectorConfiguration connectorConfiguration)
    {
      var connector = PluginManager.Instance.GetConnector(connectorConfiguration);
      ConnectorViewModel connectorViewModel = this.GetConnectorViewModel(connector);
      connectorViewModel.EditItem += this.OnSubItemEdit;
      connectorViewModel.DeleteItem += this.DeleteConnector;
      connectorViewModel.ExportItem += this.ExportItemHandler;
      connectorViewModel.Update(connector);
      this.ConnectorViewModels.Add(connectorViewModel);
    }

    /// <summary>
    /// Handles the export of a project item.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ValueEventArgs{TreeItemViewModel}"/> instance containing the event data.</param>
    private void ExportItemHandler(object sender, ValueEventArgs<TreeItemViewModel> e)
    {
      this.OnExportItem(this, e);
    }

    private ConnectorViewModel GetConnectorViewModel(Connector connector)
    {
      if (connector == null)
      {
        return new ConnectorMissingViewModel(connector);
      }

      var presentationPlugIn = PluginManager.Instance.GetPresentationPlugin(connector.Configuration.Type);
      if (presentationPlugIn != null)
      {
        return presentationPlugIn.CreateViewModel(connector);
      }

      return new ConnectorViewModel(connector);
    }

    private void OnSubItemEdit(object sender, ValueEventArgs<TreeItemViewModel> e)
    {
      this.OnEditItem(sender, e);
    }

    private async void DeleteConnector(object sender, DeleteTreeItemEventArgs e)
    {
      if (e.DeleteItem is ConnectorViewModel model)
      {
        this.OnDeleteItem(this, e);
        var canceled = await e.CheckCanceled();
        if (!canceled && this.ConnectorViewModels.Remove(model))
        {
          this.OnConfigurationChanged(this, EventArgs.Empty);
        }
      }
    }
  }
}