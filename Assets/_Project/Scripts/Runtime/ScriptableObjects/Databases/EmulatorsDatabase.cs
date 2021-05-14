﻿/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using UnityEngine;

namespace Arcade
{
    [CreateAssetMenu(menuName = "Arcade/Database/EmulatorsDatabase", fileName = "EmulatorsDatabase")]
    public sealed class EmulatorsDatabase : MultiFileDatabase<EmulatorConfiguration>
    {
        public static readonly EmulatorConfiguration FpsArcadeLauncher = MakeInternalEmulator("_fps_arcade_launcher", "InternalFpsArcadeLauncher", InteractionType.FpsArcadeConfiguration);
        public static readonly EmulatorConfiguration CylArcadeLauncher = MakeInternalEmulator("_cyl_arcade_launcher", "InternalCylArcadeLauncher", InteractionType.CylArcadeConfiguration);
        public static readonly EmulatorConfiguration FpsMenuLauncher   = MakeInternalEmulator("_fps_menu_launcher", "InternalFpsMenuLauncher", InteractionType.FpsMenuConfiguration);
        public static readonly EmulatorConfiguration CylMenuLauncher   = MakeInternalEmulator("_cyl_menu_launcher", "InternalCylMenuLauncher", InteractionType.CylMenuConfiguration);

        [field: System.NonSerialized] protected override string DirectoryAlias { get; } = "emulator_cfgs";

        protected override void PostInitialize() => LoadAll();

        private static EmulatorConfiguration MakeInternalEmulator(string id, string description, InteractionType interactionType) => new EmulatorConfiguration
        {
            Id              = id,
            Description     = description,
            InteractionType = interactionType
        };
    }
}
