using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRecoveryTool
{
  public class RecoveryTool
  {
    private readonly string _brokenFilesRoot;
    private readonly string _validFilesRoot;

    private SortedDictionary<long, string[]> _namedBrokenFiles;
    private SortedDictionary<long, string[]> _unknownValidFiles;

    public RecoveryTool(string brokenFilesRoot, string validFilesRoot)
    {
      _brokenFilesRoot = brokenFilesRoot;
      _validFilesRoot = validFilesRoot;

      InitializeAsync();
    }

    private async void InitializeAsync()
    {
      _namedBrokenFiles = await IndexFilesAsync(_brokenFilesRoot);
      _unknownValidFiles = await IndexFilesAsync(_validFilesRoot);
    }

    private async Task<SortedDictionary<long, string[]>> IndexFilesAsync(string root)
    {
      var di = new DirectoryInfo(root);
      var result = new SortedDictionary<long, string[]>();

      var groups = await Task.Run(() => di.EnumerateFiles("*", SearchOption.AllDirectories).GroupBy(fi => fi.Length));
      foreach (var g in groups)
        result[g.Key] = g.Select(fi => fi.FullName).ToArray();

      return result;
    }

    public Dictionary<string, string> SyncFiles()
    {
      var result = new Dictionary<string, string>();
      foreach (var brokenSet in _namedBrokenFiles)
      {
        // Only one file of that size found in the broken collection
        if (brokenSet.Value.Length == 1)
        {
          var validSet = _unknownValidFiles[brokenSet.Key];
          // Only one file of that size found in the unamed collection
          if (validSet.Length == 1)
            result[brokenSet.Value.Single()] = validSet.Single();
        }
      }

      return result;
    }

    //TODO: Add the copy part using the results of SyncFiles method.
  }
}