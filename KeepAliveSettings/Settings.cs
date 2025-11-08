using System.Collections.ObjectModel;
using System.Collections.Specialized;

using CommunityToolkit.Mvvm.ComponentModel;

namespace KeepAliveSettings;

public class Settings : ObservableObject
{
  public Settings() { }

  public bool IsKeepAliveServiceRunning
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      SettingsProvider.Save(this);
    }
  } = false;

  public bool ApplyToSelectedDevices
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      SettingsProvider.Save(this);
    }
  } = false;

  public ObservableCollection<string> SelectedDevices
  {
    get;
    set
    {
      if (ReferenceEquals(field, value))
        return;

      field.CollectionChanged -= SelectedDevices_CollectionChanged;
      SetProperty(ref field, value);
      value.CollectionChanged += SelectedDevices_CollectionChanged;
    }
  } = new();

  private void SelectedDevices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    SettingsProvider.Save(this);
  }

  public TimeSpan KeepAliveInterval
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      SettingsProvider.Save(this);
    }
  } = TimeSpan.FromMinutes(15);

  public SoundKind SoundKind
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      SettingsProvider.Save(this);
    }
  } = SoundKind.Silent1;

  public string UserAudio
  {
    get;
    set
    {
      if (field == value)
        return;

      SetProperty(ref field, value);
      SettingsProvider.Save(this);
    }
  } = "";
}