namespace AutoTranslate.Models
{
    public class ContentApiInstruction
    {
        public int NodeId { get; set; }
        public string CurrentCulture { get; set; }
        public bool OverwriteExistingValues { get; set; }
        public bool IncludeDescendants { get; set; }
        public string AllowedPropertyEditors { get; set; }
    }
}
