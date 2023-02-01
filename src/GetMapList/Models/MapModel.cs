namespace GetMapList.Models
{
    internal class MapModel
    {
        public string MapName { get; set; }
        public bool IsMoreThan150MB { get; set; }
        public long FileSize { get; set; }
    }
}
