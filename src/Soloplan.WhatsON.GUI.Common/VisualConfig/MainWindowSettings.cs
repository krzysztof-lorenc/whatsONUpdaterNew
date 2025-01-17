﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowSettings.cs" company="Soloplan GmbH">
//   Copyright (c) Soloplan GmbH. All rights reserved.
//   Licensed under the MIT License. See License-file in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Soloplan.WhatsON.GUI.Common.VisualConfig
{
  public class MainWindowSettings
  {
    public TreeListSettings TreeListSettings { get; set; }

    public WindowSettings MainWindowDimensions { get; set; }

    public WindowSettings ConfigDialogSettings { get; set; }

    public ColorSettings ColorSettings { get; set; }
  }
}