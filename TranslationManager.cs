using System.Collections.Generic;

namespace SCPCanvasPaint
{
    public class TranslationManager
    {
        public static TranslationManager Instance { get; } = new TranslationManager();
        public bool IsEnglish { get; set; } = false;

        private readonly Dictionary<string, string> ruStrings = new Dictionary<string, string>();
        private readonly Dictionary<string, string> enStrings = new Dictionary<string, string>();

        private TranslationManager()
        {
            InitRussian();
            InitEnglish();
        }

        public string Get(string key)
        {
            if (IsEnglish && enStrings.TryGetValue(key, out string enVal)) return enVal;
            if (ruStrings.TryGetValue(key, out string ruVal)) return ruVal;
            return key; 
        }

        private void InitRussian()
        {
            ruStrings["brush_desc"] = "[Кисть художника] Используйте для рисования на холстах. Выбросьте для включения режима.";
            ruStrings["brush_size_desc"] = "Изменить размер мазка кисти от 1 до 5";
            ruStrings["need_mode"] = "Сначала включите режим рисования медпакетом.";
            ruStrings["brush_usage"] = "Укажите размер кисти от 1 до 5. Пример: .brush 3";
            ruStrings["brush_success"] = "Размер мазка кисти успешно изменен на {0}x{0} пикселей!";
            ruStrings["canvas_desc"] = "Управление холстами для рисования";
            ruStrings["canvas_usage"] = "Используйте: .canvas [spawn/save/load/delete/permission/url/gif/clearanims/brush/color/eraser/pipette/trust/undo/redo/text/grab/lang]";
            ruStrings["clearanims_desc"] = "Удалить абсолютно все холсты с GIF-анимациями на карте";
            ruStrings["only_admins_anims"] = "Только администраторы могут массово удалять анимации!";
            ruStrings["no_anims"] = "На карте не найдено активных GIF-анимаций.";
            ruStrings["anims_removed"] = "Успешно удалено холстов с анимациями: {0} шт.";
            ruStrings["color_desc"] = "Задать точный HEX-код цвета кисти и сохранить в свою палитру";
            ruStrings["color_usage"] = "Укажите HEX-код цвета. Пример: .color #FF5733";
            ruStrings["color_success"] = "Цвет успешно изменен на кастомный HEX: {0}! Он добавлен в вашу палитру прокрутки.";
            ruStrings["color_selected"] = "Выбран цвет: <color={0}>Кастомный █</color>";
            ruStrings["color_fail"] = "Не удалось распознать HEX-код. Используйте формат #RRGGBB или #RGB.";
            ruStrings["delete_desc"] = "Удалить холст по взгляду (доступно владельцу или админу)";
            ruStrings["only_players"] = "Только для игроков!";
            ruStrings["look_at_canvas"] = "Вы должны смотреть на холст, чтобы удалить его!";
            ruStrings["delete_own_only"] = "Вы можете удалять только свои холсты! Админы могут удалять любые.";
            ruStrings["delete_success"] = "Холст успешно удален!";
            ruStrings["eraser_desc"] = "Переключить кисть в режим ластика и обратно";
            ruStrings["eraser_on"] = "Включен ЛАСТИК (стирание примитивов)!";
            ruStrings["eraser_off"] = "Включена обычная КИСТЬ!";
            ruStrings["gif_desc"] = "Установить GIF-анимацию на холст, на который вы смотрите: .canvas gif <ссылка>";
            ruStrings["gif_url_needed"] = "Укажите прямую URL ссылку на GIF файл!";
            ruStrings["must_look_canvas"] = "Вы должны смотреть на холст!";
            ruStrings["canvas_not_found"] = "Холст не найден!";
            ruStrings["gif_private_error"] = "Вы не можете устанавливать анимации на чужие приватные холсты!";
            ruStrings["gif_processing"] = "Запрос отправлен! Дождитесь скачивания и обработки кадров GIF...";
            ruStrings["load_desc"] = "Загрузить холст из файла перед собой: .canvas load <имя_файла>";
            ruStrings["only_admins_load"] = "Только администраторы могут загружать холсты!";
            ruStrings["load_name_needed"] = "Укажите имя файла для загрузки!";
            ruStrings["load_success"] = "Холст '{0}' успешно найден и начинает рендериться!";
            ruStrings["load_fail"] = "Файл с сохраненным холстом 'SavedCanvases/{0}.json' не существует.";
            ruStrings["perm_desc"] = "Управление правами создания холстов: .canvas permission [all/none/ник]";
            ruStrings["no_admin_perms"] = "У вас нет прав администратора!";
            ruStrings["perm_usage"] = "Используйте: .canvas permission [all / none / ник]";
            ruStrings["perm_all"] = "Права выданы ВСЕМ игрокам.";
            ruStrings["perm_none"] = "Права обычных игроков сброшены. Доступ только у админов.";
            ruStrings["player_not_found"] = "Игрок '{0}' не найден.";
            ruStrings["perm_revoked"] = "Права на создание холстов удалены у {0}.";
            ruStrings["perm_granted"] = "Игрок {0} получил права на создание холстов!";
            ruStrings["pipette_desc"] = "Скопировать цвет пикселя, на который вы смотрите";
            ruStrings["pipette_success"] = "Цвет успешно скопирован: {0}!";
            ruStrings["pipette_hint"] = "Пипетка: <color={0}>█</color>";
            ruStrings["pipette_fail"] = "Вы должны смотреть на пиксель холста!";
            ruStrings["redo_desc"] = "Повторить отмененное действие";
            ruStrings["redo_empty"] = "Нет действий для повтора!";
            ruStrings["canvas_destroyed"] = "Холст больше не существует.";
            ruStrings["redo_success"] = "Действие возвращено!";
            ruStrings["save_desc"] = "Сохранить холст по взгляду: .canvas save <имя_файла>";
            ruStrings["only_admins_save"] = "Только администраторы могут сохранять холсты!";
            ruStrings["save_name_needed"] = "Укажите имя для сохранения!";
            ruStrings["save_success"] = "Холст успешно сохранен в SavedCanvases/{0}.json!";
            ruStrings["save_look_at"] = "Вы должны смотреть на холст, чтобы сохранить его!";
            ruStrings["spawn_desc"] = "За спавнить холст перед собой: .canvas spawn <размер>";
            ruStrings["spawn_no_perms"] = "У вас нет прав на создание холстов!";
            ruStrings["spawn_usage"] = "Используйте: .canvas spawn <ширина_сетки> <высота_сетки> <физ_размер> <public/private>";
            ruStrings["spawn_invalid_params"] = "Укажите корректные числовые параметры сетки и размера.";
            ruStrings["spawn_limit_matrix"] = "Размер матрицы превышает лимит сервера ({0})!";
            ruStrings["spawn_limit_phys"] = "Физический размер превышает лимит сервера ({0}м)!";
            ruStrings["spawn_success"] = "Прямоугольный холст {0}x{1} ({2}м, доступ: {3}) создается. Кисть выдана.";
            ruStrings["trust_desc"] = "Разрешить игроку рисовать на ваших приватных холстах";
            ruStrings["trust_no_canvas"] = "У вас должен быть хотя бы один созданный приватный холст, чтобы использовать эту команду!";
            ruStrings["trust_usage"] = "Используйте: .canvas trust [ник/id]";
            ruStrings["trust_self"] = "Вы не можете добавить самого себя в белый список.";
            ruStrings["trust_removed"] = "Игрок {0} удален из вашего белого списка художников.";
            ruStrings["trust_revoked_hint"] = "<color=red>{0} запретил вам рисовать на его холстах.</color>";
            ruStrings["trust_granted"] = "Игрок {0} теперь может рисовать на всех ваших приватных холстах!";
            ruStrings["trust_granted_hint"] = "<color=green>{0} разрешил вам рисовать на его приватных холстах!</color>";
            ruStrings["undo_desc"] = "Отменить последнее действие";
            ruStrings["undo_empty"] = "История действий пуста!";
            ruStrings["undo_success"] = "Действие отменено!";
            ruStrings["url_desc"] = "Заспавнить холст по прямой URL-ссылке на картинку: .canvas url <размер_матрицы> <физ_размер> <ссылка>";
            ruStrings["only_admins_url"] = "Только администраторы могут импортировать картинки по URL!";
            ruStrings["url_usage"] = "Используйте: .canvas url <размер_сетки> <физический_размер> <прямая_ссылка_на_картинку>";
            ruStrings["url_success"] = "Запрос отправлен на сервер. Картинка скачивается и скоро отрендерится перед вами...";
            ruStrings["mode_on_hint"] = "<color=green>Режим рисования включен!</color>";
            ruStrings["mode_off_hint"] = "<color=red>Режим рисования выключен!</color>";
            ruStrings["private_canvas_error"] = "<color=red>Этот холст приватный! Вы не можете на нем рисовать.</color>";
            ruStrings["private_erase_error"] = "<color=red>Нельзя стирать пиксели на чужом приватном холсте!</color>";
            ruStrings["help_desc"] = "Показать справку по командам и возможностям рисования на холсте";

            ruStrings["help_text"] = "\n" +
    "==================================================\n" +
    "🎨   SCPCanvasPaint — РУКОВОДСТВО ХУДОЖНИКА   🎨\n" +
    "==================================================\n\n" +
    "📦 [УПРАВЛЕНИЕ ХОЛСТАМИ (Консоль)]:\n" +
    "  • .canvas spawn <ширина> <высота> <физ_размер> <public/private>\n" +
    "                             — Создать пустой прямоугольный холст перед собой.\n" +
    "                             Пример: .canvas spawn 30 10 2.5 public\n" +
    "  • .canvas text (текст) [цвет_текста] [размер] [цвет_холста] [public/private]\n" +
    "                             — Создать холст точных пропорций под указанный текст (8x8).\n" +
    "                             Пример: .canvas text Привет! #00FF00 3 #FF0000 private\n" +
    "  • .canvas grab             — Захватить холст взглядом для перемещения за камерой (360°).\n" +
    "                             Повторный ввод команды зафиксирует холст на месте.\n" +
    "  • .canvas url <базовый_размер> <физ_размер> <ссылка>\n" +
    "                             — Скачать картинку/GIF, авто-подстроив пропорции (Админы).\n" +
    "                             Пример: .canvas url 40 5.0 https://imgur.com\n" +
    "  • .canvas delete         — Удалить холст, на который вы смотрите (Владелец или админ).\n" +
    "  • .canvas clearanims     — Удалить ВСЕ холсты с GIF на карте (Только админы).\n" +
    "  • .canvas save <имя>     — Сохранить рисунок в файл (Только для админов).\n" +
    "  • .canvas load <имя>     — Загрузить сохраненный холст (Только для админов).\n" +
    "  • .canvas permission <all/none/ник> \n" +
    "                             — Настройка прав спавна для обычных игроков (Только админы).\n\n" +
    "🖌️ [ИНСТРУМЕНТЫ РИСОВАНИЯ (Консоль)]:\n" +
    "  • .canvas color <HEX-код> — Задать цвет кисти и сохранить в палитру. Пример: .canvas color #FF5733\n" +
    "  • .canvas pipette         — Скопировать цвет пикселя, на который вы смотрите (Пипетка).\n" +
    "  • .canvas trust <ник/id>  — Дать/забрать доступ к вашим приватным холстам.\n" +
    "  • .canvas brush <1-5>    — Изменить размер мазка кисти. Пример: .canvas brush 3\n" +
    "  • .canvas eraser         — Переключить кисть в режим ЛАСТИКА и обратно.\n" +
    "  • .canvas undo / .redo   — Отменить / вернуть последнее действие.\n" +
    "  • .canvas lang            — Переключить язык плагина (RU/EN).\n\n" +
    "🎮 [УПРАВЛЕНИЕ ПРЕДМЕТОМ (Кисть художника)]:\n" +
    "  • Включение / Выключение: Возьмите Кисть Художника в руки и нажмите 'G' (Выбросить).\n" +
    "  • Рисование взглядом   : Удерживайте Кисть Художника в руках, чтобы рисовать по холсту.\n" +
    "  • Смена цвета по кругу : Пока кисть активна, зажмите ЛКМ (Использовать). Прокручивает и кастомный HEX!\n" +
    "  • Перетаскивание (Grab) : Работает только тогда, когда кисть находится у вас в руках!\n" +
    "  • Ограничения          : На приватных холстах рисовать, заливать, двигать и менять GIF могут только\n" +
    "                             создатель, администраторы или доверенные лица из белого списка (.trust)!\n" +
    "==================================================\n";

            ruStrings["grab_desc"] = "Взять или отпустить холст перед собой";
            ruStrings["grab_release"] = "Вы успешно отпустили и зафиксировали холст!";
            ruStrings["grab_need_brush"] = "Для захвата холста необходимо держать кисть художника в руках!";
            ruStrings["grab_look_at"] = "Вы должны смотреть на холст, чтобы схватить его!";
            ruStrings["grab_precision"] = "Наведитесь точнее на пиксель холста!";
            ruStrings["grab_already_holding"] = "Этот холст уже перетаскивает другой игрок!";
            ruStrings["grab_private_error"] = "Вы не можете двигать этот холст, так как он приватный!";
            ruStrings["grab_success"] = "Холст захвачен! Он будет следовать за движениями вашей камеры, пока вы держите кисть.";

            ruStrings["text_desc"] = "Создать холст с текстом: .canvas text (текст) [цвет_текста] [физ_размер] [цвет_холста] [public/private]";
            ruStrings["text_too_long"] = "Текст слишком длинный и превышает лимиты разрешения матрицы!";
            ruStrings["text_success"] = "Холст с текстом успешно сгенерирован ({0}x{1} пикселей, доступ: {2}).";
        }

        private void InitEnglish()
        {
            enStrings["brush_desc"] = "[Artist Brush] Use it to paint on canvases. Drop it ('G') to enable the mode.";
            enStrings["brush_size_desc"] = "Change brush stroke size from 1 to 5";
            enStrings["need_mode"] = "First, enable painting mode using a medkit.";
            enStrings["brush_usage"] = "Specify a brush size from 1 to 5. Example: .brush 3";
            enStrings["brush_success"] = "Brush stroke size successfully changed to {0}x{0} pixels!";
            enStrings["canvas_desc"] = "Canvas management for painting";
            enStrings["canvas_usage"] = "Use: .canvas [spawn/save/load/delete/permission/url/gif/clearanims/brush/color/eraser/pipette/trust/undo/redo/text/grab/lang]";
            enStrings["clearanims_desc"] = "Delete absolutely all canvases with GIF animations on the map";
            enStrings["only_admins_anims"] = "Only administrators can mass-delete animations!";
            enStrings["no_anims"] = "No active GIF animations found on the map.";
            enStrings["anims_removed"] = "{0} canvases with animations successfully removed.";
            enStrings["color_desc"] = "Set a precise brush color HEX code and save to your palette";
            enStrings["color_usage"] = "Specify a HEX color code. Example: .color #FF5733";
            enStrings["color_success"] = "Color successfully changed to custom HEX: {0}! It has been added to your scroll palette.";
            enStrings["color_selected"] = "Selected color: <color={0}>Custom █</color>";
            enStrings["color_fail"] = "Failed to parse HEX code. Use #RRGGBB or #RGB format.";
            enStrings["delete_desc"] = "Delete the canvas you are looking at (available to owner or admin)";
            enStrings["only_players"] = "Players only!";
            enStrings["look_at_canvas"] = "You must be looking at a canvas to delete it!";
            enStrings["delete_own_only"] = "You can only delete your own canvases! Admins can delete any.";
            enStrings["delete_success"] = "Canvas successfully deleted!";
            enStrings["eraser_desc"] = "Toggle brush to eraser mode and back";
            enStrings["eraser_on"] = "ERASER enabled (primitive erasing)!";
            enStrings["eraser_off"] = "Regular BRUSH enabled!";
            enStrings["gif_desc"] = "Apply a GIF animation to the canvas you are looking at: .canvas gif <url>";
            enStrings["gif_url_needed"] = "Please provide a direct URL link to the GIF file!";
            enStrings["must_look_canvas"] = "You must be looking at a canvas!";
            enStrings["canvas_not_found"] = "Canvas not found!";
            enStrings["gif_private_error"] = "You cannot apply animations to someone else's private canvases!";
            enStrings["gif_processing"] = "Request sent! Please wait for the GIF frames to download and process...";
            enStrings["load_desc"] = "Load a canvas from a file in front of you: .canvas load <file_name>";
            enStrings["only_admins_load"] = "Only administrators can load canvases!";
            enStrings["load_name_needed"] = "Please specify a file name to load!";
            enStrings["load_success"] = "Canvas '{0}' successfully found and is starting to render!";
            enStrings["load_fail"] = "Saved canvas file 'SavedCanvases/{0}.json' does not exist.";
            enStrings["perm_desc"] = "Manage canvas creation permissions: .canvas permission [all/none/nickname]";
            enStrings["no_admin_perms"] = "You do not have administrator permissions!";
            enStrings["perm_usage"] = "Use: .canvas permission [all / none / nickname]";
            enStrings["perm_all"] = "Permissions granted to ALL players.";
            enStrings["perm_none"] = "Regular players' permissions reset. Access restricted to Admins only.";
            enStrings["player_not_found"] = "Player '{0}' not found.";
            enStrings["perm_revoked"] = "Canvas creation permissions revoked from {0}.";
            enStrings["perm_granted"] = "Player {0} has been granted canvas creation permissions!";
            enStrings["pipette_desc"] = "Copy the color of the pixel you are looking at";
            enStrings["pipette_success"] = "Color successfully copied: {0}!";
            enStrings["pipette_hint"] = "Pipette: <color={0}>█</color>";
            enStrings["pipette_fail"] = "You must be looking at a canvas pixel!";
            enStrings["redo_desc"] = "Redo the undone action";
            enStrings["redo_empty"] = "No actions to redo!";
            enStrings["canvas_destroyed"] = "Canvas no longer exists.";
            enStrings["redo_success"] = "Action redone!";
            enStrings["save_desc"] = "Save the canvas you are looking at: .canvas save <file_name>";
            enStrings["only_admins_save"] = "Only administrators can save canvases!";
            enStrings["save_name_needed"] = "Please specify a name to save!";
            enStrings["save_success"] = "Canvas successfully saved to SavedCanvases/{0}.json!";
            enStrings["save_look_at"] = "You must be looking at a canvas to save it!";
            enStrings["spawn_desc"] = "Spawn a canvas in front of you: .canvas spawn <size>";
            enStrings["spawn_no_perms"] = "You do not have permissions to create canvases!";
            enStrings["spawn_usage"] = "Use: .canvas spawn <grid_width> <grid_height> <phys_size> <public/private>";
            enStrings["spawn_invalid_params"] = "Please provide valid numerical grid and size parameters.";
            enStrings["spawn_limit_matrix"] = "Matrix size exceeds server limit ({0})!";
            enStrings["spawn_limit_phys"] = "Physical size exceeds server limit ({0}m)!";
            enStrings["spawn_success"] = "Rectangular canvas {0}x{1} ({2}m, access: {3}) is being created. Brush granted.";
            enStrings["trust_desc"] = "Allow a player to paint on your private canvases";
            enStrings["trust_no_canvas"] = "You must own at least one private canvas to use this command!";
            enStrings["trust_usage"] = "Use: .canvas trust [nickname/id]";
            enStrings["trust_self"] = "You cannot whitelist yourself.";
            enStrings["trust_removed"] = "Player {0} removed from your artist whitelist.";
            enStrings["trust_revoked_hint"] = "<color=red>{0} has forbidden you to paint on their canvases.</color>";
            enStrings["trust_granted"] = "Player {0} can now paint on all of your private canvases!";
            enStrings["trust_granted_hint"] = "<color=green>{0} has allowed you to paint on their private canvases!</color>";
            enStrings["undo_desc"] = "Undo the last action";
            enStrings["undo_empty"] = "Action history is empty!";
            enStrings["undo_success"] = "Action undone!";
            enStrings["url_desc"] = "Spawn a canvas via direct image URL: .canvas url <matrix_size> <phys_size> <url-link>";
            enStrings["only_admins_url"] = "Only administrators can import images via URL!";
            enStrings["url_usage"] = "Use: .canvas url <grid_size> <physical_size> <direct_image_url>";
            enStrings["url_success"] = "Request sent to server. The image is downloading and will render in front of you shortly...";
            enStrings["mode_on_hint"] = "<color=green>Painting mode enabled!</color>";
            enStrings["mode_off_hint"] = "<color=red>Painting mode disabled!</color>";
            enStrings["private_canvas_error"] = "<color=red>This canvas is private! You cannot paint on it.</color>";
            enStrings["private_erase_error"] = "<color=red>You cannot erase pixels on someone else's private canvas!</color>";
            enStrings["help_desc"] = "Show help for commands and canvas painting features";

            enStrings["help_text"] = "\n" +
                "==================================================\n" +
                "🎨   SCPCanvasPaint — ARTIST GUIDE   🎨\n" +
                "==================================================\n\n" +
                "📦 [CANVAS MANAGEMENT (Console)]:\n" +
                "  • .canvas spawn <width> <height> <phys_size> <public/private>\n" +
                "                             — Create an empty rectangular canvas in front of you.\n" +
                "                             Example: .canvas spawn 30 10 2.5 public\n" +
                "  • .canvas text (text) [text_color] [size] [bg_color] [public/private]\n" +
                "                             — Create a canvas with exact text proportions (8x8).\n" +
                "                             Example: .canvas text Hello! #00FF00 3 #FF0000 private\n" +
                "  • .canvas grab             — Grab a canvas with your gaze to move it (360°).\n" +
                "                             Re-entering the command locks the canvas in place.\n" +
                "  • .canvas url <base_size> <phys_size> <url>\n" +
                "                             — Download image/GIF, auto-adjusting proportions (Admins).\n" +
                "                             Example: .canvas url 40 5.0 https://imgur.com\n" +
                "  • .canvas delete         — Delete the canvas you are looking at (Owner or admin).\n" +
                "  • .canvas clearanims     — Delete ALL canvases with GIFs on the map (Admins only).\n" +
                "  • .canvas save <name>     — Save the drawing to a file (Admins only).\n" +
                "  • .canvas load <name>     — Load a saved canvas (Admins only).\n" +
                "  • .canvas permission <all/none/nickname> \n" +
                "                             — Configure spawn permissions for regular players (Admins only).\n\n" +
                "🖌️ [PAINTING TOOLS (Console)]:\n" +
                "  • .canvas color <HEX-code> — Set brush color and save to palette. Example: .canvas color #FF5733\n" +
                "  • .canvas pipette         — Copy the color of the pixel you are looking at (Pipette).\n" +
                "  • .canvas trust <nick/id>  — Grant/revoke access to your private canvases.\n" +
                "  • .canvas brush <1-5>    — Change the brush stroke size. Example: .canvas brush 3\n" +
                "  • .canvas eraser         — Toggle brush to ERASER mode and back.\n" +
                "  • .canvas undo / .redo   — Undo / redo the last action.\n" +
                "  • .canvas lang            — Switch plugin language (RU/EN).\n\n" +
                "🎮 [ITEM CONTROL (Artist Brush)]:\n" +
                "  • Toggle On / Off: Hold the Artist Brush and press 'G' (Drop item).\n" +
                "  • Look-to-Paint   : Hold the Artist Brush in your hands to paint on the canvas.\n" +
                "  • Cycle Colors : While the brush is active, hold LMB (Use item). Cycles custom HEX too!\n" +
                "  • Limitations          : Only the creator, admins, or whitelisted artists (.trust) \n" +
                "                             can paint or change GIFs on private canvases!\n" +
                "==================================================\n";

            enStrings["grab_desc"] = "Grab or release the canvas in front of you";
            enStrings["grab_release"] = "You have successfully released and locked the canvas!";
            enStrings["grab_need_brush"] = "You must hold the artist brush in your hands to grab the canvas!";
            enStrings["grab_look_at"] = "You must look at a canvas to grab it!";
            enStrings["grab_precision"] = "Aim more precisely at the canvas pixel!";
            enStrings["grab_already_holding"] = "This canvas is already being moved by another player!";
            enStrings["grab_private_error"] = "You cannot move this canvas because it is private!";
            enStrings["grab_success"] = "Canvas grabbed! It will follow your camera movements as long as you hold the brush.";

            enStrings["text_desc"] = "Create a canvas with text: .canvas text (text) [text_color] [phys_size] [bg_color] [public/private]";
            enStrings["text_too_long"] = "The text is too long and exceeds the matrix resolution limits!";
            enStrings["text_success"] = "Canvas with text successfully generated ({0}x{1} pixels, access: {2}).";
        }
    }
}


