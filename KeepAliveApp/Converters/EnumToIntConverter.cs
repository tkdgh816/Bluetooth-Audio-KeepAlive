using Microsoft.UI.Xaml.Data;

namespace KeepAliveApp.Converters;

public partial class EnumToIntConverter : DependencyObject, IValueConverter
{
  public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumToIntConverter), new PropertyMetadata(null));
  public Type EnumType
  {
    get => (Type)GetValue(EnumTypeProperty);
    set => SetValue(EnumTypeProperty, value);
  }

  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is null)
      return 0;

    return value.GetType().Equals(EnumType)
      ? System.Convert.ToInt32(value)
      : throw new ArgumentException("value is not type of Enum");
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    return value is int intValue
      ? Enum.ToObject(EnumType, intValue)
      : throw new ArgumentException("value is not type of int");
  }
}
