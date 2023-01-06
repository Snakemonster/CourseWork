using System.Collections.Concurrent;

namespace Server;

/// <summary>
///     Inverted index shows which string and how many time it repeats in each file
/// </summary>
public class InvertedIndex
{
    private ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _invertedIndex;
    private readonly string _pathToFolder;
    private readonly string[] _separatingStrings;
    
    /// <param name="pathToFolder"><see cref="String"/> of path where stored files</param>
    /// <param name="separatingStrings"> <see cref="Array.string"/> array of separating strings for document</param>
    public InvertedIndex(string pathToFolder, string[] separatingStrings)
    {
        _invertedIndex = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
        _pathToFolder = pathToFolder;
        _separatingStrings = separatingStrings;
    }

    /// <summary>
    /// Generates dictionary from files
    /// </summary>
    public void GenerateDictionary()
    {
        var directories = new List<string> { @"\test\neg", @"\test\pos", @"\train\neg", @"\train\pos", @"\train\unsup" };
        var allFiles = directories.Select(file => Directory.EnumerateFiles(_pathToFolder + file)).SelectMany(x => x).ToList();
        const int threadCount = 8;
        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var threadStart = allFiles.Count / threadCount * i;
            var threadEnd = i == threadCount - 1 ? allFiles.Count : allFiles.Count / threadCount * (i + 1);

            threads[i] = new Thread(() => BuildDictionary(allFiles, threadStart, threadEnd));
            threads[i].Start();
        }
        for (var i = 0; i < threadCount; i++) { threads[i].Join(); }
    }

    /// <summary>
    ///     Method building inverted index dictionary
    /// </summary>
    /// <param name="allFiles">
    ///     <see cref="List{T}"/>
    ///     list of all files that needed to be checked
    /// </param>
    /// <param name="Aind">first element in list</param>
    /// <param name="Bind">last element in list</param>
    public void BuildDictionary(List<string> allFiles, int Aind, int Bind)
    {
        for (var i = Aind; i < Bind; i++)
        {
            var file = allFiles[i];
            var content = File.ReadAllText(file).ToLower().Split(_separatingStrings, StringSplitOptions.RemoveEmptyEntries).ToList();
            addToIndex(content, file.Replace(_pathToFolder, ""));
        }
    }

    private void addToIndex(List<string> words, string document)
    {
        foreach (var word in words)
        {
            if (!_invertedIndex.ContainsKey(word))
            {
                _invertedIndex.TryAdd(word, new ConcurrentDictionary<string, int>());
                _invertedIndex[word].TryAdd(document, 1);
            }
            else if (!_invertedIndex[word].ContainsKey(document)) _invertedIndex[word].TryAdd(document, 1);
            else _invertedIndex[word][document]++;
        }
    }
    
    /// <param name="text">text which needed to be found</param>
    /// <returns>
    ///     <see cref="ConcurrentDictionary{TKey,TValue}"/>
    ///     where key is file, value is count of word in file.
    /// </returns>
    public ConcurrentDictionary<string, int> this[string text] => (_invertedIndex.ContainsKey(text) ? _invertedIndex[text] : null)!;
}