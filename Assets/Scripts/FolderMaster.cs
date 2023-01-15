using System.Collections.Generic;

/*
 * Contains references to every folder in the main gameplay loop.
 */ 
public static class FolderMaster
{
    public static List<FolderFile> allFolders = new();
    public static List<string> folderTitles = new()
    {
        "Music",
        "Sophie logs",
        "Marie logs",
        "Paul logs",
        "etcetcetc",
        "Marie's Food",
        "Paul's Thrift Finds",
        "My photo stuff",
        "Recycle Bin"
    };
    public static List<MediaFile> allMediaFilesInFoldersForPreservation = new(); // Stores every file's location and attributes to unload again after the ending cutscene, allowing the user to continue playing exactly where they left off.
}
