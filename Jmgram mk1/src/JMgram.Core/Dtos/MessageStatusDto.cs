namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public enum MessageStatusDto
    {
        Sent,       // Отправлено, но не доставлено
        Delivered,  // Отправлено и доставлено
        Read        // Отправлено, доставлено и прочитано
    }

}
