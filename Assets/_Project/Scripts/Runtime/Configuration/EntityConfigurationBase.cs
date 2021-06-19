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
    public abstract class EntityConfigurationBase : IArcadeObject
    {
        [XmlAttribute("id")]                           public string NameId                           = "";
        [XmlElement("grabbable")]                      public bool Grabbable                          = true;
        [XmlElement("movecab_movable")]                public bool MoveCabMovable                     = true;
        [XmlElement("movecab_grabbable")]              public bool MoveCabGrabbable                   = true;
        [XmlElement("model_override")]                 public string Model                            = "";
        [XmlElement("artwork_files_override")]         public FilesOverrides ArtworkFiles             = new FilesOverrides();
        [XmlElement("artwork_directories_override")]   public DirectoriesOverrides ArtworkDirectories = new DirectoriesOverrides();
        [XmlElement("description_override")]           public string Description                      = "";
        [XmlElement("position"), HideInInspector]      public DatabaseVector3 Position                = DatabaseVector3.Zero;
        [XmlElement("rotation"), HideInInspector]      public DatabaseVector3 Rotation                = DatabaseVector3.Zero;
        [XmlElement("scale"), HideInInspector]         public DatabaseVector3 Scale                   = DatabaseVector3.One;

        [XmlIgnore()] public string Id { get => NameId; set => NameId = value; }

        public virtual string GetDescription() => !string.IsNullOrEmpty(Description) ? Description : Id;
    }
}
