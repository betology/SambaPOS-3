﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionDocument : Entity
    {
        public AccountTransactionDocument()
        {
            _accountTransactions = new List<AccountTransaction>();
            Date = DateTime.Now;
        }

        public DateTime Date { get; set; }

        private IList<AccountTransaction> _accountTransactions;
        public virtual IList<AccountTransaction> AccountTransactions
        {
            get { return _accountTransactions; }
            set { _accountTransactions = value; }
        }
    }
}
