﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.Formatters;
using Samba.Services.Implementations.PrinterModule.Tools;

namespace Samba.Services.Implementations.PrinterModule.PrintJobs
{
    class DemoPrinterJob : AbstractPrintJob
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        public DemoPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            Debug.Assert(!string.IsNullOrEmpty(Printer.ShareName));
            var pcs = Printer.ShareName.Split('#');
            var wname = "Edit";
            if (pcs.Length > 1)
                wname = pcs[1];

            var notepads = Process.GetProcessesByName(pcs[0]);

            if (notepads.Length == 0) 
                notepads = Process.GetProcessesByName("notepad");

            if (notepads.Length == 0) 
                return;

            if (notepads[0] != null)
            {
                var child = FindWindowEx(notepads[0].MainWindowHandle, new IntPtr(0), wname, null);
                var text = new FormattedDocument(lines, Printer.CharsPerLine).GetFormattedText();
                SendMessage(child, 0x000C, 0, text);
            }
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document));
        }
    }
}
