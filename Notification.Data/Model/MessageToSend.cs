using System;

namespace Notification.Data.Model
{
	/// <summary>
	///  Message Contract
	/// </summary>
	public class MessageToSend
	{
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

		public string FromUser { get; set; }
		public string ToUser { get; set; }

	}
}
