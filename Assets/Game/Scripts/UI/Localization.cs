using System.Collections.Generic;
namespace Shorok
{
    public sealed class Localization
    {
        public string Language = "ru";
        static readonly Dictionary<string, string[]> table;
        static Localization() { table = Build(); }
        static Dictionary<string, string[]> Build()
        {
            var result = new Dictionary<string, string[]>();
            foreach (var line in Lines) { var p = line.Split('|'); result.Add(p[0], new[] { p[1], p[2] }); }
            return result;
        }
        public string Get(string key) => table.TryGetValue(key, out var pair) ? pair[Language == "ru" ? 0 : 1] : key;
        public static bool Contains(string key) => table.ContainsKey(key);
        static readonly string[] Lines = {
            "menu.continue|Продолжить|Continue", "menu.new|Новая игра|New game", "menu.settings|Настройки|Settings", "menu.how|Как играть|How to play",
            "menu.tagline|У каждого дома есть голос.|Every house has a voice.", "menu.subtitle|Разбуди тихий дом|Wake the quiet house", "menu.chapter|ПЯТЬ КОМНАТ ПАМЯТИ|FIVE ROOMS OF MEMORY",
            "menu.confirm|Начать заново? Текущий прогресс будет заменён.|Start again? Current progress will be replaced.", "yes|Начать|Start", "no|Назад|Back",
            "pause.title|ТИШИНА|A MOMENT OF QUIET", "pause.restart|Перезапустить комнату|Restart room", "pause.checkpoint|К контрольной точке|Return to checkpoint", "pause.menu|В главное меню|Main menu",
            "settings.master|Общая громкость|Master volume", "settings.music|Музыка|Music", "settings.sfx|Звуки|Sound effects", "settings.mute|Без звука|Mute", "settings.shake|Дрожание камеры|Screen shake", "settings.touch|Экранные кнопки|Touch buttons", "settings.language|Язык: Русский|Language: English",
            "on|Вкл|On", "off|Выкл|Off", "loading|Дом вспоминает...|The house remembers...", "orientation|Поверните устройство|Rotate your device", "orientation.sub|Дому нужно чуть больше места.|The house needs a little more room.",
            "hud.memory|ВОСПОМИНАНИЯ|MEMORIES", "hint.possess|E  Вселиться|E  Possess", "hint.exit|E  Выйти|E  Leave", "hint.exit_blocked|Нет места для выхода. Сдвиньте предмет.|No safe exit. Move the object first.",
            "action.toggle|F  Включить / выключить|F  Switch on / off", "action.fan|F  Поток   Q  Направление|F  Wind   Q  Direction", "action.drive|A / D  Ехать   E  Выйти|A / D  Drive   E  Leave", "action.heavy|A / D  Двигаться   F  Топнуть|A / D  Move   F  Stomp", "action.clock|F  Замедлить механизмы|F  Slow the mechanisms", "action.music|F  Музыкальная волна|F  Play a melody",
            "how.body|A / D или стрелки — движение\nПробел / W / вверх — прыжок\nУдерживайте прыжок для лёгкого парения\nE — вселиться / выйти\nF — способность предмета, Q — направление\nEsc — пауза, удерживать R — сброс комнаты\n\nНайдите 20 осколков памяти и разбудите часы.\nВсе загадки решаются без рекламы.|A / D or arrows — move\nSpace / W / up — jump\nHold jump for a little float\nE — possess / leave an object\nF — object ability, Q — direction\nEsc — pause, hold R — reset this room\n\nFind 20 memories and awaken the clock.\nEvery puzzle can be solved without ads.",
            "memory.found|Дом вспомнил ещё одну мелочь.|The house remembers one more thing.", "room.reset|Вещи вернулись на свои места.|Everything is back in its place.",
            "ad.hint|Подсказка за видео|Watch video for a hint", "ad.detail|После просмотра: подсветка нужного предмета.|Reward: highlight the object you need.", "ad.unavailable|Видео недоступно. Можно продолжать играть.|No video available. You can keep playing.",
            "ending.title|ДОМ ПРОСНУЛСЯ|THE HOUSE AWAKENS", "ending.body|Тишина стала музыкой.\nА за окнами — утро.|The silence became music.\nAnd outside, morning.", "ending.extra|Каждая вещь помнит тепло рук.\nТеперь дом помнит и тебя.|Every object remembers a gentle touch.\nNow the house remembers you, too.", "ending.revisit|Вернуться за воспоминаниями|Revisit the house", "ending.choose|Выберите область|Choose an area",
            "hint.awakening|A / D — двигаться. Пробел — прыгать.|A / D to move. Space to jump.",
            "hint.lamp|Свет помнит то, что скрылось. Включите лампу и выйдите из неё.|Light remembers hidden things. Switch on the lamp, then leave it.",
            "hint.car|Машинка пройдёт под шкафом. Найдите пластину на дальней стороне.|The car fits beneath the cupboard. Reach the far pressure plate.",
            "hint.fan|Включите вентилятор. Выйдите и прыгните в поток воздуха.|Switch on the fan. Leave it and jump into the rising air.",
            "hint.clock|Замедлите часы. Механизмы станут безопаснее, ваш прыжок — неторопливее не станет.|Slow the clock. The mechanisms slow down; you keep your own rhythm.",
            "hint.heavy|Тяжёлая фигурка удержит пластину и станет ступенькой.|The heavy figure can hold the plate and become a step.",
            "hint.music|Музыка создаёт мост на 18 секунд. Мелодию можно повторить.|Music makes a bridge for 18 seconds. You can play it again.",
            "hint.lampcar|Лампа проявит дорогу для машинки. Пластина откроет большой проход.|Light reveals the car route. Its pressure plate opens the tall passage.",
            "hint.fancrate|Направьте ветер к коробке: она нажмёт пластину. Затем поднимитесь на потоке.|Blow the crate onto its plate, then ride the rising air.",
            "hint.clockheavy|Замедлите механизм и проведите фигурку к пластине.|Slow the mechanism and guide the heavy figure onto the plate.",
            "hint.finale|Машинка освободит сердцевину часов. Потом поднимитесь к светлому колесу.|The car releases the clock's spindle. Then climb to the pale wheel.",
            "object.lamp|Лампа|Lamp", "object.fan|Вентилятор|Fan", "object.car|Машинка|Toy car", "object.clock|Часы|Clock", "object.heavy|Фигурка|Heavy figure", "object.music|Шкатулка|Music box",
            "touch.jump|Прыжок|Jump", "touch.possess|Вселиться|Possess", "touch.leave|Выйти|Leave", "touch.action|Действие|Action", "touch.reverse|Поворот|Reverse"
            ,"hint.jump|Пробел / W — прыжок. Удерживайте для лёгкого парения.|Space / W to jump. Hold for a little float."
        };
    }
}
