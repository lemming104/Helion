﻿using System;

namespace Helion.World.Static;

[Flags]
public enum SectorDynamic
{
    None = 0,
    Movement = 1,
    Light = 2,
    TransferHeights = 4,
    TransferHeightStatic = 8,
    Scroll = 16,
    ChangeFloorTexture = 32,
    ChangeWallTexture = 64,
    Alpha = 128,
    Sky = 256
}
