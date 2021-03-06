﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    public class SelectedOrderTagGroupViewModel : ObservableObject
    {
        private readonly IEnumerable<Order> _selectedOrders;

        public SelectedOrderTagGroupViewModel(OrderTagGroup model, IEnumerable<Order> selectedOrders)
        {
            Model = model;
            _selectedOrders = selectedOrders;
        }

        private ObservableCollection<OrderTagButtonViewModel> _orderTags;
        public ObservableCollection<OrderTagButtonViewModel> OrderTags { get { return _orderTags ?? (_orderTags = new ObservableCollection<OrderTagButtonViewModel>(GetOrderTags(_selectedOrders, Model))); } }

        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }

        public OrderTagGroup Model { get; private set; }
        public string Name { get { return Model.Name; } }

        private static IEnumerable<OrderTagButtonViewModel> GetOrderTags(IEnumerable<Order> selectedOrders, OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.Select(item => new OrderTagButtonViewModel(selectedOrders, baseModel, item));
        }

        public void Refresh()
        {
            foreach (var orderTagButtonViewModel in OrderTags)
            {
                orderTagButtonViewModel.Refresh();
            }
        }
    }
}
