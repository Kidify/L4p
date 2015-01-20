using System;

namespace L4p.Common.Plugins
{
    class InstanceBox
    {
        public int Version { get; set; }
        public Type InterfaceType { get; set; }
        public Type ImplementationType { get; set; }
        public DateTime LoadedAt { get; set; }
        public DateTime LastCheckedAt { get; set; }
        public string LoadedFrom { get; set; }
        public object Instance { get; set; }
    }
}