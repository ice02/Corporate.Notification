using System;
using System.Collections;
using System.Collections.Generic;

namespace Notification.Data.Model
{
	/// <summary>
	///  Message Contract
	/// </summary>
	public class Message
	{
		/// <summary>
		/// 
		/// </summary>
		public int MessageId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CircleImageUri { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ImageUri { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool WithTextFeedback { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime CreatedDate { get; set; }

		public virtual ICollection<NotificationUser> Users {get; set;}

		public int CampaignId { get; set; }
		public Campaign Campaign { get; set; }


	}
}
