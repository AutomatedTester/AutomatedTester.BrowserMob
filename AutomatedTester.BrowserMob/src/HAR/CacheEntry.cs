﻿using System;

namespace AutomatedTester.BrowserMob.HAR
{
    public class CacheEntry
    {
        public DateTime? Expires { get; set; }

        public DateTime LastAccess { get; set; }

        public string Etag { get; set; }

        public int HitCount { get; set; }

        public string Comment { get; set; }
    }
}