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

namespace Arcade
{
    [System.Serializable, XmlRoot("emulator")]
    public sealed class EmulatorConfiguration : DatabaseEntry
    {
        [XmlElement("interaction_type")]                           public InteractionType InteractionType = InteractionType.LibretroCore;
        [XmlElement("directory")]                                  public string Directory                = "";
        [XmlElement("working_directory")]                          public string WorkingDirectory         = "";
        [XmlElement("executable")]                                 public string Executable               = "";
        [XmlElement("arguments")]                                  public string Arguments                = "";
        [XmlElement("supported_extensions")]                       public string SupportedExtensions      = "";
        [XmlArray("games_directories"), XmlArrayItem("directory")] public string[] GamesDirectories       = new string[0];
    }
}
