﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Samba.Presentation.Common.Converters
{
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
