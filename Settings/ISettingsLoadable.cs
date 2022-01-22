namespace VideoCompressorGUI.Settings
{
    public interface ISettingsLoadable<T>
    {
        T CreateDefault();
    }
}