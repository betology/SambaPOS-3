﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.TicketModule
{
    public class TicketExplorerFilter : ITicketExplorerFilter
    {
        public TicketExplorerFilter()
        {
            FilterValues = new List<string>();
        }

        private readonly string[] _filterTypes = { Resources.OnlyOpenTickets, Resources.AllTickets, Resources.Account, Resources.Location };

        public int FilterTypeIndex
        {
            get { return (int)FilterType; }
            set
            {
                FilterType = (FilterType)value;
                FilterValue = "";

            }
        }

        public bool IsTextBoxEnabled { get { return FilterType != FilterType.OpenTickets; } }
        public string[] FilterTypes { get { return _filterTypes; } }

        public FilterType FilterType { get; set; }

        public string FilterValue { get; set; }

        public List<string> FilterValues { get; set; }

        public Expression<Func<Ticket, bool>> GetExpression()
        {
            Expression<Func<Ticket, bool>> result = null;

            if (FilterType == FilterType.AllTickets)
                result = x => x.Id > 0;

            if (FilterType == FilterType.OpenTickets)
                result = x => !x.IsPaid;

            //if (FilterType == FilterType.Target)
            //{
            //    if (FilterValue == "*")
            //        result = x => !string.IsNullOrEmpty(x.TargetAccountName);
            //    else if (!string.IsNullOrEmpty(FilterValue))
            //        result = x => x.TargetAccountName.ToLower() == FilterValue.ToLower();
            //    else result = x => string.IsNullOrEmpty(x.TargetAccountName);
            //}

            if (FilterType == FilterType.Account)
            {
                if (FilterValue == "*")
                    result = x => !string.IsNullOrEmpty(x.AccountName);
                else if (!string.IsNullOrEmpty(FilterValue))
                    result = x => x.AccountName.ToLower().Contains(FilterValue.ToLower());
                else
                    result = x => string.IsNullOrEmpty(x.AccountName);
            }

            return result;
        }
    }
}
