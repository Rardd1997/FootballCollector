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

        public const string DefaultSoccerRegulations = @"`
Регламент
    10|12 людей -> 2 команди
    15|18 людей -> 3 команди - по 7хв|до 2-ох голів, нічия: сідає та, що грає довше
голи зі своєї половини поля не зараховуються!
якщо ти ліваєш за 24 години до гри і не знаходиш собі заміну, то всерівно скидуєшся після гри!!!
`";

        public const string ChatRulesFormat =
@"{0} Welcome!
Ми тут граємо, майже, щотижня. Приєднатись дуже просто!:
- Як тільки повідомлення про нову гру закріплене, потрібно у відповідь поставити '+' (потрібно саме відповісти на закріплене повідомлення).
- Якщо ти хочеш взяти з собою +1, тобі треба поставити + ще раз";

        public const string HelpMessage =
@"Команди доступні лише для адміністратора:
- створення гри: `/new_game address \""\"" date \""mm.dd.yyyy hh:mm:ss\"" durationInMins {0..9} cost {0..9} hasShower {false|true} hasChangingRoom {false|true} hasParking {false|true} type {Soccer|Volleyball}`
- оновлення гри: `/update_game address \""\"" date \""mm.dd.yyyy hh:mm:ss\"" durationInMins {0..9} cost {0..9} hasShower {false|true} hasChangingRoom {false|true} hasParking {false|true} type {Soccer|Volleyball}`
";

        public const string GameMessageFormat =
@"Граємо *{0:dd.MM.yyyy}* (*{1}*) початок о *{0:HH:mm}* {2}хв {3}
Адреса: _{4}_
бути за 10-15хв до початку
{5}
{6}
Список:
{7}
Резерв:
{8}";
    }
}