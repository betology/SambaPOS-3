﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Services.Common;

namespace Samba.Presentation.Common.Services
{
    public static class PresentationServices
    {
        public static ObservableCollection<ICaptionCommand> OrderCommands { get; private set; }
        public static ObservableCollection<ICaptionCommand> TicketCommands { get; private set; }
        public static ObservableCollection<ICategoryCommand> NavigationCommandCategories { get; private set; }
        public static ObservableCollection<DashboardCommandCategory> DashboardCommandCategories { get; private set; }

        public static void Initialize()
        {
            NavigationCommandCategories = new ObservableCollection<ICategoryCommand>();
            DashboardCommandCategories = new ObservableCollection<DashboardCommandCategory>();
            OrderCommands = new ObservableCollection<ICaptionCommand>();
            TicketCommands = new ObservableCollection<ICaptionCommand>();

            EventServiceFactory.EventService.GetEvent<GenericEvent<ICategoryCommand>>().Subscribe(OnCommandAdded);
            EventServiceFactory.EventService.GetEvent<GenericEvent<ICaptionCommand>>().Subscribe(OnTicketCommandAdded);
        }

        private static void OnTicketCommandAdded(EventParameters<ICaptionCommand> obj)
        {
            if(obj.Topic==EventTopicNames.AddCustomTicketCommand)
                TicketCommands.Add(obj.Value);
            if (obj.Topic == EventTopicNames.AddCustomOrderCommand)
                OrderCommands.Add(obj.Value);
        }

        private static void OnCommandAdded(EventParameters<ICategoryCommand> result)
        {
            if (result.Topic == EventTopicNames.NavigationCommandAdded)
                NavigationCommandCategories.Add(result.Value);

            if (result.Topic == EventTopicNames.DashboardCommandAdded)
            {
                var cat = DashboardCommandCategories.FirstOrDefault(item => item.Category == result.Value.Category);
                if (cat == null)
                {
                    cat = new DashboardCommandCategory(result.Value.Category);
                    DashboardCommandCategories.Add(cat);
                }
                if (result.Value.Order > cat.Order)
                    cat.Order = result.Value.Order;
                cat.AddCommand(result.Value);
            }
        }
    }
}
