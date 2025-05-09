﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

		public const string StatusPending = "Pending";
		public const string StatusApproved = "Approved";
		public const string StatusInProcess = "Processing";
		public const string StatusShipped = "Shipped";
		public const string StatusCancelled = "Cancelled";
		public const string StatusRefunded = "Refunded";

		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected = "Rejected";
		
		public const string Buy = "Buy";
		public const string Borrow = "Borrow";

		public const string Delay = "Delay";
		public const string Corrupted = "Corrupted";
		
		public const string Fines = "Fines";
		public const string Orders = "Orders";
		public const string WishItems = "WishItems";
		public const string PendingFine = "pending";
		public const string PayedFine = "payed";
		
		
		
        public const string SessionCart = "SessionShoppingCart";


    }
}
