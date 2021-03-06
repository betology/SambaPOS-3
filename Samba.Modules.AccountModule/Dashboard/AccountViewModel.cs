﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountViewModel : EntityViewModelBase<Account>, IEntityCreator<Account>
    {
        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        private AccountTemplate _accountTemplate;
        public AccountTemplate AccountTemplate
        {
            get
            {
                return _accountTemplate ??
                       (_accountTemplate = Workspace.Single<AccountTemplate>(x => x.Id == Model.AccountTemplateId));
            }
            set
            {
                Model.AccountTemplateId = value.Id;
                _accountTemplate = null;
                RaisePropertyChanged(() => AccountTemplate);
            }
        }

        public override Type GetViewType()
        {
            return typeof(AccountView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        protected override AbstractValidator<Account> GetValidator()
        {
            return new AccountValidator();
        }

        public IEnumerable<Account> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateAccounts(data.ToArray(), Workspace);
        }
    }

    internal class AccountValidator : EntityValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.AccountTemplateId).GreaterThan(0);
        }
    }
}
