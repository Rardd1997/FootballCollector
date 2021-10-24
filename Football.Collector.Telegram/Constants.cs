namespace Football.Collector
{
    public static class Constants
    {
        public const string BearerSchemeType = "Bearer";
        public const string ApplicationJsonContentType = "application/json";

        public const string TelegramWebhookPath = "/api/telegram/message/update";

        public const string LoginPath = "/api/auth/login";
        
        public const string TelegramGamePath = "/api/telegram_game";
        public const string FindTelegramGamePath = "/api/telegram_game/find";
        public const string FindLastTelegramGamePath = "/api/telegram_game/last";

        public const string TelegramGamePlayerPath = "/api/telegram_game/player";

        public const string CreateTelegramUserPath = "/api/telegram_user";
        public const string FindTelegramUserPath = "/api/telegram_user/find";

        public const string TelegramChatUserPath = "/api/telegram_chat_user";
        public const string FindTelegramChatUserPath = "/api/telegram_chat_user/find";


        public static string ChatRulesFormat =
@"{0} Welcome!
Ми тут граємо, майже, щотижня у футбол. Приєднатись дуже просто!:
- Як тільки повідомлення про нову гру закріплене, потрібно у відповідь поставити '+' (потрібно саме відповісти на закріплене повідомлення).
- Гравці з попередньої гри автоматично переносяться на наступну в той же день (тобто з минулої НД на наступну НД). 
- Якщо ж ти грав у ЧТ, тоді на НД ти не попадаєш автоматично і потрібно ставати в чергу бажаючих.
- Якщо ж ти грав раніше, але наступного разу не будеш грати, потрібно у відповідь поставити '-' (потрібно саме відповісти на закріплене повідомлення).";

        public static string GameMessageFormat =
@"Бігаємо *{0:dd.MM.yyyy}* початок о *{0:hh:mm}* {1}хв ({2} грн)
Адреса: _{3}_
бути за 10-15хв до початку
душ, роздягальня і парковка є
`
Регламент
    15 людей -> 3 команди - по 7хв|до 2-ох голів, нічия: сідає та, що грає довше
    20 людей -> 4 команди - по 6хв|до 2-ох голів, нічия: сідають обидві команди
голи зі своєї половини поля не зараховуються!
якщо ти ліваєш в день гри і не знаходиш собі заміну, то всерівно скидуєшся після гри!!!
`
Список:
{4}
Резерв:
{5}";
    }
}
