namespace JMMClient
{
    public class NamingLanguage
    {
        public string Language { get; set; }

        public string FlagImage
        {
            get
            {
                return Languages.GetFlagImage(Language.Trim().ToUpper());
            }
        }

        public string LanguageDescription
        {
            get
            {
                return Languages.GetLanguageDescription(Language.Trim().ToUpper());

            }
        }

        public NamingLanguage()
        {
        }

        public NamingLanguage(string language)
        {
            this.Language = language;
        }

        public override string ToString()
        {
            return string.Format("{0} - ({1}) - {2}", Language, LanguageDescription, FlagImage);
        }
    }
}
