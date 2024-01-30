namespace SirSqlChauffeur;

internal static class CopyFolder
{
    private const string margin = "    ";

    public static void CopyDirectory(string sourceDir, string destinationDir, bool firstCall = true)
    {
        if (firstCall)
            Console.WriteLine($"\n\rCopie des nouveaux fichiers");

        Console.WriteLine($"{margin}SOURCE {sourceDir.eos(50)}");
        Console.WriteLine($"{margin}TARGET {destinationDir.eos(50)}");

        // Create the target directory if it does not already exist
        Directory.CreateDirectory(destinationDir);

        // Copy each file into the new directory
        foreach (string filePath in Directory.GetFiles(sourceDir))
            CopyAndLog(filePath, destinationDir);

        // Copy each subdirectory using recursion
        foreach (string subDirPath in Directory.GetDirectories(sourceDir))
            CopyDirectory(subDirPath, Path.Combine(destinationDir, Path.GetFileName(subDirPath)), true); // recursive call
    }

    private static void CopyAndLog (string filePath, string destinationDir)
    {
        File.Copy(filePath, Path.Combine(destinationDir, Path.GetFileName(filePath)), true);
        Console.WriteLine($"{margin}-> {Path.GetFileName(filePath)}");
    }

    private static string eos (this string s, int l) => s.Length < l ? s : $"...{new string(s.TakeLast(l - 3).ToArray())}";
}
