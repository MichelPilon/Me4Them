using   SirSqlChauffeur;

string  processName = "SSMS";
var     source      = @"\\ccq.org\Partages\SS01\PILON_MICHEL\SirSqlChauffeur\FilesNew";
var     target      = @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 19\Common7\IDE\Extensions";

Console.Clear();
Console.WriteLine();
Console.WriteLine("Sir Sql Chauffeur");
Console.WriteLine();
Console.WriteLine();

Action execute = () =>
{ 
    if (!new DirectoryInfo(source).Exists)
    {
        Console.WriteLine();
        throw new Exception($"Incapable de repérer le folder suivant :\n\r    {source}");
    }
    Console.WriteLine($"Le folder d'update a été repéré");

    if (!Directory.GetFiles(source).Select(_ => new FileInfo(_)).Any())
    {
        Console.WriteLine();
        throw new Exception($"Il n'y a pas de fichiers dans le folder suivant :\n\r    {source}");
    }
    Console.WriteLine($"Les fichiers et folders du folder d'update ont été repérés");

    var processes = Process.GetProcessesByName(processName);
    if (processes.Any())
    {
        Console.WriteLine();
        throw new Exception("SSMS est en cours d'exécution et doit être fermé");
    }

    if (!Directory.Exists(target))
    {
        Console.WriteLine();
        throw new Exception($"Incapable de repérer le folder suivant :\n\r    {target}");
    }

    target = Path.Combine(target, "SirSqlValet");
    if (Directory.Exists(target)) 
    {
        try
        {
            new DirectoryInfo(target).Delete(recursive: true);
            Console.WriteLine($"Les fichiers et folders déjà en place ont été détruits.");
        }
        catch (Exception)
        {
            Console.WriteLine();
            throw new Exception($"Incapable de repérer le folder suivant :\n\r    {target}");
        }
    }

    CopyFolder.CopyDirectory(source, target);
    Console.WriteLine();
    Console.WriteLine($"Les nouveaux fichiers et folders ont été copiés.");
};

try
{
    execute();
    Console.WriteLine("Tout semble s'être déroulé normalement");
}
catch (Exception exception)
{
    string message = string.Empty;
    while ((exception = string.IsNullOrWhiteSpace(message) ? exception : exception.InnerException) != null) 
        message += $"\n\r\n\r - {exception.Message}";

    Console.WriteLine($"\n\r\n\rERREUR(S) NON PRÉVUE(S){message}");
}

Console.Write("\n\r\n\r...appuyez une touche pour fermer cette fenêtre ... ");
Console.ReadKey();
Console.Write("\n\r\n\r");