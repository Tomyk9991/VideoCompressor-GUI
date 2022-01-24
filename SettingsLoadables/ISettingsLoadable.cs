namespace VideoCompressorGUI.SettingsLoadables
{
    public interface ISettingsLoadable<T>
    {
        T CreateDefault();
    }
}