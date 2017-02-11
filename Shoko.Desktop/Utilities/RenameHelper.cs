using System.Collections.Generic;
using Shoko.Commons;

namespace Shoko.Desktop.Utilities
{
    public class RenameHelper
    {
        public static List<RenameTag> GetAllTags()
        {
            List<RenameTag> allTags = new List<RenameTag>();

            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeID, Models.Constants.FileRenameTag_Tag.AnimeID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameEnglish, Models.Constants.FileRenameTag_Tag.AnimeNameEnglish));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameKanji, Models.Constants.FileRenameTag_Tag.AnimeNameKanji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameRomaji, Models.Constants.FileRenameTag_Tag.AnimeNameRomaji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.CRCLower, Models.Constants.FileRenameTag_Tag.CRCLower));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.CRCUpper, Models.Constants.FileRenameTag_Tag.CRCUpper));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.DubLanguage, Models.Constants.FileRenameTag_Tag.DubLanguage));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.ED2KLower, Models.Constants.FileRenameTag_Tag.ED2KLower));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.ED2KUpper, Models.Constants.FileRenameTag_Tag.ED2KUpper));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeID, Models.Constants.FileRenameTag_Tag.EpisodeID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNameEnglish, Models.Constants.FileRenameTag_Tag.EpisodeNameEnglish));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNameRomaji, Models.Constants.FileRenameTag_Tag.EpisodeNameRomaji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNumber, Models.Constants.FileRenameTag_Tag.EpisodeNumber));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Episodes, Models.Constants.FileRenameTag_Tag.Episodes));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.FileID, Models.Constants.FileRenameTag_Tag.FileID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.FileVersion, Models.Constants.FileRenameTag_Tag.FileVersion));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupID, Models.Constants.FileRenameTag_Tag.GroupID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupLongName, Models.Constants.FileRenameTag_Tag.GroupLongName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupShortName, Models.Constants.FileRenameTag_Tag.GroupShortName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.VideoBitDepth, Models.Constants.FileRenameTag_Tag.VideoBitDepth));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Resolution, Models.Constants.FileRenameTag_Tag.Resolution));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Source, Models.Constants.FileRenameTag_Tag.Source));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.SubLanguage, Models.Constants.FileRenameTag_Tag.SubLanguage));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Type, Models.Constants.FileRenameTag_Tag.Type));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Year, Models.Constants.FileRenameTag_Tag.Year));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.OriginalFileName, Models.Constants.FileRenameTag_Tag.OriginalFileName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Censored, Models.Constants.FileRenameTag_Tag.Censored));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Deprecated, Models.Constants.FileRenameTag_Tag.Deprecated));
            return allTags;
        }

        public static List<RenameTest> GetAllTests()
        {
            List<RenameTest> allTests = new List<RenameTest>();

            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.AnimeID, Models.Constants.FileRenameTest_Test.AnimeID));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.AnimeType, Models.Constants.FileRenameTest_Test.AnimeType));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Codec, Models.Constants.FileRenameTest_Test.Codec));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.DubLanguage, Models.Constants.FileRenameTest_Test.DubLanguage));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.EpisodeCount, Models.Constants.FileRenameTest_Test.EpisodeCount));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.EpisodeNumber, Models.Constants.FileRenameTest_Test.EpisodeNumber));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.FileVersion, Models.Constants.FileRenameTest_Test.FileVersion));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.GroupID, Models.Constants.FileRenameTest_Test.GroupID));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.RipSource, Models.Constants.FileRenameTest_Test.RipSource));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.SubLanguage, Models.Constants.FileRenameTest_Test.SubLanguage));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Tag, Models.Constants.FileRenameTest_Test.Tag));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoBitDepth, Models.Constants.FileRenameTest_Test.VideoBitDepth));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Year, Models.Constants.FileRenameTest_Test.Year));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoResolutionWidth, Models.Constants.FileRenameTest_Test.VideoResolutionWidth));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoResolutionHeight, Models.Constants.FileRenameTest_Test.VideoResolutionHeight));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.ManuallyLinked, Models.Constants.FileRenameTest_Test.ManuallyLinked));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.HasEpisodes, Models.Constants.FileRenameTest_Test.HasEpisodes));
            return allTests;
        }
    }
}
