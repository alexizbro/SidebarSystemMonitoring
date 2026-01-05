using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SidebarSystemMonitoring.Utilities;

namespace SidebarSystemMonitoring.Models.Entries;

[JsonObject(MemberSerialization.OptIn)]
public class ChangeLogEntry
{
    public static ChangeLogEntry[] Load()
    {
        ChangeLogEntry[] _return = null;

        string _file = Paths.ChangeLog;

        if (File.Exists(_file))
        {
            using (StreamReader _reader = File.OpenText(_file))
            {
                _return = (ChangeLogEntry[])new JsonSerializer().Deserialize(_reader, typeof(ChangeLogEntry[]));
            }
        }

        return _return ?? new ChangeLogEntry[0];
    }

    [JsonProperty]
    public string Version { get; set; }

    [JsonProperty]
    public string[] Changes { get; set; }
}