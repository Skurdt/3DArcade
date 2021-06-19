/* MIT License

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

using System.Xml.Serialization;
using UnityEngine;

namespace Arcade
{
    [System.Serializable]
    public sealed class GameEntityConfiguration : EntityConfigurationBase
    {
        [XmlElement("interaction_type")]      public InteractionType InteractionType = InteractionType.Default;
        [XmlAttribute("platform")]            public string Platform                 = "";
        [XmlElement("emulator_override")]     public string Emulator                 = "";
        [XmlElement("name_override")]         public string Name                     = "";
        [XmlElement("cloneof_override")]      public string CloneOf                  = "";
        [XmlElement("romof_override")]        public string RomOf                    = "";
        [XmlElement("genre_override")]        public string Genre                    = "";
        [XmlElement("year_override")]         public string Year                     = "";
        [XmlElement("manufacturer_override")] public string Manufacturer             = "";

        [XmlIgnore, HideInInspector] public PlatformConfiguration PlatformConfiguration = null;
        [XmlIgnore, HideInInspector] public EmulatorConfiguration EmulatorConfiguration = null;
        [XmlIgnore, HideInInspector] public GameConfiguration GameConfiguration         = null;

        public void AssignConfigurations(Databases databases)
        {
            GameConfiguration game = null;

            bool foundPlatfrom = databases.Platforms.TryGet(Platform, out PlatformConfiguration platform);
            if (foundPlatfrom)
            {
                string[] returnFields = new string[]
                {
                    "Description",
                    "CloneOf",
                    "RomOf",
                    "Genre",
                    "Year",
                    "Manufacturer",
                    "ScreenType",
                    "ScreenRotation",
                    "Mature",
                    "Playable",
                    "IsBios",
                    "IsDevice",
                    "IsMechanical",
                    "Available",
                };
                string[] searchFields = new string[] { "Name" };
                _ = databases.Games.TryGet(platform.MasterList, Id, returnFields, searchFields, out game);
            }

            bool foundEmulator = databases.Emulators.TryGet(Emulator, out EmulatorConfiguration emulator);
            if (foundPlatfrom && !foundEmulator)
                _ = databases.Emulators.TryGet(platform.Emulator, out emulator);

            PlatformConfiguration = platform;
            EmulatorConfiguration = emulator;
            GameConfiguration     = game;
        }

        public override string GetDescription()
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;

            if (!(GameConfiguration is null) && !string.IsNullOrEmpty(GameConfiguration.Description))
                return GameConfiguration.Description;

            return Id;
        }
    }
}
