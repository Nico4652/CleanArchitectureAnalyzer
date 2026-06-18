using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitectureAnalyzer.Constants
{
    public class DomainType
    {
        public const string Object = "Object";
        public const string Service = "Service";
        public const string Event = "Event";
        public const string Entity = "Entity";
        public const string Root = "Root";


        public static readonly string[] types =
        {
            Object,
            Service,
            Event, 
            Entity, 
            Root
        };
    }
}
