namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class ChangePasswordRequest
    {
        public int UserId { get; set; }  // ID пользователя, меняющего пароль
        public string OldPassword { get; set; } // Старый пароль (для проверки)
        public string NewPassword { get; set; } // Новый пароль
    }





}
