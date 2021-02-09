using Notification.Data.Model;

namespace Notification.Data.Dto
{
    public enum MessageDtoKind
    {
        Info, Important, Critical
    }

    public class MessageDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ImageUri { get; set; }
        public string CircleUri { get; set; }
        public FeedbackKindDto FeedbackKind { get; set; }
        public MessageDtoKind Kind { get; set; }

        public Message ToMessage()
        {
            return new Message()
            {
                CircleImageUri = this.CircleUri,
                ImageUri = this.ImageUri,
                Text = this.Message,
                Title = this.Title
            };
        }
    }
}