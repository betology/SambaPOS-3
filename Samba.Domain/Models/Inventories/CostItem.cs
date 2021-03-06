﻿using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventories
{
    public class CostItem : Entity
    {
        public int PeriodicConsumptionId { get; set; }
        public virtual MenuItemPortion Portion { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrediction { get; set; }
        public decimal Cost { get; set; }
    }
}
