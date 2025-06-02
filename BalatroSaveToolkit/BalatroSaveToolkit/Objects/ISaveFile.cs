namespace BalatroSaveToolkit.Objects;

public interface ISaveFile {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string SaveDate { get; set; }
    public string SaveTime { get; set; }
    
    public string Ante { get; set; }
    public string Blind { get; set; }
    public string Hand { get; set; }
    
}
