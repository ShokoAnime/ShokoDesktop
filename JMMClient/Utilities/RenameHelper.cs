using System.Collections.Generic;

namespace JMMClient
{
    public class RenameHelper
    {
        public static List<RenameTag> GetAllTags()
        {
            List<RenameTag> allTags = new List<RenameTag>();

            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeID, Constants.FileRenameTag_Tag.AnimeID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameEnglish, Constants.FileRenameTag_Tag.AnimeNameEnglish));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameKanji, Constants.FileRenameTag_Tag.AnimeNameKanji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.AnimeNameRomaji, Constants.FileRenameTag_Tag.AnimeNameRomaji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.CRCLower, Constants.FileRenameTag_Tag.CRCLower));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.CRCUpper, Constants.FileRenameTag_Tag.CRCUpper));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.DubLanguage, Constants.FileRenameTag_Tag.DubLanguage));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.ED2KLower, Constants.FileRenameTag_Tag.ED2KLower));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.ED2KUpper, Constants.FileRenameTag_Tag.ED2KUpper));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeID, Constants.FileRenameTag_Tag.EpisodeID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNameEnglish, Constants.FileRenameTag_Tag.EpisodeNameEnglish));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNameRomaji, Constants.FileRenameTag_Tag.EpisodeNameRomaji));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.EpisodeNumber, Constants.FileRenameTag_Tag.EpisodeNumber));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Episodes, Constants.FileRenameTag_Tag.Episodes));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.FileID, Constants.FileRenameTag_Tag.FileID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.FileVersion, Constants.FileRenameTag_Tag.FileVersion));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupID, Constants.FileRenameTag_Tag.GroupID));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupLongName, Constants.FileRenameTag_Tag.GroupLongName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.GroupShortName, Constants.FileRenameTag_Tag.GroupShortName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.VideoBitDepth, Constants.FileRenameTag_Tag.VideoBitDepth));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Resolution, Constants.FileRenameTag_Tag.Resolution));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Source, Constants.FileRenameTag_Tag.Source));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.SubLanguage, Constants.FileRenameTag_Tag.SubLanguage));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Type, Constants.FileRenameTag_Tag.Type));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Year, Constants.FileRenameTag_Tag.Year));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.OriginalFileName, Constants.FileRenameTag_Tag.OriginalFileName));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Censored, Constants.FileRenameTag_Tag.Censored));
            allTags.Add(new RenameTag(Constants.FileRenameTag_Name.Deprecated, Constants.FileRenameTag_Tag.Deprecated));
            return allTags;
        }

        public static List<RenameTest> GetAllTests()
        {
            List<RenameTest> allTests = new List<RenameTest>();

            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.AnimeID, Constants.FileRenameTest_Test.AnimeID));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.AnimeType, Constants.FileRenameTest_Test.AnimeType));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Codec, Constants.FileRenameTest_Test.Codec));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.DubLanguage, Constants.FileRenameTest_Test.DubLanguage));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.EpisodeCount, Constants.FileRenameTest_Test.EpisodeCount));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.EpisodeNumber, Constants.FileRenameTest_Test.EpisodeNumber));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.FileVersion, Constants.FileRenameTest_Test.FileVersion));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.GroupID, Constants.FileRenameTest_Test.GroupID));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.RipSource, Constants.FileRenameTest_Test.RipSource));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.SubLanguage, Constants.FileRenameTest_Test.SubLanguage));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Tag, Constants.FileRenameTest_Test.Tag));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoBitDepth, Constants.FileRenameTest_Test.VideoBitDepth));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.Year, Constants.FileRenameTest_Test.Year));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoResolutionWidth, Constants.FileRenameTest_Test.VideoResolutionWidth));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.VideoResolutionHeight, Constants.FileRenameTest_Test.VideoResolutionHeight));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.ManuallyLinked, Constants.FileRenameTest_Test.ManuallyLinked));
            allTests.Add(new RenameTest(Constants.FileRenameTest_Name.HasEpisodes, Constants.FileRenameTest_Test.HasEpisodes));
            return allTests;
        }
    }
}
