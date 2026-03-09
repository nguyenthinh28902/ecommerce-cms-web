using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Web.Shared.Models.Order.ViewModel.Transaction
{
    public class TransactionLogViewModel
    {
        public int Id { get; set; }
        public string LogContent { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

    }
}
