using System.Windows;
using System.Windows.Markup;
using UP01_Rusakov_RealEstate.Classes;

namespace UP01_Rusakov_RealEstate
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("ru-RU")));

            base.OnStartup(e);
            Helper.LoadSavedTheme();
        }
    }
}
