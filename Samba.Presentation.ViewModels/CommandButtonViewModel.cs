﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public interface ICommandButtonViewModel
    {

    }

    public class CommandButtonViewModel<T>
    {
        public CommandButtonViewModel()
        {
            Color = "Gainsboro";
        }
        public ICaptionCommand Command { get; set; }
        public string Caption { get; set; }
        public T Parameter { get; set; }
        public string Color { get; set; }
    }
}
