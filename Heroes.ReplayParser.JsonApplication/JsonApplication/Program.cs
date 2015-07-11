using System.IO;
using MpqLib.Mpq;
using System;
using System.Linq;
using Newtonsoft.Json;
using Heroes.ReplayParser;

namespace ReplayToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
           
            // Use temp directory for MpqLib directory permissions requirements
            var tmpPath = Path.GetTempFileName();
            File.Copy(path, tmpPath, true);

            try
            {
                // Create our Replay object: this object will be filled as you parse the different files in the .StormReplay archive
                var replay = new Replay();
                MpqHeader.ParseHeader(replay, tmpPath);
                using (var archive = new CArchive(tmpPath))
                {
                    ReplayInitData.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.initData"));
                    ReplayDetails.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.details"));
                    ReplayTrackerEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.tracker.events"));
                    ReplayAttributeEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.attributes.events"));
                    if (replay.ReplayBuild >= 32455)
                        ReplayGameEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.game.events"));
                    ReplayServerBattlelobby.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.server.battlelobby"));
                    Unit.ParseUnitData(replay);
                }

                // Our Replay object now has all currently available information
                Console.WriteLine(JsonConvert.SerializeObject(replay, Formatting.None, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects }));
            }
            finally
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private static byte[] GetMpqArchiveFileBytes(CArchive archive, string archivedFileName)
        {
            var buffer = new byte[archive.FindFiles(archivedFileName).Single().Size];
            archive.ExportFile(archivedFileName, buffer);
            return buffer;
        }
    }
}
