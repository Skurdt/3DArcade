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

using System.Xml.Serialization;
using UnityEngine;

namespace Arcade
{
    [System.Serializable, XmlRoot("arcade")]
    public sealed class ArcadeConfiguration : DatabaseEntry
    {
        [XmlElement("fpsarcade_properties")]
        public FpsArcadeProperties FpsArcadeProperties = new FpsArcadeProperties();

        [XmlElement("cylarcade_properties")]
        public CylArcadeProperties CylArcadeProperties = new CylArcadeProperties();

        [XmlArray("games"), XmlArrayItem("game")]
        [HideInInspector]
        public ModelConfiguration[] Games = new ModelConfiguration[0];

        [XmlIgnore]
        public ArcadeType ArcadeType { get; set; }

        [XmlIgnore]
        public ArcadeMode ArcadeMode { get; set; }
    }
}
