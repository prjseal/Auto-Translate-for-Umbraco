﻿namespace AutoTranslate.Models
{
    public class DictionaryApiInstruction
    {
        public int NodeId { get; set; }
        public string CurrentCulture { get; set; }
        public bool OverwriteExistingValues { get; set; }
        public bool IncludeDescendants { get; set; }
        public bool FallbackToKey { get; set; }
    }
}
