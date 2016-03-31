using Prism.Windows.AppModel;
using Windows.ApplicationModel.Resources;

namespace MeetingManager
{
    public static class ResMan
    {
        private static readonly ResourceLoaderAdapter _resourceLoader = new ResourceLoaderAdapter(new ResourceLoader());

        public static string GetString(string id)
        {
            return _resourceLoader.GetString(id);
        }
    }
}
