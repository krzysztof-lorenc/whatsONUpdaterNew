﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruiseControlPlugin.cs" company="Soloplan GmbH">
//   Copyright (c) Soloplan GmbH. All rights reserved.
//   Licensed under the MIT License. See License-file in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Soloplan.WhatsON.CruiseControl
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Soloplan.WhatsON.Composition;
  using Soloplan.WhatsON.Configuration;
  using Soloplan.WhatsON.Model;

  public class CruiseControlPlugin : ConnectorPlugin
  {
    public CruiseControlPlugin()
      : base(typeof(CruiseControlConnector))
    {
    }

    public override Connector CreateNew(ConnectorConfiguration configuration)
    {
      return new CruiseControlConnector(configuration);
    }

    /// <summary>
    /// Gets the projects.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns>
    /// The projects from the server.
    /// </returns>
    public override async Task<IList<Project>> GetProjects(string address)
    {
      var result = new List<Project>();
      var server = CruiseControlManager.GetServer(address, false);
      var allProjects = await server.GetAllProjects();
      var serverProjects = new List<Project>();
      foreach (var project in allProjects.CruiseControlProject)
      {
        var serverProjectTreeItem = new Project { Name = project.Name, Address = address };
        if (string.IsNullOrWhiteSpace(project.ServerName))
        {
          result.Add(serverProjectTreeItem);
        }
        else
        {
          var serverProject = serverProjects.FirstOrDefault(s => s.Name == project.ServerName);
          if (serverProject == null)
          {
            serverProject = new Project { Name = project.ServerName };
            serverProjects.Add(serverProject);
            result.Add(serverProject);
          }

          serverProject.Children.Add(serverProjectTreeItem);
        }
      }

      return result;
    }

    /// <summary>
    /// Assigns the <see cref="T:Soloplan.WhatsON.ServerProject" /> to <see cref="T:Soloplan.WhatsON.ConfigurationItem" />.
    /// </summary>
    /// <param name="project">The server project.</param>
    /// <param name="configurationItemsSupport">The configuration items provider.</param>
    /// <param name="serverAddress">The server address.</param>
    public override void Configure(Project project, IConfigurationItemProvider configurationItemsSupport, string serverAddress)
    {
      configurationItemsSupport.GetConfigurationByKey(Connector.ProjectName).Value = project.Name;
      configurationItemsSupport.GetConfigurationByKey(Connector.ServerAddress).Value = serverAddress;
    }
  }
}