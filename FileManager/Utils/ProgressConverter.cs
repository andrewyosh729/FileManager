using System;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Data.Converters;

namespace FileManager.Utils;

public class ProgressConverter : IValueConverter
{
    private DriveInfo DriveInfo { get; }

    public ProgressConverter(DriveInfo driveInfo)
    {
        DriveInfo = driveInfo;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long l)
        {
            return l / (double)DriveInfo.TotalSize;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}