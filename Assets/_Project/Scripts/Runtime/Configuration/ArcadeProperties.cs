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
    public abstract class ArcadeProperties
    {
        [XmlElement("scene")]                                      public string Scene                    = "";
        [XmlElement("camera_settings")]                            public CameraSettings CameraSettings   = new CameraSettings();
        [XmlElement("render_settings")]                            public RenderSettings RenderSettings   = new RenderSettings();
        [XmlElement("audio_settings")]                             public AudioSettings AudioSettings     = new AudioSettings();
        [XmlElement("model")]                                      public string Model                    = "";
        [XmlArray("props"), XmlArrayItem("prop"), HideInInspector] public PropEntityConfiguration[] Props = new PropEntityConfiguration[0];
    }
}
