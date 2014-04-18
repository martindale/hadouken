﻿using System.Collections.Generic;

namespace Hadouken.Plugins.Metadata
{
    public interface IUserInterface
    {
        IEnumerable<string> BackgroundScripts { get; }

        ISettingsPage SettingsPage { get; }
    }
}