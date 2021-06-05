using System.Xml.Serialization;

namespace Arcade.Emulation.Mame
{
    namespace v2003Plus
    {
        [XmlRoot("mame")]
        public sealed class Data
        {
            [XmlElement("game")] public Game[] Games { get; set; }
        }

        public sealed class Game
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("sourcefile")] public string SourceFile { get; set; }
            [XmlAttribute("runnable")] public string Runnable { get; set; }
            [XmlAttribute("cloneof")] public string CloneOf { get; set; }
            [XmlAttribute("romof")] public string RomOf { get; set; }
            [XmlAttribute("sampleof")] public string SampleOf { get; set; }
            [XmlElement("description")] public string Description { get; set; }
            [XmlElement("year")] public string Year { get; set; }
            [XmlElement("manufacturer")] public string Manufacturer { get; set; }
            [XmlElement("biosset")] public BiosSet[] BiosSets { get; set; }
            [XmlElement("rom")] public Rom[] Roms { get; set; }
            [XmlElement("disk")] public Disk[] Disks { get; set; }
            [XmlElement("sample")] public Sample[] Samples { get; set; }
            [XmlElement("chip")] public Chip[] Chips { get; set; }
            [XmlElement("video")] public Video Video{ get; set; }
            [XmlElement("sound")] public Sound Sound { get; set; }
            [XmlElement("input")] public Input Input { get; set; }
            [XmlElement("dipswitch")] public Dipswitch[] Dipswitches { get; set; }
            [XmlElement("driver")] public Driver Driver { get; set; }
        }

        public sealed class BiosSet
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("description")] public string Description { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Rom
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("bios")] public string Bios { get; set; }
            [XmlAttribute("size")] public string Size { get; set; }
            [XmlAttribute("crc")] public string Crc { get; set; }
            [XmlAttribute("md5")] public string Md5 { get; set; }
            [XmlAttribute("sha1")] public string Sha1 { get; set; }
            [XmlAttribute("merge")] public string Merge { get; set; }
            [XmlAttribute("region")] public string Region { get; set; }
            [XmlAttribute("offset")] public string Offset { get; set; }
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("dispose")] public string Dispose { get; set; }
            [XmlAttribute("soundonly")] public string SoundOnly { get; set; }
        }

        public sealed class Disk
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("md5")] public string Md5 { get; set; }
            [XmlAttribute("sha1")] public string Sha1 { get; set; }
            [XmlAttribute("region")] public string Region { get; set; }
            [XmlAttribute("index")] public string Index { get; set; }
        }

        public sealed class Sample
        {
            [XmlAttribute("name")] public string Name { get; set; }
        }

        public sealed class Chip
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("soundonly")] public string SoundOnly { get; set; }
            [XmlAttribute("clock")] public string Clock { get; set; }
        }

        public sealed class Video
        {
            [XmlAttribute("screen")] public string Screen { get; set; }
            [XmlAttribute("orientation")] public string Orientation { get; set; }
            [XmlAttribute("width")] public string Width { get; set; }
            [XmlAttribute("height")] public string Height { get; set; }
            [XmlAttribute("aspectx")] public string AspectX { get; set; }
            [XmlAttribute("aspecty")] public string AspectY { get; set; }
            [XmlAttribute("refresh")] public float Refresh { get; set; }
        }

        public sealed class Sound
        {
            [XmlAttribute("channels")] public string Channels { get; set; }
        }

        public sealed class Input
        {
            [XmlAttribute("service")] public string Service { get; set; }
            [XmlAttribute("tilt")] public string Tilt { get; set; }
            [XmlAttribute("players")] public string Players { get; set; }
            [XmlElement("control")] public string Control { get; set; }
            [XmlAttribute("buttons")] public string Buttons { get; set; }
            [XmlAttribute("coins")] public string Coins { get; set; }
        }

        public sealed class Dipswitch
        {

            [XmlAttribute("name")] public string Name { get; set; }
            [XmlElement("dipvalue")] public DipValue[] DipValues { get; set; }
        }

        public sealed class DipValue
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Driver
        {
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("color")] public string Color { get; set; }
            [XmlAttribute("sound")] public string Sound { get; set; }
            [XmlAttribute("graphic")] public string Graphic { get; set; }
            [XmlAttribute("palettesize")] public string PaletteSize { get; set; }
        }
    }

    namespace v0221Compatible
    {
        [XmlRoot("mame")]
        public sealed class Data
        {
            [XmlAttribute("build")] public string Build { get; set; }
            [XmlElement("machine")] public Machine[] Machines { get; set; }
        }

        public sealed class Machine
        {
            [XmlAttribute("name")] public string Name { get; set; }
            //[XmlAttribute("sourcefile")] public string SourceFile { get; set; }
            [XmlAttribute("isbios")] public string IsBios { get; set; }
            [XmlAttribute("isdevice")] public string IsDevice { get; set; }
            [XmlAttribute("ismechanical")] public string IsMechanical { get; set; }
            [XmlAttribute("runnable")] public string Runnable { get; set; }
            [XmlAttribute("cloneof")] public string CloneOf { get; set; }
            [XmlAttribute("romof")] public string RomOf { get; set; }
            //[XmlAttribute("sampleof")] public string SampleOf { get; set; }
            [XmlElement("description")] public string Description { get; set; }
            [XmlElement("year")] public string Year { get; set; }
            [XmlElement("manufacturer")] public string Manufacturer { get; set; }
            //[XmlElement("biosset")] public BiosSet[] BiosSets { get; set; }
            //[XmlElement("rom")] public Rom[] Roms { get; set; }
            //[XmlElement("disk")] public Disk[] Disks { get; set; }
            //[XmlElement("device_ref")] public DeviceRef[] DeviceRefs { get; set; }
            //[XmlElement("sample")] public Sample[] Samples { get; set; }
            //[XmlElement("chip")] public Chip[] Chips { get; set; }
            [XmlElement("display")] public Display Display { get; set; }
            [XmlElement("sound")] public Sound Sound { get; set; }
            [XmlElement("condition")] public Condition Condition { get; set; }
            //[XmlElement("input")] public Input Input { get; set; }
            //[XmlElement("dipswitch")] public Dipswitch[] Dipswitches { get; set; }
            //[XmlElement("configuration")] public Configuration[] Configurations { get; set; }
            //[XmlElement("port")] public Port[] Ports { get; set; }
            //[XmlElement("adjuster")] public Adjuster Adjuster { get; set; }
            [XmlElement("driver")] public Driver Driver { get; set; }
            //[XmlElement("feature")] public Feature Feature { get; set; }
            //[XmlElement("device")] public Device Device { get; set; }
            //[XmlElement("slot")] public Slot Slot { get; set; }
            //[XmlElement("softwarelist")] public SoftwareList SoftwareList { get; set; }
            //[XmlElement("ramoption")] public RamOption RamOption { get; set; }
        }

        public sealed class BiosSet
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("description")] public string Description { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Rom
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("bios")] public string Bios { get; set; }
            [XmlAttribute("size")] public string Size { get; set; }
            [XmlAttribute("crc")] public string Crc { get; set; }
            [XmlAttribute("sha1")] public string Sha1 { get; set; }
            [XmlAttribute("merge")] public string Merge { get; set; }
            [XmlAttribute("region")] public string Region { get; set; }
            [XmlAttribute("offset")] public string Offset { get; set; }
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("optional")] public string Optional { get; set; }
        }

        public sealed class Disk
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("sha1")] public string Sha1 { get; set; }
            [XmlAttribute("merge")] public string Merge { get; set; }
            [XmlAttribute("region")] public string Region { get; set; }
            [XmlAttribute("index")] public string Index { get; set; }
            [XmlAttribute("writable")] public string Writable { get; set; }
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("optional")] public string Optional { get; set; }
        }

        public sealed class DeviceRef
        {
            [XmlAttribute("name")] public string Name { get; set; }
        }

        public sealed class Sample
        {
            [XmlAttribute("name")] public string Name { get; set; }
        }

        public sealed class Chip
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("clock")] public string Clock { get; set; }
        }

        public sealed class Display
        {
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("rotate")] public string Rotate { get; set; }
            [XmlAttribute("flipx")] public string FlipX { get; set; }
            [XmlAttribute("width")] public string Width { get; set; }
            [XmlAttribute("height")] public string Height { get; set; }
            [XmlAttribute("refresh")] public string Refresh { get; set; }
            [XmlAttribute("pixclock")] public string PixClock { get; set; }
            [XmlAttribute("htotal")] public string HTotal { get; set; }
            [XmlAttribute("hbend")] public string HBEnd { get; set; }
            [XmlAttribute("hbstart")] public string HBStart { get; set; }
            [XmlAttribute("vtotal")] public string VTotal { get; set; }
            [XmlAttribute("vbend")] public string VBEnd { get; set; }
            [XmlAttribute("vbstart")] public string VBStart { get; set; }
        }

        public sealed class Sound
        {
            [XmlAttribute("channels")] public string Channels { get; set; }
        }

        public sealed class Condition
        {
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("mask")] public string Mask { get; set; }
            [XmlAttribute("relation")] public string Relation { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
        }

        public sealed class Input
        {
            [XmlAttribute("service")] public string Service { get; set; }
            [XmlAttribute("tilt")] public string Tilt { get; set; }
            [XmlAttribute("players")] public string Players { get; set; }
            [XmlAttribute("coins")] public string Coins { get; set; }
            [XmlElement("control")] public Control[] Controls { get; set; }
        }

        public sealed class Control
        {
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("player")] public string Player { get; set; }
            [XmlAttribute("buttons")] public string Buttons { get; set; }
            [XmlAttribute("reqbuttons")] public string ReqButtons { get; set; }
            [XmlAttribute("minimum")] public string Minimum { get; set; }
            [XmlAttribute("maximum")] public string Maximum { get; set; }
            [XmlAttribute("sensitivity")] public string Sensitivity { get; set; }
            [XmlAttribute("keydelta")] public string KeyDelta { get; set; }
            [XmlAttribute("reverse")] public string Reverse { get; set; }
            [XmlAttribute("ways")] public string Ways { get; set; }
            [XmlAttribute("ways2")] public string Ways2 { get; set; }
            [XmlAttribute("ways3")] public string Ways3 { get; set; }
        }

        public sealed class Dipswitch
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("mask")] public string Mask { get; set; }
            [XmlElement("diplocation")] public DipLocation[] DipLocations { get; set; }
            [XmlElement("dipvalue")] public DipValue[] DipValues { get; set; }
        }

        public sealed class DipLocation
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("number")] public string Number { get; set; }
            [XmlAttribute("inverted")] public string Inverted { get; set; }
        }

        public sealed class DipValue
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Configuration
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("mask")] public string Mask { get; set; }
            [XmlElement("conflocation")] public ConfLocation[] ConfLocations { get; set; }
            [XmlElement("confsetting")] public ConfSetting[] ConfSettings { get; set; }
        }

        public sealed class ConfLocation
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("number")] public string Number { get; set; }
            [XmlAttribute("inverted")] public string Inverted { get; set; }
        }

        public sealed class ConfSetting
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Port
        {
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlElement("analog")] public Analog[] Analogs { get; set; }
        }

        public sealed class Analog
        {
            [XmlAttribute("mask")] public string Mask { get; set; }
        }

        public sealed class Adjuster
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Driver
        {
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("emulation")] public string Emulation { get; set; }
            [XmlAttribute("cocktail")] public string Cocktail { get; set; }
            [XmlAttribute("savestate")] public string Savestate { get; set; }
        }

        public sealed class Feature
        {
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("overall")] public string Overall { get; set; }
        }

        public sealed class Instance
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("briefname")] public string BriefName { get; set; }
        }

        public sealed class Extension
        {
            [XmlAttribute("name")] public string Name { get; set; }
        }

        public sealed class Device
        {
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("fixed_image")] public string FixedImage { get; set; }
            [XmlAttribute("mandatory")] public string Mandatory { get; set; }
            [XmlAttribute("stringerface")] public string Interface { get; set; }
            [XmlElement("instance")] public Instance[] Instances { get; set; }
            [XmlElement("extension")] public Extension[] Extensions { get; set; }
        }

        public sealed class SlotOption
        {
            [XmlAttribute("Name")] public string Name { get; set; }
            [XmlAttribute("DevName")] public string DevName { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }

        public sealed class Slot
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlElement("slotoption")] public SlotOption[] SlotOptions { get; set; }
        }

        public sealed class SoftwareList
        {
            [XmlAttribute("tag")] public string Tag { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("status")] public string Status { get; set; }
            [XmlAttribute("filter")] public string Filter { get; set; }
        }

        public sealed class RamOption
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("default")] public string Default { get; set; }
        }
    }
}
