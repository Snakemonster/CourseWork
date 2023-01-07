using System.Diagnostics;

var pathToFiles = @"..\..\..\..\files";

var separatingStrings = new[] { ".", ",", "<br", " ", ":", ";", "/>", "<br/>", "\"", "?", "(", ")" , "{", "}", "@", "<", ">", "!"};
var invertedIndex = new InvertedIndex(pathToFiles, separatingStrings);

var directories = (from dir1 in Directory.EnumerateDirectories(pathToFiles)
    from dir2 in Directory.EnumerateDirectories(dir1)
    select dir2.Replace(pathToFiles, "")).ToList();


var timer = new Stopwatch();
const int maxThreads = 8;
for (int i = 1; i <= maxThreads; i++)
{
    long averageTime = 0;
    for (int j = 0; j < maxThreads; j++)
    {
        timer.Start();
        invertedIndex.GenerateDictionary(directories, i);
        timer.Stop();
        averageTime += timer.ElapsedMilliseconds;
        timer.Reset();
        invertedIndex.ClearDataBase();
    }

    Console.WriteLine($"Using {i} threads we have average result {averageTime / maxThreads}ms");
}