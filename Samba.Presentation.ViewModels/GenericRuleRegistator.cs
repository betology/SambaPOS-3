﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Persistance.Data.Specification;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.ViewModels
{
    public static class GenericRuleRegistator
    {
        private static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        private static readonly ITicketService TicketService = ServiceLocator.Current.GetInstance<ITicketService>();
        private static readonly IApplicationState ApplicationState = ServiceLocator.Current.GetInstance<IApplicationState>();
        private static readonly IUserService UserService = ServiceLocator.Current.GetInstance<IUserService>();
        private static readonly ITriggerService TriggerService = ServiceLocator.Current.GetInstance<ITriggerService>();
        private static readonly IPrinterService PrinterService = ServiceLocator.Current.GetInstance<IPrinterService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly IAutomationService AutomationService = ServiceLocator.Current.GetInstance<IAutomationService>();
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        private static readonly IResourceService ResourceService = ServiceLocator.Current.GetInstance<IResourceService>();

        private static bool _registered;
        public static void RegisterOnce()
        {
            Debug.Assert(_registered == false);
            RegisterActions();
            RegisterRules();
            RegisterParameterSources();
            HandleEvents();
            RegisterNotifiers();
            _registered = true;
        }

        private static void RegisterActions()
        {
            AutomationService.RegisterActionType("SendEmail", Resources.SendEmail, new { SMTPServer = "", SMTPUser = "", SMTPPassword = "", SMTPPort = 0, ToEMailAddress = "", Subject = "", FromEMailAddress = "", EMailMessage = "", FileName = "", DeleteFile = false, BypassSslErrors = false });
            AutomationService.RegisterActionType("AddTicketDiscount", Resources.AddTicketDiscount, new { DiscountPercentage = 0m });
            AutomationService.RegisterActionType("AddOrder", Resources.AddOrder, new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "" });
            AutomationService.RegisterActionType("UpdateTicketTag", Resources.UpdateTicketTag, new { TagName = "", TagValue = "" });
            AutomationService.RegisterActionType("TagOrder", "Tag Order", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType("UntagOrder", "Untag Order", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterActionType("RemoveOrderTag", "Remove OrderTag", new { OrderTagName = "" });
            AutomationService.RegisterActionType("UpdatePriceTag", Resources.UpdatePriceTag, new { DepartmentName = "", PriceTag = "" });
            AutomationService.RegisterActionType("RefreshCache", Resources.RefreshCache);
            AutomationService.RegisterActionType("SendMessage", Resources.BroadcastMessage, new { Command = "" });
            AutomationService.RegisterActionType("UpdateProgramSetting", Resources.UpdateProgramSetting, new { SettingName = "", SettingValue = "", UpdateType = Resources.Update, IsLocal = true });
            AutomationService.RegisterActionType("UpdateTicketTax", Resources.UpdateTicketTax, new { TaxTemplate = "" });
            AutomationService.RegisterActionType("RegenerateTicketTax", Resources.RegenerateTicketTax);
            AutomationService.RegisterActionType("UpdateTicketService", Resources.UpdateTicketService, new { CalculationTemplate = "", Amount = 0m });
            AutomationService.RegisterActionType("UpdateTicketAccount", Resources.UpdateTicketAccount, new { AccountPhone = "", AccountName = "", Note = "" });
            AutomationService.RegisterActionType("ExecutePrintJob", "Execute Print Job", new { PrintJobName = "" });
            AutomationService.RegisterActionType("UpdateResourceState", "Update Resource State", new { ResourceId = 0, ResourceState = "" });
        }

        private static void RegisterRules()
        {
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedIn, Resources.UserLogin, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.UserLoggedOut, Resources.UserLogout, new { RoleName = "" });
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodStarts, Resources.WorkPeriodStarted);
            AutomationService.RegisterEvent(RuleEventNames.WorkPeriodEnds, Resources.WorkPeriodEnded);
            AutomationService.RegisterEvent(RuleEventNames.TriggerExecuted, Resources.TriggerExecuted, new { TriggerName = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketCreated, Resources.TicketCreated);
            AutomationService.RegisterEvent(RuleEventNames.TicketLocationChanged, Resources.TicketLocationChanged, new { OldLocation = "", NewLocation = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTagSelected, Resources.TicketTagSelected, new { TagName = "", TagValue = "", NumericValue = 0, TicketTag = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderTagged, "Order Tagged", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.OrderUntagged, "Order Untagged", new { OrderTagName = "", OrderTagValue = "" });
            AutomationService.RegisterEvent(RuleEventNames.AccountSelectedForTicket, Resources.AccountSelectedForTicket, new { AccountName = "", PhoneNumber = "", AccountNote = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketTotalChanged, Resources.TicketTotalChanged, new { TicketTotal = 0m, PreviousTotal = 0m, DiscountTotal = 0m, DiscountAmount = 0m, TipAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.MessageReceived, Resources.MessageReceived, new { Command = "" });
            AutomationService.RegisterEvent(RuleEventNames.TicketLineAdded, "Line Added to Ticket", new { MenuItemName = "" });
            AutomationService.RegisterEvent(RuleEventNames.ChangeAmountChanged, "Change Amount Updated", new { TicketAmount = 0, ChangeAmount = 0, TenderedAmount = 0 });
            AutomationService.RegisterEvent(RuleEventNames.TicketClosed, "Ticket Closed", new { AccountId = 0, RemainingAmount = 0m });
            AutomationService.RegisterEvent(RuleEventNames.ApplicationStarted, "Application Started");
            AutomationService.RegisterEvent(RuleEventNames.OrdersCreated, "Orders Created");
        }

        private static void RegisterParameterSources()
        {
            AutomationService.RegisterParameterSoruce("UserName", () => UserService.GetUserNames());
            AutomationService.RegisterParameterSoruce("DepartmentName", () => DepartmentService.GetDepartmentNames());
            AutomationService.RegisterParameterSoruce("TerminalName", () => SettingService.GetTerminalNames());
            AutomationService.RegisterParameterSoruce("TriggerName", () => Dao.Select<Trigger, string>(yz => yz.Name, y => !string.IsNullOrEmpty(y.Expression)));
            AutomationService.RegisterParameterSoruce("MenuItemName", () => Dao.Distinct<MenuItem>(yz => yz.Name));
            AutomationService.RegisterParameterSoruce("PriceTag", () => Dao.Distinct<MenuItemPriceDefinition>(x => x.PriceTag));
            AutomationService.RegisterParameterSoruce("Color", () => typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name));
            AutomationService.RegisterParameterSoruce("TaxTemplate", () => Dao.Distinct<TaxTemplate>(x => x.Name));
            AutomationService.RegisterParameterSoruce("CalculationTemplate", () => Dao.Distinct<CalculationTemplate>(x => x.Name));
            AutomationService.RegisterParameterSoruce("TagName", () => Dao.Distinct<TicketTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSoruce("OrderTagName", () => Dao.Distinct<OrderTagGroup>(x => x.Name));
            AutomationService.RegisterParameterSoruce("ResourceState", () => Dao.Distinct<ResourceState>(x => x.Name));
        }

        private static void ResetCache()
        {
            TriggerService.UpdateCronObjects();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
            ApplicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(x =>
            {
                if (x.Value.Action.ActionType == "UpdateResourceState")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        foreach (var ticketResource in ticket.TicketResources)
                        {
                            var stateName = x.Value.GetAsString("ResourceState");
                            var state = CacheService.GetResourceStateByName(stateName);
                            if (state != null)
                                ResourceService.UpdateResourceState(ticketResource.ResourceId, state.Id);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketAccount")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        Expression<Func<Resource, bool>> qFilter = null;

                        var phoneNumber = x.Value.GetAsString("AccountPhone");
                        var accountName = x.Value.GetAsString("AccountName");
                        var note = x.Value.GetAsString("Note");

                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            qFilter = y => y.SearchString == phoneNumber;
                        }

                        if (!string.IsNullOrEmpty(accountName))
                        {
                            if (qFilter == null) qFilter = y => y.Name == accountName;
                            else qFilter = qFilter.And(y => y.Name == accountName);
                        }

                        if (qFilter != null)
                        {
                            var account = Dao.Query(qFilter).FirstOrDefault();
                            if (account != null)
                                TicketService.UpdateResource(ticket, account);
                        }
                        else TicketService.UpdateResource(ticket, Resource.Null);
                    }
                }

                if (x.Value.Action.ActionType == "UpdateProgramSetting")
                {
                    var settingName = x.Value.GetAsString("SettingName");
                    var updateType = x.Value.GetAsString("UpdateType");
                    if (!string.IsNullOrEmpty(settingName))
                    {
                        var isLocal = x.Value.GetAsBoolean("IsLocal");
                        var setting = isLocal
                            ? SettingService.ReadLocalSetting(settingName)
                            : SettingService.ReadGlobalSetting(settingName);

                        if (updateType == Resources.Increase)
                        {
                            var settingValue = x.Value.GetAsInteger("SettingValue");
                            if (string.IsNullOrEmpty(setting.StringValue))
                                setting.IntegerValue = settingValue;
                            else
                                setting.IntegerValue = setting.IntegerValue + settingValue;
                        }
                        else if (updateType == Resources.Decrease)
                        {
                            var settingValue = x.Value.GetAsInteger("SettingValue");
                            if (string.IsNullOrEmpty(setting.StringValue))
                                setting.IntegerValue = settingValue;
                            else
                                setting.IntegerValue = setting.IntegerValue - settingValue;
                        }
                        else if (updateType == "Toggle")
                        {
                            var settingValue = x.Value.GetAsString("SettingValue");
                            var parts = settingValue.Split(',');
                            if (string.IsNullOrEmpty(setting.StringValue))
                            {
                                setting.StringValue = parts[0];
                            }
                            else
                            {
                                for (var i = 0; i < parts.Length; i++)
                                {
                                    if (parts[i] == setting.StringValue)
                                    {
                                        setting.StringValue = (i + 1) < parts.Length ? parts[i + 1] : parts[0];
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var settingValue = x.Value.GetAsString("SettingValue");
                            setting.StringValue = settingValue;
                        }
                        if (!isLocal) SettingService.SaveProgramSettings();
                    }
                }

                if (x.Value.Action.ActionType == "RefreshCache")
                {
                    MethodQueue.Queue("ResetCache", ResetCache);
                }

                if (x.Value.Action.ActionType == "SendMessage")
                {
                    AppServices.MessagingService.SendMessage("ActionMessage", x.Value.GetAsString("Command"));
                }

                if (x.Value.Action.ActionType == "SendEmail")
                {
                    EMailService.SendEMailAsync(x.Value.GetAsString("SMTPServer"),
                        x.Value.GetAsString("SMTPUser"),
                        x.Value.GetAsString("SMTPPassword"),
                        x.Value.GetAsInteger("SMTPPort"),
                        x.Value.GetAsString("ToEMailAddress"),
                        x.Value.GetAsString("FromEMailAddress"),
                        x.Value.GetAsString("Subject"),
                        x.Value.GetAsString("EMailMessage"),
                        x.Value.GetAsString("FileName"),
                        x.Value.GetAsBoolean("DeleteFile"),
                        x.Value.GetAsBoolean("BypassSslErrors"));
                }

                if (x.Value.Action.ActionType == "UpdateTicketTax")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var taxTemplateName = x.Value.GetAsString("TaxTemplate");
                        var taxTemplate = SettingService.GetTaxTemplateByName(taxTemplateName);
                        if (taxTemplate != null)
                        {
                            ticket.UpdateTax(taxTemplate);
                            TicketService.RecalculateTicket(ticket);
                            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketService")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var calculationTemplateName = x.Value.GetAsString("CalculationTemplate");
                        var calculationTemplate = SettingService.GetCalculationTemplateByName(calculationTemplateName);
                        if (calculationTemplate != null)
                        {
                            var amount = x.Value.GetAsDecimal("Amount");
                            ticket.AddCalculation(calculationTemplate, amount);
                            TicketService.RecalculateTicket(ticket);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "RegenerateTicketTax")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        TicketService.RegenerateTaxRates(ticket);
                        TicketService.RecalculateTicket(ticket);
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "AddTicketDiscount")
                {
                    //var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    //if (ticket != null)
                    //{
                    //    var percentValue = x.Value.GetAsDecimal("DiscountPercentage");
                    //    ticket.AddTicketDiscount(DiscountType.Percent, percentValue, ApplicationState.CurrentLoggedInUser.Id);
                    //    TicketService.RecalculateTicket(ticket);
                    //}
                }

                if (x.Value.Action.ActionType == "AddOrder")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");

                    if (ticket != null)
                    {
                        var menuItemName = x.Value.GetAsString("MenuItemName");
                        var menuItem = CacheService.GetMenuItem(y => y.Name == menuItemName);
                        var portionName = x.Value.GetAsString("PortionName");
                        var quantity = x.Value.GetAsDecimal("Quantity");
                        var tag = x.Value.GetAsString("Tag");

                        var ti = ticket.AddOrder(
                                 ApplicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate,
                                 ApplicationState.CurrentLoggedInUser.Name, menuItem, portionName,
                                 ApplicationState.CurrentDepartment.PriceTag);

                        ti.Quantity = quantity;
                        ti.Tag = tag;

                        TicketService.RecalculateTicket(ticket);

                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "UpdateTicketTag")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    if (ticket != null)
                    {
                        var tagName = x.Value.GetAsString("TagName");
                        var tagValue = x.Value.GetAsString("TagValue");
                        ticket.SetTagValue(tagName, tagValue);
                        var tagData = new TicketTagData { TagName = tagName, TagValue = tagValue };
                        tagData.PublishEvent(EventTopicNames.TagSelectedForSelectedTicket);
                    }
                }

                if (x.Value.Action.ActionType == "TagOrder" || x.Value.Action.ActionType == "UntagOrder" || x.Value.Action.ActionType == "RemoveOrderTag")
                {
                    var order = x.Value.GetDataValue<Order>("Order");
                    if (order != null)
                    {
                        var tagName = x.Value.GetAsString("OrderTagName");
                        var orderTag = ApplicationState.CurrentDepartment.TicketTemplate.OrderTagGroups.SingleOrDefault(y => y.Name == tagName);
                        if (x.Value.Action.ActionType == "RemoveOrderTag")
                        {
                            var tags = order.OrderTagValues.Where(y => y.OrderTagGroupId == orderTag.Id);
                            tags.ToList().ForEach(y => order.OrderTagValues.Remove(y));
                            return;
                        }
                        var tagValue = x.Value.GetAsString("OrderTagValue");
                        if (orderTag != null)
                        {
                            var orderTagValue = orderTag.OrderTags.SingleOrDefault(y => y.Name == tagValue);
                            if (orderTagValue != null)
                            {
                                if (x.Value.Action.ActionType == "TagOrder")
                                    order.TagIfNotTagged(orderTag, orderTagValue, ApplicationState.CurrentLoggedInUser.Id);
                                if (x.Value.Action.ActionType == "UntagOrder")
                                    order.UntagIfTagged(orderTag, orderTagValue, ApplicationState.CurrentLoggedInUser.Id);
                            }
                        }
                    }
                }

                if (x.Value.Action.ActionType == "UpdatePriceTag")
                {
                    using (var workspace = WorkspaceFactory.Create())
                    {
                        var priceTag = x.Value.GetAsString("PriceTag");
                        var departmentName = x.Value.GetAsString("DepartmentName");
                        var department = workspace.Single<Department>(y => y.Name == departmentName);
                        if (department != null)
                        {
                            department.PriceTag = priceTag;
                            workspace.CommitChanges();
                            MethodQueue.Queue("ResetCache", ResetCache);
                        }
                    }
                }

                if (x.Value.Action.ActionType == "ExecutePrintJob")
                {
                    var ticket = x.Value.GetDataValue<Ticket>("Ticket");
                    var pjName = x.Value.Action.GetParameter("PrintJobName");
                    if (!string.IsNullOrEmpty(pjName))
                    {
                        var j = ApplicationState.CurrentTerminal.PrintJobs.SingleOrDefault(y => y.Name == pjName);

                        if (j != null)
                        {
                            if (ticket != null)
                                PrinterService.ManualPrintTicket(ticket, j);
                            else
                                PrinterService.ExecutePrintJob(j);
                        }
                    }
                }
            });
        }

        private static void RegisterNotifiers()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == "ActionMessage")
                {
                    AutomationService.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });
        }
    }
}
